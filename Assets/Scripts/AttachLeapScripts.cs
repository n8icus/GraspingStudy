using UnityEngine;
using System.Collections;

public class AttachLeapScripts : MonoBehaviour {

    private GameObject[] pinchObjects;
    private GameObject[] pinchColliders;

    void Awake ()
    {
        pinchObjects = GameObject.FindGameObjectsWithTag("PinchObject");
        pinchColliders = GameObject.FindGameObjectsWithTag("PinchCollider");

//        foreach(GameObject obj in pinchObjects)
//        {
//            //Debug.Log(obj.gameObject.name);
//            if (obj.GetComponent<Leap.Unity.PinchUtility.NewLeapRTS>() == null)
//                obj.AddComponent<Leap.Unity.PinchUtility.NewLeapRTS>();
//        }

        foreach (GameObject col in pinchColliders)
        {
//            if (col.GetComponent<ColliderTrigger>() == null)
//                col.AddComponent<ColliderTrigger>();
			if (col.GetComponent<GrabbableObject> () == null)
				col.AddComponent<GrabbableObject> ();
			//if (col.GetComponent<SteamVR_InteractableObject> () == null) {
			//	col.AddComponent<SteamVR_InteractableObject> ();
			//	col.GetComponent<SteamVR_InteractableObject> ().isGrabbable = true;
			//}
		}
    }

}
