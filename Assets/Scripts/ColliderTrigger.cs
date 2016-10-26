using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ColliderTrigger : MonoBehaviour {

    private bool _triggered;
    private Rigidbody rbody;
    private BoxCollider bcollider;
    void Awake()
    {
        _triggered = false;
        rbody = GetComponent<Rigidbody>();
//        rbody.useGravity = true;
		rbody.mass = 5;
        bcollider = GetComponent<BoxCollider>();
        bcollider.isTrigger = false;
    }

	void OnTriggerEnter(Collider other)
    {
        _triggered = true;
//		Debug.Log ("Triggered!");
    }

//	void OnTriggerExit(Collider other)
//	{
//		_triggered = false;
//	}
//

    public bool isTriggered()
    {
        return _triggered;
    }

    public void setTriggerOff()
    {
        _triggered = false;
    }

	public void setTriggerOn()
	{
		_triggered = true;
	}
}
