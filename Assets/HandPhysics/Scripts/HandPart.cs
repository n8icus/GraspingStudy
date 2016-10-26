using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HandPart : MonoBehaviour
{
    public bool IsBending;
    public bool IsTouchedObject;

    public bool IsRoot;
    public bool IsHoldingObject;

    public HandPart NextFingerBone;
    public HandPart PrevFingerBone;

    public Quaternion TargetRotation;
    public Quaternion DefaultRotation;

    public List<GameObject> CollidedObjects;
    
    void Awake()
    {
        DefaultRotation = transform.localRotation;
        CollidedObjects = new List<GameObject>();
    }
    
    public void StartBending()
    {
        IsBending = true;
    }

    public void StopBending()
    {
        IsBending = false;
        IsTouchedObject = false;
    }

    public void TouchObject(GameObject obj)
    {
        IsTouchedObject = true;

        if (PrevFingerBone != null)
        {
            PrevFingerBone.TouchObject(obj);
        }
    }


}
