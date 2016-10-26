/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class GrabbableObject : MonoBehaviour {

  public bool useAxisAlignment = false;
  public Vector3 rightHandAxis;
  public Vector3 objectAxis;

  public bool rotateQuickly = true;
  public bool centerGrabbedObject = false;

  public Rigidbody breakableJoint;
  public float breakForce;
  public float breakTorque;

  protected bool grabbed_ = false;
  protected bool hovered_ = false;

  private Quaternion defaultRot;
  private Vector3 defaultTrans;

	private BoxCollider bcollider;
	private Rigidbody rbody;

	void Awake()
	{
		rbody = GetComponent<Rigidbody>();
		rbody.useGravity = true;
		bcollider = GetComponent<BoxCollider>();
		bcollider.isTrigger = false;
		defaultRot = transform.rotation;
		defaultTrans = transform.position;
	}

  public bool IsHovered() {
    return hovered_;
  }

  public bool IsGrabbed() {
    return grabbed_;
  }

  public virtual void OnStartHover() {
    hovered_ = true;
//		rbody.isKinematic = true;
  }

  public virtual void OnStopHover() {
    hovered_ = false;
  }

  public virtual void OnGrab() {
    grabbed_ = true;
    hovered_ = false;
//		rbody.isKinematic = false;

    if (breakableJoint != null) {
      Joint breakJoint = breakableJoint.GetComponent<Joint>();
      if (breakJoint != null) {
        breakJoint.breakForce = breakForce;
        breakJoint.breakTorque = breakTorque;
      }
    }
  }

  public virtual void OnRelease() {
    grabbed_ = false;

    if (breakableJoint != null) {
      Joint breakJoint = breakableJoint.GetComponent<Joint>();
      if (breakJoint != null) {
        breakJoint.breakForce = Mathf.Infinity;
        breakJoint.breakTorque = Mathf.Infinity;
      }
    }
//		transform.rotation = defaultRot;
//		transform.position = defaultTrans;
//		rbody.isKinematic = true;
	}
}
