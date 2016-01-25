using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Mono.Zeroconf;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

public class NetworkHandler : MonoBehaviour
{
    private RegisterService service;
    private TcpListener server;
    private Thread serverThread;
    private bool running = true;

    private List<Queue<string>> messageQueues = new List<Queue<string>>();

    private const int SERVER_PORT = 10993;

    public void Start()
    {
        StartServer();
        RegisterZeroConf();
    }

    public void OnDestroy()
    {
        StopServer();
        DeRegisterZeroConf();
    }

    public void SendCommand(string host, string itemName, string command)
    {
        string url = host + "/rest/items/" + itemName;

        StartCoroutine(WaitForRequest(url, command));
    }

    private IEnumerator WaitForRequest(String url, String command)
    {
        var encoding = new System.Text.UTF8Encoding();
        var postHeader = new Dictionary<String, String>();

        postHeader["Content-Type"] = "text/plain";
        postHeader["Content-Length"] = "" + command.Length;

        Debug.Log("Sent: " + command + " To: " + url);
        WWW www = new WWW(url, encoding.GetBytes(command), postHeader);
        yield return www;

        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    /// <summary>
    /// Start kinect service listening for all ips.
    /// </summary>
    private void StartServer()
    {
        Debug.Log("Server starting");
        IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
        server = new TcpListener(ipAddress, SERVER_PORT);
        server.Start();
        serverThread = new Thread(ServerRunner);
        serverThread.Start();
    }

    public void ServerRunner() {
        while (running) {
            TcpClient client = server.AcceptTcpClient();
            new Thread(() => ClientRunner(client)).Start();
        }
    }

    public void PublishItemList() {
        string itemListMessage = CreateItemListMessage();
        foreach (Queue<string> messages in messageQueues) {
            messages.Enqueue(itemListMessage);
        }
        Debug.Log("Publishing item list");
    }

    public void ClientRunner(TcpClient client) {
        NetworkStream ns = client.GetStream();
        StreamReader sr = new StreamReader(ns);

        Queue<string> messageQueue = new Queue<string>();
        messageQueues.Add(messageQueue);
        messageQueue.Enqueue(CreateItemListMessage());
        new Thread(() => OutRunner(client, messageQueue)).Start();

        while (running && client.Connected) {
            string jMessage = sr.ReadLine();
            if (jMessage.Length == 0) {
                break;
            }

            JSONObject serializedObject = new JSONObject(jMessage);
            int messageType = -1;
            serializedObject.GetField(out messageType, "mtype", -1);
            Debug.Log("Type: " + messageType + " Message: " + serializedObject);

            if (messageType == 1) {
                messageQueue.Enqueue(CreateItemListMessage());
            }
        }

        if (client.Connected) {
            client.Close();
        }
        Debug.Log("Client disconnected");
    }

    public void OutRunner(TcpClient client, Queue<string> messageQueue)
    {
        NetworkStream ns = client.GetStream();
        StreamWriter sw = new StreamWriter(ns);

        while (running && client.Connected)
        {
            while (messageQueue.Count > 0) {
                string outData = messageQueue.Dequeue();
                Debug.Log("message " + outData.ToString());
                sw.WriteLine(outData);
                sw.Flush();
            }
            Thread.Sleep(50);
        }
    }

    public string CreateItemListMessage()
    {
        OHKMessage message = new OHKMessage(1);
 
        List<OHobject> ohObjects = DataStore.Instance.GetObjectData();

        JSONObject outData = new JSONObject(JSONObject.Type.OBJECT);
        outData.AddField("mtype", 1);
        outData.AddField("size", ohObjects.Count);

        JSONObject jItems = new JSONObject(JSONObject.Type.ARRAY);
        foreach (OHobject itemData in ohObjects)
        {
            JSONObject item = new JSONObject(JSONObject.Type.OBJECT);
            item.AddField("name", itemData.ItemName);
            item.AddField("state", itemData.State);
            jItems.Add(item);
        }
        outData.AddField("items", jItems);
        return outData.ToString();
    }

    public void OnClientConnected(NetworkMessage netMsg) {
        Debug.Log("Client connected");
    }

    public void OnClientDisconnected(NetworkMessage netMsg) {
        Debug.Log("Client disconnected");
    }

    private void StopServer()
    {
        running = false;
        server.Stop();
        foreach (Queue<string> messages in messageQueues) {
            messages.Clear();
        }
    }
    
    /// <summary>
    /// Start broadcasting service using zeroconf.
    /// </summary>
    private void RegisterZeroConf()
    {
        Debug.Log("Registering Zeroconf");
        service = new RegisterService();
        service.Name = "Openhab Kinect";
        service.RegType = "_openhab-kinect._tcp";
        service.ReplyDomain = "local.";
        service.Port = SERVER_PORT;

        service.Register();
        Debug.Log("Registered Zeroconf " + service.RegType);

        Mono.Zeroconf.ServiceBrowser browser = new Mono.Zeroconf.ServiceBrowser();
        browser.ServiceAdded += delegate (object o, Mono.Zeroconf.ServiceBrowseEventArgs args)
        {
            Debug.Log("Found Service: " + args.Service.Name);
        };
        browser.Browse("_openhab-kinect._tcp", "local");
    }

    
    /// <summary>
    /// De register zeroconf service
    /// </summary>
    private void DeRegisterZeroConf()
    {
        if (service != null) { 
            service.Dispose();
        }
        Debug.Log("Deregistered Zeroconf");
    }

    private class OHKMessage {
        public int MType { get; set; }

        public OHKMessage(int mtype)
        {
            MType = mtype;
        }
    }
}

