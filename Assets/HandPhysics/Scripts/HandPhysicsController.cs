using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Leap.Unity;
using Leap;


public enum FingersType : byte
{
    Thumb = 1, Index, Middle, Ring, Pinky
}

public enum HandTyp
{
    LeftHand, RightHand
}

[Serializable]
public class PositionLimit
{
    public bool EnableLimits;
    public Vector3 MinPosition = new Vector3(-50, 2, -50);
    public Vector3 MaxPosition = new Vector3(50, 20, 50);
}

public class HandPhysicsController : MonoBehaviour
{
    public HandTyp HandType = HandTyp.LeftHand;
    
    public bool EnableControl = true;

    public float ForearmMovementSpeedXZ = 5;
    public float ForearmMovementSpeedY = 50;
    public float ForearmRotationSpeed = 1;
    public float WristRotationSpeed = 0.75f;
    public float FingersBendingSpeed = 3;
    public float FingersUnbendingSpeed = 4;

	public float activateGripStrength = 0.6f;
	public float deactivateGripStrength = 0.5f;

    public PositionLimit PositionLimits; //Global position limits for forearm movement

    public HandPart[][] HandParts; //Links to all hand bones
    /*
	HandParts[0][0] - Forearm
	HandParts[0][1] - Wrist
	HandParts[1][0, 1, 2] - Thumb bones
	HandParts[1][0, 1, 2] - Index bones
	HandParts[1][0, 1, 2] - Middle bones
	HandParts[1][0, 1, 2] - Ring bones
	HandParts[1][0, 1, 2] - Pinky bones	
	*/

	//public SteamVR_ControllerEvents viveController;

	public bool useControllers;

	private LeapServiceProvider provider;
	private LeapHandController controller;
	private HandModel hm; 

    [HideInInspector] 
    public bool ObjectAttached; //Is any rigidbody object attached to hand?

    private ConfigurableJoint _forearmJoint;
    private ConfigurableJoint _wristJoint;

    private Vector3 _forearmMovement;
    private float _forearmTargetRotation;
    private Transform _forearmRotDummy;
    private float _wristTargetRotation;

    private GameObject _objectToAttach;


    void Awake()
    {
        InitHand();
        ChangeHandType(HandType);

		controller = FindObjectOfType<LeapHandController>();
		provider = FindObjectOfType<LeapServiceProvider>();
    }

    void InitHand() //Initialize ang configure bones of this hand
    {
        //Initalize HandPart array
        #region InitArray

        HandParts = new HandPart[6][];

        HandParts[0] = new HandPart[2];
        for (int i = 1; i < HandParts.Length; i++)
        {
            HandParts[i] = new HandPart[3];
        }
        #endregion

        //Find all hand parts by name of this transform
        #region HandPartsFinding

        HandParts[0][0] = transform.Find("Forearm").gameObject.AddComponent<HandPart>();
        HandParts[0][1] = HandParts[0][0].transform.Find("Wrist").gameObject.AddComponent<HandPart>();
        _forearmJoint = HandParts[0][0].GetComponent<ConfigurableJoint>();
        _wristJoint = HandParts[0][1].GetComponent<ConfigurableJoint>();

        for (int i = 1; i < HandParts.Length; i++)
        {
            FingersType fingerType = (FingersType)i;

            HandParts[i][0] = HandParts[0][1].transform.Find(fingerType.ToString() + "0").gameObject.AddComponent<HandPart>();
            HandParts[i][0].IsRoot = true;

            for (int j = 1; j < HandParts[i].Length; j++)
            {
                HandParts[i][j] = HandParts[i][j - 1].transform.Find(fingerType.ToString() + j).gameObject.AddComponent<HandPart>();

                HandParts[i][j].PrevFingerBone = HandParts[i][j - 1];
                HandParts[i][j - 1].NextFingerBone = HandParts[i][j];
            }
        }
        #endregion

        //Create collision detectors for all fingers
        #region CollisionDetectors

        for (int i = 1; i < HandParts.Length; i++)
        {
            FingersType fingerType = (FingersType)i;

            for (int j = 0; j < HandParts[i].Length; j++)
            {
                if (j == 0)
                    continue;
                GameObject collisionDetector = (GameObject)Instantiate(HandParts[i][j].gameObject);
                Destroy(collisionDetector.GetComponent<HandPart>());

                var children = new List<GameObject>();
                foreach (Transform child in collisionDetector.transform) children.Add(child.gameObject);
                children.ForEach(child => Destroy(child));

                collisionDetector.name = fingerType.ToString() + j + "_colDetector";
                collisionDetector.transform.parent = HandParts[i][j].transform;
                collisionDetector.transform.localPosition = Vector3.zero;
                collisionDetector.transform.localRotation = Quaternion.identity;

                CapsuleCollider collider = collisionDetector.GetComponent<CapsuleCollider>();
                collider.isTrigger = true;
                collider.radius *= 0.05f;
//                collider.center = new Vector3(-collider.center.x, 0, collider.center.z + 0.05f);
				collider.center = Vector3.zero;
                collider.height *= 0.09f;

                collisionDetector.AddComponent<FingerCollisionDetector>();
                collisionDetector.GetComponent<FingerCollisionDetector>().ThisHandPart = HandParts[i][j];

            }
        }
        #endregion

        //Set target local rotation for all fingers
        #region SetTargetRotation

        HandParts[1][0].TargetRotation = Quaternion.Euler(300, -15, 315);
        HandParts[1][1].TargetRotation = Quaternion.Euler(-10, 315, 12.5f);
        HandParts[1][2].TargetRotation = Quaternion.Euler(5, 320, 10);

        float indexLocalRotationX = -5;
        float indexLocalRotationZ = 8;
        
        for (int i = 2; i < HandParts.Length; i++)
        {
            for (int j = 0; j < HandParts[i].Length; j++)
            {
                if (j == 0)
                    HandParts[i][j].TargetRotation = Quaternion.Euler(indexLocalRotationX, -70, indexLocalRotationZ);
                else HandParts[i][j].TargetRotation = Quaternion.Euler(0, -60, 0);
            }
            indexLocalRotationX += 2.5f;
            indexLocalRotationZ -= 3f;
        }
        #endregion

        //Ignore collisions between all hand parts
        #region IgnoreCollisions

        foreach (var handPartParent in HandParts)
        {
            foreach (var handPart in handPartParent)
            {
                for (int i = 0; i < HandParts.Length; i++)
                {
                    for (int j = 0; j < HandParts[i].Length; j++)
                    {
                        if (HandParts[i][j] != handPart)
                            Physics.IgnoreCollision(HandParts[i][j].GetComponent<Collider>(), handPart.GetComponent<Collider>());
                    }

                }

            }
        }
        #endregion

//		HandParts[0][0].transform.pos

    }

	public void UpdateLeap()
	{		
		Frame currentFrame = provider.CurrentFrame;
		if (!useControllers) {
			foreach (Hand hand in currentFrame.Hands) {
				if (hand.IsLeft && HandType == HandTyp.LeftHand) {
//				_wristTargetRotation = hand.Direction.Pitch * -1;
//				RotateWrist(hand.Direction.Pitch * -1);
//				HandParts[0][1].transform.rotation = new Quaternion(0, hand.Direction.Pitch*-1, 0, 1);
//				Debug.Log(hand.Direction.Pitch);
					foreach (Finger finger in hand.Fingers) {
						if (finger.Type == Finger.FingerType.TYPE_INDEX && !finger.IsExtended) {
							BendFinger (FingersType.Index);
						} else if(finger.Type == Finger.FingerType.TYPE_INDEX && finger.IsExtended){
							UnbendFinger (FingersType.Index);
						}
						if (finger.Type == Finger.FingerType.TYPE_MIDDLE && !finger.IsExtended) {
							BendFinger (FingersType.Middle);
						} else if(finger.Type == Finger.FingerType.TYPE_MIDDLE && finger.IsExtended){
							UnbendFinger (FingersType.Middle);
						}
						if (finger.Type == Finger.FingerType.TYPE_RING && !finger.IsExtended) {
							BendFinger (FingersType.Ring);
						} else if(finger.Type == Finger.FingerType.TYPE_RING && finger.IsExtended){
							UnbendFinger (FingersType.Ring);
						}
						if (finger.Type == Finger.FingerType.TYPE_PINKY && !finger.IsExtended) {
							BendFinger (FingersType.Pinky);
						} else if(finger.Type == Finger.FingerType.TYPE_PINKY && finger.IsExtended){
							UnbendFinger (FingersType.Pinky);
						}
						if (finger.Type == Finger.FingerType.TYPE_THUMB && !finger.IsExtended) {
							BendFinger (FingersType.Thumb);
						} else if(finger.Type == Finger.FingerType.TYPE_THUMB && finger.IsExtended){
							UnbendFinger (FingersType.Thumb);
						}
					}
//					if (hand.GrabStrength > activateGripStrength) {
//						BendAllFingers ();
//					} else if (hand.GrabStrength < deactivateGripStrength) {
//						UnbendAllFingers ();
//					}
				} else if (hand.IsRight && HandType == HandTyp.RightHand) {
					foreach (Finger finger in hand.Fingers) {
						if (finger.Type == Finger.FingerType.TYPE_INDEX && !finger.IsExtended) {
							BendFinger (FingersType.Index);
						} else if(finger.Type == Finger.FingerType.TYPE_INDEX && finger.IsExtended){
							UnbendFinger (FingersType.Index);
						}
						if (finger.Type == Finger.FingerType.TYPE_MIDDLE && !finger.IsExtended) {
							BendFinger (FingersType.Middle);
						} else if(finger.Type == Finger.FingerType.TYPE_MIDDLE && finger.IsExtended){
							UnbendFinger (FingersType.Middle);
						}
						if (finger.Type == Finger.FingerType.TYPE_RING && !finger.IsExtended) {
							BendFinger (FingersType.Ring);
						} else if(finger.Type == Finger.FingerType.TYPE_RING && finger.IsExtended){
							UnbendFinger (FingersType.Ring);
						}
						if (finger.Type == Finger.FingerType.TYPE_PINKY && !finger.IsExtended) {
							BendFinger (FingersType.Pinky);
						} else if(finger.Type == Finger.FingerType.TYPE_PINKY && finger.IsExtended){
							UnbendFinger (FingersType.Pinky);
						}
						if (finger.Type == Finger.FingerType.TYPE_THUMB && !finger.IsExtended) {
							BendFinger (FingersType.Thumb);
						} else if(finger.Type == Finger.FingerType.TYPE_THUMB && finger.IsExtended){
							UnbendFinger (FingersType.Thumb);
						}
					}
//					if (hand.GrabStrength > activateGripStrength) {
//						BendAllFingers ();
//					} else if (hand.GrabStrength < deactivateGripStrength) {
//						UnbendAllFingers ();
//					}
				}
			}
		} else {
			//if (viveController.triggerPressed) {
			//	BendAllFingers ();
			//} else if (viveController.touchpadPressed) {
			//	BendAllFingers ();
			//	UnbendFinger (FingersType.Index);
			//} else{
			//	UnbendAllFingers ();
			//}
		}
	}
    public void MoveForearm(Vector3 direction)
    {
        if (!EnableControl)
            return;

        _forearmMovement += direction;
    }

    public void RotateForearm(float direction)
    {
        if (!EnableControl)
            return;

        float _rotMultiplier = _forearmTargetRotation <- 1 ? Math.Abs(_forearmTargetRotation * 1.5f): 1;
        
        _forearmTargetRotation += (direction * Time.deltaTime) * (ForearmRotationSpeed * _rotMultiplier);
        _forearmTargetRotation = Mathf.Clamp(_forearmTargetRotation, -3.1f, 1.4f);
    }

    public void RotateWrist(float direction)
    {
        if (!EnableControl)
            return;

        _wristTargetRotation += (direction*Time.deltaTime)*WristRotationSpeed;
        _wristTargetRotation = Mathf.Clamp(_wristTargetRotation, -0.6f, 0.6f);
    }
    
    public void BendFinger(FingersType fingerType)
    {
        if (!EnableControl)
            return;

        byte fingerParentId = (byte) fingerType;
        
        for (int i = 0; i < HandParts[fingerParentId].Length; i++)
        {
            HandParts[fingerParentId][i].StartBending();
        }
    }

    public void UnbendFinger(FingersType fingerType)
    {
        if (!EnableControl)
            return;

        byte fingerParentId = (byte)fingerType;
        
        for (int i = 0; i < HandParts[fingerParentId].Length; i++)
        {
            HandParts[fingerParentId][i].StopBending();
        }
    }

    public void BendAllFingers()
    {
        if (!EnableControl)
            return;

        foreach (var handPartRoot in HandParts)
        {
            foreach (var handPart in handPartRoot)
            {
                handPart.StartBending();
            }
        }
    }

    public void UnbendAllFingers()
    {
        if (!EnableControl)
            return;

        foreach (var handPartRoot in HandParts)
        {
            foreach (var handPart in handPartRoot)
            {
                handPart.StopBending();
            }
        }
    }

    public void ChangeHandType(HandTyp handType)
    {
        if (handType == HandTyp.LeftHand)
        {
//            HandParts[0][0].transform.lossyScale = new Vector3(-1, 1, 1);
        }
        else
        {
//            HandParts[0][0].transform.localScale = new Vector3(1, 1, 1);
        }
        HandType = handType;
    }

    public GameObject GetObjectInHands()
    {
        if (_objectToAttach != null)
            return _objectToAttach;
        return null;
    }

    void FixedUpdate()
    {
        //Bend or unbend fingers based on user input
        #region FingersBending
        for (int i = 1; i < HandParts.Length; i++)
        {
            foreach (var finger in HandParts[i])
            {
                if (!finger.IsTouchedObject)
                {
                    if (finger.IsBending)
                    {
                        Quaternion localRot = finger.transform.localRotation;
                        localRot = Quaternion.Lerp(localRot, finger.TargetRotation, Time.deltaTime * FingersBendingSpeed);
                        finger.transform.localRotation = localRot;
                    }
                }
                
                if (!finger.IsBending)
                {
                    Quaternion localRot = finger.transform.localRotation;
                    localRot = Quaternion.Lerp(localRot, finger.DefaultRotation, Time.deltaTime * FingersUnbendingSpeed);
                    finger.transform.localRotation = localRot;

                    if (finger.IsHoldingObject)
                    {
                        finger.IsHoldingObject = false;
                        DetachObject();
                    }
                        
                }
            }
        }
        #endregion

        //Grab object with non-kinematic rigidbody component by checking fingers collisions
        #region ObjectGrabbing

        if (!ObjectAttached)
        {
            if (CheckIfCanAttach())
            {
                if (_objectToAttach != null)
                {
                    AttachObject(_objectToAttach.GetComponent<Rigidbody>());
                }
            }
        }
        else
        {
            if (_objectToAttach == null)
            {
                DetachObject();
            }
        }

        #endregion

        //Move and rotate forearm and wrist based on user input
        #region MovementAndRotation
        //Move forearm based _forearmMovement value
//        _forearmMovement = Vector3.Lerp(_forearmMovement, Vector3.zero, Time.deltaTime * 2);
        
//        HandParts[0][0].GetComponent<Rigidbody>().AddForce(new Vector3(_forearmMovement.x * Time.deltaTime * (ForearmMovementSpeedXZ * 4000), _forearmMovement.y * Time.deltaTime * (ForearmMovementSpeedY * 4000), _forearmMovement.z * Time.deltaTime * (ForearmMovementSpeedXZ * 4000)));
        //Rotate forearm based on _forearmTargetRotation value
//        _forearmJoint.targetRotation = new Quaternion(_forearmTargetRotation, 0, 0, 1);
        
        //Rotate wrist based on _wristTargetRotation value
//        _wristJoint.targetRotation = new Quaternion(_wristTargetRotation, 0, 0, 1);
        
        #endregion
    }
    void Update()
    {

		UpdateLeap ();
        //Set position when forearm is beyond the limits
        #region PositionLimits
        if (PositionLimits.EnableLimits)
        {
            Vector3 forearmPos = HandParts[0][0].transform.position;

            if (forearmPos.x < PositionLimits.MinPosition.x)
                forearmPos.x = PositionLimits.MinPosition.x;
            if (forearmPos.y < PositionLimits.MinPosition.y)
                forearmPos.y = PositionLimits.MinPosition.y;
            if (forearmPos.z < PositionLimits.MinPosition.z)
                forearmPos.z = PositionLimits.MinPosition.z;

            if (forearmPos.x > PositionLimits.MaxPosition.x)
                forearmPos.x = PositionLimits.MaxPosition.x;
            if (forearmPos.y > PositionLimits.MaxPosition.y)
                forearmPos.y = PositionLimits.MaxPosition.y;
            if (forearmPos.z > PositionLimits.MaxPosition.z)
                forearmPos.z = PositionLimits.MaxPosition.z;

            HandParts[0][0].transform.position = forearmPos;
        }
        #endregion

    }

    bool CheckIfCanAttach()
    {
        bool thumbIsReady = false;
        List<GameObject> thumbCollidedObjects = new List<GameObject>();
        for (int i = 0; i < HandParts[1].Length; i++)
        {
            if (HandParts[1][i].IsTouchedObject && HandParts[1][i].IsBending)
            {
                HandParts[1][i].IsHoldingObject = true;
                thumbIsReady = true;
            }
            thumbCollidedObjects.AddRange(HandParts[1][i].CollidedObjects);
        }

        if (!thumbIsReady)
        {
            foreach (var thumb in HandParts[1])
            {
                thumb.IsHoldingObject = false;
            }
            return false;
        }

        for (int i = 2; i < HandParts.Length; i++)
        {
            for (int j = 1; j < HandParts[i].Length; j++)
            {
                if (HandParts[i][j].IsTouchedObject && HandParts[i][j].IsBending)
                {
                    foreach (var collidedObject in HandParts[i][j].CollidedObjects)
                    {
                        if (thumbCollidedObjects.Contains(collidedObject))
                        {
                            HandParts[i][j].IsHoldingObject = true;
                            _objectToAttach = collidedObject;
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    void AttachObject(Rigidbody rb)
    {
        foreach (var handPartRoots in HandParts)
        {
            foreach (var handPart in handPartRoots)
            {
                Physics.IgnoreCollision(handPart.GetComponent<Collider>(), rb.GetComponent<Collider>());
            }
        }

        HandParts[0][1].gameObject.AddComponent<FixedJoint>();
        HandParts[0][1].gameObject.GetComponent<FixedJoint>().connectedBody = rb;
//        HandParts[0][1].gameObject.GetComponent<FixedJoint>().enableCollision = true;
        ObjectAttached = true;
    }

    void DetachObject()
    {
        if (_objectToAttach != null)
        {
            foreach (var handPartRoots in HandParts)
            {
                foreach (var handPart in handPartRoots)
                {
                    Physics.IgnoreCollision(handPart.GetComponent<Collider>(), _objectToAttach.GetComponent<Collider>(), false);
                }
            }
        }
        
        Destroy(HandParts[0][1].GetComponent<FixedJoint>());
        ObjectAttached = false;
        _objectToAttach = null;
    }
    
}
