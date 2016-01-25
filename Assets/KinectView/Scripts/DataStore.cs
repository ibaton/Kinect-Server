using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class DataStore {

	public DataStore ()	{}

	private static DataStore instance;
	List<GameObject> ohObjects = new List<GameObject>();
    List<OHobject> ohObjectData = new List<OHobject>();

    public static DataStore Instance {
		get {
			if(instance == null){
				instance = new DataStore();
			}
			return instance;
		}
	}

	public void AddOhObject(GameObject ohObject) {
		if(!ohObjects.Contains(ohObject)){
			ohObjects.Add(ohObject);
            ohObjectData.Add(ohObject.GetComponent<OHobject>());
        }
	}

    public void RemoveOhObject(GameObject ohObject)
    {
        ohObjectData.Remove(ohObject.GetComponent<OHobject>());
        ohObjects.Remove(ohObject);
        GameObject.Destroy(ohObject);
        Save();
    }

    public List<GameObject> GetObjects() {
        return ohObjects;
    }

    public List<OHobject> GetObjectData(){
        return ohObjectData;
    }

    public void Save() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Open(Application.persistentDataPath + "/OHObject.dat",
		                            FileMode.OpenOrCreate);

		List<OHObjectData> datas = new List<OHObjectData> ();

		foreach(GameObject ohGameObject in ohObjects){
			OHObjectData data = new OHObjectData();
			data.positionX = ohGameObject.transform.position.x;
			data.positionY = ohGameObject.transform.position.y;
			data.positionZ = ohGameObject.transform.position.z;
			data.size = ohGameObject.transform.localScale.x;

			OHobject ohObject = (OHobject) ohGameObject.GetComponent(typeof(OHobject));
			data.objectName = ohObject.ItemName;
			datas.Add(data);
		}
		
		bf.Serialize(file, datas);
		file.Close();

		Debug.Log("Saved " + datas.Count + " dataobjects");
	}

	public List<OHObjectData> LoadOHObject() {
		List<OHObjectData> data = new List<OHObjectData>();
		if(File.Exists(Application.persistentDataPath + "/OHObject.dat")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/OHObject.dat",
			                            FileMode.Open);

			try{
				data = (List<OHObjectData>)bf.Deserialize(file);
				file.Close();
			}
			catch(Exception e) {
				Debug.Log(e);
				file.Close();
				File.Delete(Application.persistentDataPath + "/OHObject.dat");
			}
		}
		Debug.Log("Loaded " + data.Count + " dataobjects");
		return data;
	}
}
