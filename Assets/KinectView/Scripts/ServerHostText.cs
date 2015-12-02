using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ServerHostText : MonoBehaviour {

	private const string KEY_SERVER = "key_server";

    public static string host;
	
	/*private void SubmitName(string arg0)
	{
		Debug.Log(arg0);
	}*/

	private InputField mainInputField;
	
	public void Start()
	{
		mainInputField = GetComponent<InputField>();

		string server = PlayerPrefs.GetString(KEY_SERVER, "");
		mainInputField.text = server;
		host = mainInputField.text;

		//Adds a listener to the main input field and invokes a method when the value changes.
		mainInputField.onValueChange.AddListener (delegate {ValueChangeCheck ();});
	}
	
	// Invoked when the value of the text field changes.
	public void ValueChangeCheck()
	{
		Debug.Log ("Host value Changed " + mainInputField.text);

		host = mainInputField.text;
		PlayerPrefs.SetString(KEY_SERVER, mainInputField.text);
	}
}
