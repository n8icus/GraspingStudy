//using UnityEngine;
//
//namespace Leap.Unity.PinchUtility
//{
//
//    /// <summary>
//    /// Use this component on a Game Object to allow it to be manipulated by a pinch gesture.  The component
//    /// allows rotation, translation, and scale of the object (RTS).
//    /// </summary>
//    //[ExecuteAfter(typeof(LeapPinchDetector))]
//    public class NewLeapRTS : MonoBehaviour
//    {
//
//        public enum RotationMethod
//        {
//            None,
//            Single,
//            Full
//        }
//
//        [SerializeField]
//        private LeapPinchDetector _pinchDetectorA;
//
//        [SerializeField]
//		private LeapPinchDetector _pinchDetectorB;
//
//		private GameObject _handR;
//		private GameObject _handL;
//
//        private LeapPinchDetector[] _detectors;
//
//        [SerializeField]
//        private RotationMethod _oneHandedRotationMethod;
//
//        [SerializeField]
//        private RotationMethod _twoHandedRotationMethod;
//
//        [SerializeField]
//        private bool _allowScale = true;
//
//        [Header("GUI Options")]
//        [SerializeField]
//        private KeyCode _toggleGuiState = KeyCode.G;
//
//        [SerializeField]
//        private bool _showGUI = true;
//
//        private Transform _anchor;
//
//        private Quaternion _defaultRot;
//        private Vector3 _defaultTrans;
//
//        private float _defaultNearClip;
//
//
//        void Awake()
//        {
//			_handR = GameObject.FindGameObjectWithTag("HandR");
//			_handL = GameObject.FindGameObjectWithTag("HandL");
//
//			_pinchDetectorA = _handR.GetComponentInChildren<LeapPinchDetector>();
//			_pinchDetectorB = _handL.GetComponentInChildren<LeapPinchDetector>();
//
//
//
//            if (_pinchDetectorA == null || _pinchDetectorB == null)
//            {
//                Debug.LogWarning("Both Pinch Detectors of the LeapRTS component must be assigned. This component has been disabled.");
//                enabled = false;
//            }
//
//            GameObject pinchControl = new GameObject("RTS Anchor");
//            _anchor = pinchControl.transform;
//            _anchor.transform.parent = transform.parent;
//            transform.parent = _anchor;
//
//            _defaultTrans = transform.position;
//            _defaultRot = transform.rotation;
//
//            _oneHandedRotationMethod = RotationMethod.Full;
//            _allowScale = false;
//            _showGUI = false;
//	
//        }
//
//		void Start()
//		{
//				_handR.SetActive(false);
//				_handL.SetActive(false);
//		}
//
//        void Update()
//        {
//
//            bool _triggered = GetComponentInChildren<ColliderTrigger>().isTriggered();
//
//            if (Input.GetKeyDown(KeyCode.G))
//            {
//                _showGUI = !_showGUI;
//            }
//
//            bool didUpdate = false;
//            didUpdate |= _pinchDetectorA.DidChangeFromLastFrame;
//            didUpdate |= _pinchDetectorB.DidChangeFromLastFrame;
//
//            if (didUpdate)
//            {
//                transform.SetParent(null, true);
//            }
//
////            if (_pinchDetectorA.IsPinching && _pinchDetectorB.IsPinching && _triggered)
////            {
////                transformDoubleAnchor();
////                //transform.rotation = _defaultRot;
////                //transform.position = _defaultTrans;
////                //GetComponentInChildren<ColliderTrigger>().setTriggerOff();
////            }
////            else 
//			if (_pinchDetectorA.IsPinching && _triggered)
//            {
////				Debug.Log ("Grabbed~");
//                transformSingleAnchor(_pinchDetectorA);
//            }
//            else if (_pinchDetectorB.IsPinching && _triggered)
//            {
//                transformSingleAnchor(_pinchDetectorB);
//            }
//            else {
//                transform.rotation = _defaultRot;
//                transform.position = _defaultTrans;
//                GetComponentInChildren<ColliderTrigger>().setTriggerOff();
//            }
//
//            if (didUpdate & _triggered)
//            {
//                transform.SetParent(_anchor, true);
//            }
//
//        }
//
//        void OnGUI()
//        {
//            if (_showGUI)
//            {
//                GUILayout.Label("One Handed Settings");
//                doRotationMethodGUI(ref _oneHandedRotationMethod);
//                GUILayout.Label("Two Handed Settings");
//                doRotationMethodGUI(ref _twoHandedRotationMethod);
//                _allowScale = GUILayout.Toggle(_allowScale, "Allow Two Handed Scale");
//            }
//        }
//
//        private void doRotationMethodGUI(ref RotationMethod rotationMethod)
//        {
//            GUILayout.BeginHorizontal();
//
//            GUI.color = rotationMethod == RotationMethod.None ? Color.green : Color.white;
//            if (GUILayout.Button("No Rotation"))
//            {
//                rotationMethod = RotationMethod.None;
//            }
//
//            GUI.color = rotationMethod == RotationMethod.Single ? Color.green : Color.white;
//            if (GUILayout.Button("Single Axis"))
//            {
//                rotationMethod = RotationMethod.Single;
//            }
//
//            GUI.color = rotationMethod == RotationMethod.Full ? Color.green : Color.white;
//            if (GUILayout.Button("Full Rotation"))
//            {
//                rotationMethod = RotationMethod.Full;
//            }
//
//            GUI.color = Color.white;
//
//            GUILayout.EndHorizontal();
//        }
//
//        private void transformDoubleAnchor()
//        {
//            _anchor.position = (_pinchDetectorA.Position + _pinchDetectorB.Position) / 2.0f;
//
//            switch (_twoHandedRotationMethod)
//            {
//                case RotationMethod.None:
//                    break;
//                case RotationMethod.Single:
//                    Vector3 p = _pinchDetectorA.Position;
//                    p.y = _anchor.position.y;
//                    _anchor.LookAt(p);
//                    break;
//                case RotationMethod.Full:
//                    Quaternion pp = Quaternion.Lerp(_pinchDetectorA.Rotation, _pinchDetectorB.Rotation, 0.5f);
//                    Vector3 u = pp * Vector3.up;
//                    _anchor.LookAt(_pinchDetectorA.Position, u);
//                    break;
//            }
//
//            if (_allowScale)
//            {
//                _anchor.localScale = Vector3.one * Vector3.Distance(_pinchDetectorA.Position, _pinchDetectorB.Position);
//            }
//        }
//
//        private void transformSingleAnchor(LeapPinchDetector singlePinch)
//        {
//            _anchor.position = singlePinch.Position;
//
//            switch (_oneHandedRotationMethod)
//            {
//                case RotationMethod.None:
//                    break;
//                case RotationMethod.Single:
//                    Vector3 p = singlePinch.Rotation * Vector3.right;
//                    p.y = _anchor.position.y;
//                    _anchor.LookAt(p);
//                    break;
//                case RotationMethod.Full:
//                    _anchor.rotation = singlePinch.Rotation;
//                    break;
//            }
//
//            _anchor.localScale = Vector3.one;
//        }
//    }
//
//    
//}
