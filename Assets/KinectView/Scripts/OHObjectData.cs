using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class OHObjectData
{
	public float positionX;
	public float positionY;
	public float positionZ;
	public float size;

	public string objectName = "";

	public OHObjectData ()
	{
	}

	public OHObjectData (UnityEngine.Vector3 position, float size, string objectName)
	{
		this.positionX = position.x;
		this.positionY = position.y;
		this.positionZ = position.z;
		this.size = size;
	}
	
}

