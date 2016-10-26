using UnityEngine;
using System.Collections;

public class HandPhysicsControllerInput : MonoBehaviour
{
    private HandPhysicsController _handController;

	void Start ()
	{
	    _handController = GetComponent<HandPhysicsController>();
	}

    void FixedUpdate()
    {
        if (Camera.main != null)
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, _handController.HandParts[0][0].transform.position + new Vector3(0, 7, 6), Time.deltaTime * 15);
    }

	void Update () 
    {
        //Enable or disable control
	    if (Input.GetKeyDown(KeyCode.C))
	        _handController.EnableControl = !_handController.EnableControl;

        //Bend or unbend all fingers
        if (Input.GetMouseButtonDown(0))
        {
            _handController.BendAllFingers();
        }
        if (Input.GetMouseButtonUp(0))
        {
            _handController.UnbendAllFingers();
        }

        //Bend or unbend one specific finger
        if (Input.GetKeyDown(KeyCode.A))
            _handController.BendFinger(FingersType.Pinky);
        if (Input.GetKeyDown(KeyCode.S))
            _handController.BendFinger(FingersType.Ring);
        if (Input.GetKeyDown(KeyCode.D))
            _handController.BendFinger(FingersType.Middle);
        if (Input.GetKeyDown(KeyCode.F))
            _handController.BendFinger(FingersType.Index);
        if (Input.GetKeyDown(KeyCode.Space))
            _handController.BendFinger(FingersType.Thumb);

        if (Input.GetKeyUp(KeyCode.A))
            _handController.UnbendFinger(FingersType.Pinky);
        if (Input.GetKeyUp(KeyCode.S))
            _handController.UnbendFinger(FingersType.Ring);
        if (Input.GetKeyUp(KeyCode.D))
            _handController.UnbendFinger(FingersType.Middle);
        if (Input.GetKeyUp(KeyCode.F))
            _handController.UnbendFinger(FingersType.Index);
        if (Input.GetKeyUp(KeyCode.Space))
            _handController.UnbendFinger(FingersType.Thumb);
        
        //Change hand type
	    if (Input.GetKeyUp(KeyCode.G))
	    {
	        if (_handController.HandType == HandTyp.LeftHand)
                _handController.ChangeHandType(HandTyp.RightHand);
            else _handController.ChangeHandType(HandTyp.LeftHand);
	    }
        
        //Rotate forearm and wrist
        if (Input.GetMouseButton(1))
        {
            _handController.RotateForearm(Input.GetAxis("Mouse X"));
            _handController.RotateWrist(Input.GetAxis("Mouse Y"));
        }

        //Move forearm
        else _handController.MoveForearm(new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse ScrollWheel"), Input.GetAxis("Mouse Y")));

        Cursor.lockState = CursorLockMode.Locked;
	}
}
