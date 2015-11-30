using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class NetworkHandler : MonoBehaviour
{

	public static string host;
	public static NetworkHandler instance;

	public static NetworkHandler getInstance(){
		if(instance == null) {
			instance = new NetworkHandler();
		}
		return instance;
	}


	public WWW SendCommand(string command, string itemName)
	{
		//https://home.3hou.se:8443
		string url = host + "/rest/items/" + itemName; //"hue_LCT001_001788184dbe_1_color";
		
		var encoding = new System.Text.UTF8Encoding();
		var postHeader = new Dictionary<String, String>();
		
		postHeader ["Content-Type"] = "text/plain";
		postHeader["Content-Length"] = ""+ command.Length;
		
		Debug.Log("Sent: " + command + " To: " + url);
		WWW www = new WWW(url, encoding.GetBytes(command), postHeader);
		
		StartCoroutine(WaitForRequest(www));
		return www; 
	}
	
	private IEnumerator WaitForRequest(WWW www)
	{
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

