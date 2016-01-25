using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private const float SIZE_BONE = 2f;
    private const float SIZE_JOINT = 1f;

    private const float POINT_ANGLE = 140f;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

	public GameObject HandTracker;
    
	private Dictionary<GameObject, TrackingPoints> _TrackerMap = new Dictionary<GameObject, TrackingPoints>();

    void Update () 
    {
        if (BodySourceManager == null) {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null) {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null) {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data) {
            if (body == null) {
            	continue;
          	}
                
            if(body.IsTracked) {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds) {
            if(!trackedIds.Contains(trackingId)) {
				GameObject bodyObject = _Bodies[trackingId];

				foreach(GameObject tracker in _TrackerMap[bodyObject].TrackerMap.Values) {
					Destroy(tracker);
				}
				_TrackerMap.Remove(bodyObject);


				Destroy(bodyObject);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data) {
            if (body == null) {
                continue;
            }
            
            if(body.IsTracked) {
                if(!_Bodies.ContainsKey(body.TrackingId)) {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

		TrackingPoints tracker = new TrackingPoints ();
		_TrackerMap[body] = tracker;

		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++) {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
			if(jt == Kinect.JointType.WristLeft || jt == Kinect.JointType.WristRight) {
				GameObject cube = Instantiate(HandTracker);
				tracker.TrackerMap[jt] = cube;
			}
			lr.SetWidth(SIZE_BONE, SIZE_BONE);
            
            jointObj.transform.localScale = new Vector3(SIZE_JOINT, SIZE_JOINT, SIZE_JOINT);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject) {
		TrackingPoints tracker = _TrackerMap[bodyObject];

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++) {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt)) {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue) {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            } else {
                lr.enabled = false;
            }

            if (tracker.TrackerMap.ContainsKey(jt)) {
				lr.SetWidth(1.0f, 1.0f);

                Kinect.Joint wristJoint;
                Kinect.Joint elbowJoint;
                Kinect.Joint sholderJoint;

                if (jt == Kinect.JointType.WristRight) {
                    wristJoint = body.Joints[Kinect.JointType.WristRight];
                    elbowJoint = body.Joints[Kinect.JointType.ElbowRight];
                    sholderJoint = body.Joints[Kinect.JointType.ShoulderRight];
                }
                else {
                    wristJoint = body.Joints[Kinect.JointType.WristLeft];
                    elbowJoint = body.Joints[Kinect.JointType.ElbowLeft];
                    sholderJoint = body.Joints[Kinect.JointType.ShoulderLeft];
                }

                Vector3 elbowPosition = new Vector3(elbowJoint.Position.X, elbowJoint.Position.Y, elbowJoint.Position.Z);
                Vector3 sholderPosition = new Vector3(sholderJoint.Position.X, sholderJoint.Position.Y, sholderJoint.Position.Z);
                Vector3 wristPosition = new Vector3(wristJoint.Position.X, wristJoint.Position.Y, wristJoint.Position.Z);

                Vector3 previousVector = sholderPosition - elbowPosition;
                Vector3 nextVector = wristPosition - elbowPosition;

                float angle = Vector3.Angle(previousVector, nextVector);
                bool isActive = angle > POINT_ANGLE;

                GameObject handCollider = tracker.TrackerMap[jt];
                if (handCollider.activeSelf != isActive) {
                    handCollider.SetActive(isActive);
                }

                handCollider.transform.position = jointObj.localPosition;

				Vector3 previousJoinPosition = GetVector3FromJoint(targetJoint.Value);
				handCollider.transform.LookAt(previousJoinPosition);

				handCollider.transform.position -= handCollider.transform.forward * (handCollider.transform.localScale.z/2);
			}
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state) {
        switch (state) {
            case Kinect.TrackingState.Tracked:
                return Color.green;
            case Kinect.TrackingState.Inferred:
                return Color.red;
            default:
                return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint) {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

	class TrackingPoints {
		public Dictionary<Kinect.JointType, GameObject> TrackerMap = new Dictionary<Kinect.JointType, GameObject>();
	}
}
