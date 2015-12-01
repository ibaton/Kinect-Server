using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class ObjectLoader : MonoBehaviour
{
    private UnityEngine.Object OHObjectRef;

    void Start()
    {
        OHObjectRef = Resources.Load("OHItemObject");

        List<OHObjectData> ohDatas = DataStore.Instance.LoadOHObject();
        foreach (OHObjectData data in ohDatas)
        {
            transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
            transform.localScale = new Vector3(data.size, data.size, data.size);

            GameObject ohObject = Instantiate(OHObjectRef) as GameObject;
            DataStore.Instance.AddOhObject(ohObject);

            ohObject.transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
            ohObject.transform.localScale = new Vector3(data.size, data.size, data.size);
        }
        Debug.Log("Data objects loaded");
    }

    public void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            GameObject ohObject = Instantiate(OHObjectRef) as GameObject;
            ohObject.transform.position = new Vector3(0, 0, 0);
            DataStore.Instance.AddOhObject(ohObject);
            Debug.Log("Added new oh object");
        }
    }

    public void OnDestroy()
    {
        DataStore.Instance.Save();
    }


    public void OnApplicationQuit()
    {
        DataStore.Instance.Save();
    }

}

