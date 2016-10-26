using UnityEngine;
using System.Collections;
using Leap;
namespace Leap.Unity {
  public class SphereGrab : MonoBehaviour {
    public Leap.Unity.GrabDetector PinchDetector;

    public Leap.Unity.IHandModel _handModel;

    Collider[] colliders;
    Hand leapHand;
    GameObject Marker;

    [SerializeField]
    private Mesh _sphereMesh;
    [SerializeField]
    private Material _sphereMaterial;

    Vector3 curPos;
    Vector3 prevPos;
    Quaternion curQuat;
    Quaternion prevQuat;

    bool isGrabbing = false;
    Rigidbody grabbedObject;

    void Start() {
      /*
      Marker = new GameObject("Marker");
      Marker.transform.localPosition = Vector3.zero;
      Marker.transform.localRotation = Quaternion.identity;
      Marker.transform.localScale = Vector3.one * 0.06f;
      Marker.AddComponent<MeshFilter>().mesh = _sphereMesh;
      Marker.AddComponent<MeshRenderer>().sharedMaterial = _sphereMaterial;
      */
      curPos = Vector3.zero;
      prevPos = Vector3.zero;

      curQuat = Quaternion.identity;
      prevQuat = Quaternion.identity;
    }

    // Update is called once per frame
    void Update() {
      leapHand = _handModel.GetLeapHand();
      curPos = (leapHand.Fingers[2].Bone(Bone.BoneType.TYPE_PROXIMAL).PrevJoint + (leapHand.PalmNormal * 0.04f)).ToVector3();
      curQuat = leapHand.Basis.CalculateRotation();
      if (PinchDetector.DidStartPinch) {
        colliders = Physics.OverlapSphere(curPos, 0.025f);
        //Marker.transform.position = curPos;
        foreach (Collider col in colliders) {
          if (col.GetComponent<Rigidbody>()) {
            col.gameObject.GetComponent<Renderer>().material.color = Color.green;
            col.transform.parent = PinchDetector.transform;
            grabbedObject = col.GetComponent<Rigidbody>();
            grabbedObject.isKinematic = true;
            isGrabbing = true;
          }
        }
        //Marker.GetComponent<Renderer>().material.color = Color.green;
      } else if (PinchDetector.DidEndPinch) {
        foreach (Collider col in colliders) {
          if (col.GetComponent<Rigidbody>()) {
            col.gameObject.GetComponent<Renderer>().material.color = Color.white;
            col.transform.parent = null;
            grabbedObject.isKinematic = false;
            grabbedObject.useGravity = true;
            grabbedObject.freezeRotation = false;
            grabbedObject.velocity = (curPos-prevPos)/Time.deltaTime;

            //Quaternion workingQuat = curQuat;
            //float dot = Quaternion.Dot(prevQuat, curQuat);
            //if (dot > 0f) { workingQuat = new Quaternion(-workingQuat.x, -workingQuat.y, -workingQuat.z, -workingQuat.w); }
            //Vector3 axis; float angle;
            //Quaternion localQuat = prevQuat * Quaternion.Inverse(workingQuat);
            //localQuat.ToAngleAxis(out angle, out axis);
            //axis *= angle;
            //if ((axis / Time.deltaTime).x != Mathf.Infinity && (axis / Time.deltaTime).x != Mathf.NegativeInfinity && float.IsNaN((axis / Time.fixedDeltaTime).x)) {
              //grabbedObject.angularVelocity = (axis / Time.deltaTime)/2f;
            //}
            grabbedObject = null;
            isGrabbing = false;
          }
        }
        //Marker.GetComponent<Renderer>().material.color = Color.white;
      }

      if (isGrabbing) {
        grabbedObject.useGravity = false;
        grabbedObject.freezeRotation = true;
        grabbedObject.velocity = (curPos - prevPos) / Time.deltaTime;
      }

      prevPos = curPos;
      prevQuat = curQuat;
    }
  }
}