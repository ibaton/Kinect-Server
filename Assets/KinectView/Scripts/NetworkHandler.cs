using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class NetworkHandler : MonoBehaviour
{
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
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}
}

