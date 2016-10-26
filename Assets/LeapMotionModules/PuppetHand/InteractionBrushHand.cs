using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Leap.Unity
{
    /** Collision brushes */
    public class InteractionBrushHand : IHandModel
    {
        private const int N_FINGERS = 5;
        private const int N_ACTIVE_BONES = 3;

        private Rigidbody palmBody;
        private Rigidbody[] _capsuleBodies;
        private ConfigurableJoint[] _ConfigurableJoints;
        private Quaternion[] _origPalmToJointRotation;
        private Vector3 _lastPalmPosition;
        private Vector3[] _lastPositions;
        private Hand hand_;
        private GameObject HandParent;

        public override ModelType HandModelType
        {
            get { return ModelType.Physics; }
        }

        [SerializeField]
        private Chirality handedness;
        /** Whether this model can be used to represent a right or a left hand.*/
        public override Chirality Handedness
        {
            get { return handedness; }
            set { handedness = value; }
        }

        [SerializeField]
        [Tooltip("The mass of each finger bone; the palm will be 3x this.")]
        private float _perBoneMass = 3.0f;

        [SerializeField]
        [Tooltip("The maximum velocity that is applied when setting the velocity of the bodies.")]
        private float _maxVelocity = 2.0f;

        [SerializeField]
        [Tooltip("Which collision mode the hand uses.")]
        private CollisionDetectionMode _collisionDetection = CollisionDetectionMode.ContinuousDynamic;

        [SerializeField]
        [Tooltip("The physics material that the hand uses.")]
        private PhysicMaterial _material = null;

        [SerializeField]
        [Tooltip("Temporarily disables collision on bones that are too displaced.")]
        private bool uncollideFarBones = true;

        [SerializeField]
        [Tooltip("Detaches Hand.")]
        private bool detachHand = false;

        [SerializeField]
        [Tooltip("Makes hand affected by gravity.")]
        private bool useGravity = false;

        [SerializeField]
        [Tooltip("If a segment displaces farther than this times its width, it will stop colliding if uncollideFarBones is set to true.")]
        private float uncollideThreshold = 2.0f;

        [SerializeField]
        [Tooltip("Adds powered joints in between all of the finger segments.")]
        private bool useConstraints = false;

        [SerializeField]
        [Tooltip("Sets each fingers' velocity directly every frame.")]
        private bool setFingerVelocity = true;

        [SerializeField]
        [Tooltip("Finds all static and kinematic colliders and puts them on the hand's layer so they do not collide with eachother.")]
        private bool noCollideWithStaticGeometry = true;

        [SerializeField]
        [Tooltip("For debugging, will look jittery because it's updated in the fixedTimestep.")]
        private bool showHand = true;

        //HAND VISUALIZATION PROPERTIES
        private const int THUMB_BASE_INDEX = (int)Finger.FingerType.TYPE_THUMB * 4;
        private const int PINKY_BASE_INDEX = (int)Finger.FingerType.TYPE_PINKY * 4;
        private float SPHERE_RADIUS = 0.0003f;
        private float CYLINDER_RADIUS = 0.0003f;
        private float PALM_RADIUS = 0.0005f;
        private bool _showArm = true;

        [SerializeField]
        private Material _vismaterial;
        [SerializeField]
        private Mesh _sphereMesh;
        [SerializeField]
        private int _cylinderResolution = 10;

        private Transform[] _jointSpheres;
        private Transform mockThumbJointSphere;
        private Transform palmPositionSphere;

        private Transform wristPositionSphere;

        private List<Renderer> _armRenderers;
        private List<Transform> _capsuleTransforms;
        private List<Transform> _sphereATransforms;
        private List<Transform> _sphereBTransforms;

        private Transform armFrontLeft, armFrontRight, armBackLeft, armBackRight;
        //END HAND VISUALIZATION PROPERTIES

        // DELETE ME
        private int _hack = 0;

        void Start()
        {
            if (noCollideWithStaticGeometry) {
                Collider[] bodies = FindObjectsOfType<Collider>();
                foreach (Collider colliderr in bodies) {
                    if ((colliderr.attachedRigidbody == null || colliderr.attachedRigidbody.isKinematic) && !colliderr.isTrigger) {
                        colliderr.gameObject.layer = gameObject.layer;
                    }
                }
            }
        }

        public override Hand GetLeapHand() { return hand_; }
        public override void SetLeapHand(Hand hand) { hand_ = hand; }

        public override void InitHand()
        {
            base.InitHand();
            if (showHand) {
                _jointSpheres = new Transform[4 * 5];
                _armRenderers = new List<Renderer>();
                _capsuleTransforms = new List<Transform>();
                _sphereATransforms = new List<Transform>();
                _sphereBTransforms = new List<Transform>();

                CYLINDER_RADIUS *= transform.lossyScale.x;
                createSpheres();
                createCylinders();
                updateArmVisibility();
            }
        }

        public override void BeginHand()
        {
            base.BeginHand();
            HandParent = new GameObject(gameObject.name + "'s Parent");
            HandParent.transform.parent = null;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;

            // We also require a material for friction to be able to work.
            if (_material == null || _material.bounciness != 0.0f || _material.bounceCombine != PhysicMaterialCombine.Minimum) {
                UnityEditor.EditorUtility.DisplayDialog("Collision Error!",
                                                        "An InteractionBrushHand must have a material with 0 bounciness "
                                                        + "and a bounceCombine of Minimum.  Name:" + gameObject.name,
                                                        "Ok");
                Debug.Break();
            }
#endif

            GameObject palmGameObject = new GameObject(gameObject.name + " Palm", typeof(Rigidbody), typeof(BoxCollider));
            palmGameObject.layer = gameObject.layer;

            Transform palmTransform = palmGameObject.GetComponent<Transform>();
            palmTransform.parent = HandParent.transform;
            palmTransform.position = hand_.PalmPosition.ToVector3();
            palmTransform.rotation = hand_.Basis.CalculateRotation();
            if (palmTransform.parent != null) {
                palmTransform.localScale = new Vector3(1f / palmTransform.parent.lossyScale.x, 1f / palmTransform.parent.lossyScale.y, 1f / palmTransform.parent.lossyScale.z);
            }

            BoxCollider box = palmGameObject.GetComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.005f, 0.015f);
            box.size = new Vector3(0.06f, 0.02f, 0.07f);
            box.material = _material;

            palmBody = palmGameObject.GetComponent<Rigidbody>();
            palmBody.position = hand_.PalmPosition.ToVector3();
            palmBody.rotation = hand_.Basis.CalculateRotation();
            palmBody.freezeRotation = !detachHand;
            palmBody.useGravity = false;

            palmBody.mass = _perBoneMass * 3f;
            palmBody.collisionDetectionMode = _collisionDetection;


            _capsuleBodies = new Rigidbody[N_FINGERS * N_ACTIVE_BONES];
            _lastPositions = new Vector3[N_FINGERS * N_ACTIVE_BONES];

            _ConfigurableJoints = new ConfigurableJoint[(N_FINGERS * N_ACTIVE_BONES)];
            _origPalmToJointRotation = new Quaternion[(N_FINGERS * N_ACTIVE_BONES)];

            for (int fingerIndex = 0; fingerIndex < N_FINGERS; fingerIndex++) {
                for (int jointIndex = 0; jointIndex < N_ACTIVE_BONES; jointIndex++) {
                    Bone bone = hand_.Fingers[fingerIndex].Bone((Bone.BoneType)(jointIndex + 1)); // +1 to skip first bone.

                    int boneArrayIndex = fingerIndex * N_ACTIVE_BONES + jointIndex;

                    GameObject capsuleGameObject;
                    if (jointIndex < N_ACTIVE_BONES - 1 && useConstraints) {
                        capsuleGameObject = new GameObject(gameObject.name + " Finger " + boneArrayIndex, typeof(Rigidbody), typeof(CapsuleCollider), typeof(ConfigurableJoint));
                    } else {
                        capsuleGameObject = new GameObject(gameObject.name + " Finger " + boneArrayIndex, typeof(Rigidbody), typeof(CapsuleCollider));
                    }

                    //GameObject capsuleGameObject = new GameObject(gameObject.name, typeof(Rigidbody), typeof(CapsuleCollider));
                    capsuleGameObject.layer = gameObject.layer;
#if UNITY_EDITOR
                    // This is a debug facility that warns developers of issues.
                    capsuleGameObject.AddComponent<InteractionBrushBone>();
#endif

                    Transform capsuleTransform = capsuleGameObject.GetComponent<Transform>();
                    capsuleTransform.parent = HandParent.transform;
                    capsuleTransform.position = bone.Center.ToVector3();
                    capsuleTransform.rotation = bone.Rotation.ToQuaternion();
                    if (capsuleTransform.parent != null) {
                        capsuleTransform.localScale = new Vector3(1f / capsuleTransform.parent.lossyScale.x, 1f / capsuleTransform.parent.lossyScale.y, 1f / capsuleTransform.parent.lossyScale.z);
                    }

                    CapsuleCollider capsule = capsuleGameObject.GetComponent<CapsuleCollider>();
                    capsule.direction = 2;
                    capsule.radius = bone.Width * 0.5f;
                    capsule.height = bone.Length + bone.Width;
                    capsule.material = _material;

                    Rigidbody body = capsuleGameObject.GetComponent<Rigidbody>();
                    _capsuleBodies[boneArrayIndex] = body;
                    body.position = bone.Center.ToVector3();
                    body.rotation = bone.Rotation.ToQuaternion();
                    body.useGravity = useGravity;
                    if (!useConstraints) {
                        body.freezeRotation = true;
                    }

                    body.mass = _perBoneMass;
                    body.collisionDetectionMode = _collisionDetection;

                    _lastPositions[boneArrayIndex] = bone.Center.ToVector3();
                }
            }

            if (useConstraints) {
                //Add Joints to prevent fingers from coming apart
                for (int fingerIndex = 0; fingerIndex < N_FINGERS; fingerIndex++) {
                    for (int jointIndex = 0; jointIndex < N_ACTIVE_BONES; jointIndex++) {
                        int boneArrayIndex = fingerIndex * N_ACTIVE_BONES + jointIndex;
                        Bone bone = hand_.Fingers[fingerIndex].Bone((Bone.BoneType)(jointIndex + 1)); // +1 to skip first bone.

                        Rigidbody body = _capsuleBodies[boneArrayIndex].GetComponent<Rigidbody>();
                        if (jointIndex < N_ACTIVE_BONES - 1) {
                            Bone nextBone = hand_.Fingers[fingerIndex].Bone((Bone.BoneType)(jointIndex + 2)); // +1 to skip first bone.
                            _ConfigurableJoints[boneArrayIndex + 1] = _capsuleBodies[boneArrayIndex].GetComponent<ConfigurableJoint>();

                            _ConfigurableJoints[boneArrayIndex + 1].enablePreprocessing = true;
                            _ConfigurableJoints[boneArrayIndex + 1].autoConfigureConnectedAnchor = false;
                            _ConfigurableJoints[boneArrayIndex + 1].connectedBody = _capsuleBodies[boneArrayIndex + 1].gameObject.GetComponent<Rigidbody>();
                            Quaternion origJointRotation = nextBone.Rotation.ToQuaternion();
                            Quaternion origPalmRotation = _capsuleBodies[boneArrayIndex].rotation;
                            _origPalmToJointRotation[boneArrayIndex + 1] = Quaternion.Inverse(origPalmRotation) * origJointRotation;

                            _ConfigurableJoints[boneArrayIndex + 1].rotationDriveMode = RotationDriveMode.Slerp;
                            _ConfigurableJoints[boneArrayIndex + 1].anchor = body.transform.InverseTransformPoint(_capsuleBodies[boneArrayIndex + 1].transform.TransformPoint(new Vector3(0f, 0f, (_capsuleBodies[boneArrayIndex + 1].GetComponent<CapsuleCollider>().radius) - (_capsuleBodies[boneArrayIndex + 1].GetComponent<CapsuleCollider>().height / 2f))));
                            _ConfigurableJoints[boneArrayIndex + 1].connectedAnchor = new Vector3(0f, 0f, (_capsuleBodies[boneArrayIndex + 1].GetComponent<CapsuleCollider>().radius) - (_capsuleBodies[boneArrayIndex + 1].GetComponent<CapsuleCollider>().height / 2f));
                            _ConfigurableJoints[boneArrayIndex + 1].axis = body.transform.InverseTransformDirection(_capsuleBodies[boneArrayIndex + 1].transform.right);
                            _ConfigurableJoints[boneArrayIndex + 1].enableCollision = false;
                            _ConfigurableJoints[boneArrayIndex + 1].xMotion = ConfigurableJointMotion.Locked;
                            _ConfigurableJoints[boneArrayIndex + 1].yMotion = ConfigurableJointMotion.Locked;
                            _ConfigurableJoints[boneArrayIndex + 1].zMotion = ConfigurableJointMotion.Locked;

                            _ConfigurableJoints[boneArrayIndex + 1].hideFlags = HideFlags.DontSave | HideFlags.DontSaveInEditor;

                            JointDrive motorMovement = new JointDrive();
                            motorMovement.maximumForce = 500000000f;
                            motorMovement.positionSpring = 500000000f;

                            _ConfigurableJoints[boneArrayIndex + 1].slerpDrive = motorMovement;
                        }

                        if (jointIndex == 0) {
                            _ConfigurableJoints[boneArrayIndex] = palmBody.gameObject.AddComponent<ConfigurableJoint>();
                            _ConfigurableJoints[boneArrayIndex].configuredInWorldSpace = false;
                            _ConfigurableJoints[boneArrayIndex].connectedBody = _capsuleBodies[boneArrayIndex].GetComponent<Rigidbody>();
                            Quaternion origJointRotation = bone.Rotation.ToQuaternion();
                            Quaternion origPalmRotation = palmBody.rotation;
                            _origPalmToJointRotation[boneArrayIndex] = Quaternion.Inverse(origPalmRotation) * origJointRotation;

                            _ConfigurableJoints[boneArrayIndex].rotationDriveMode = RotationDriveMode.Slerp;

                            _ConfigurableJoints[boneArrayIndex].enablePreprocessing = true;
                            _ConfigurableJoints[boneArrayIndex].autoConfigureConnectedAnchor = false;
                            _ConfigurableJoints[boneArrayIndex].anchor = palmBody.transform.InverseTransformPoint(_capsuleBodies[boneArrayIndex].transform.TransformPoint(new Vector3(0f, 0f, (_capsuleBodies[boneArrayIndex].GetComponent<CapsuleCollider>().radius) - (_capsuleBodies[boneArrayIndex].GetComponent<CapsuleCollider>().height / 2f))));
                            _ConfigurableJoints[boneArrayIndex].connectedAnchor = new Vector3(0f, 0f, (_capsuleBodies[boneArrayIndex].GetComponent<CapsuleCollider>().radius) - (_capsuleBodies[boneArrayIndex].GetComponent<CapsuleCollider>().height / 2f));
                            _ConfigurableJoints[boneArrayIndex].enableCollision = false;

                            _ConfigurableJoints[boneArrayIndex].hideFlags = HideFlags.DontSave | HideFlags.DontSaveInEditor;

                            _ConfigurableJoints[boneArrayIndex].xMotion = ConfigurableJointMotion.Locked;
                            _ConfigurableJoints[boneArrayIndex].yMotion = ConfigurableJointMotion.Locked;
                            _ConfigurableJoints[boneArrayIndex].zMotion = ConfigurableJointMotion.Locked;

                            JointDrive motorMovement = new JointDrive();
                            motorMovement.maximumForce = 500000000f;
                            motorMovement.positionSpring = 500000000f;

                            _ConfigurableJoints[boneArrayIndex].slerpDrive = motorMovement;
                        }
                    }
                }
            }
            if (showHand) {
                for (int i = 0; i < _capsuleTransforms.Count; i++) {
                    _capsuleTransforms[i].gameObject.SetActive(true);
                }
                for (int i = 0; i < _jointSpheres.Length; i++) {
                    _jointSpheres[i].gameObject.SetActive(true);
                }
                wristPositionSphere.gameObject.SetActive(true);
                mockThumbJointSphere.gameObject.SetActive(true);
                armBackLeft.gameObject.SetActive(true);
                armBackRight.gameObject.SetActive(true);
                armFrontLeft.gameObject.SetActive(true);
                armFrontRight.gameObject.SetActive(true);
                palmPositionSphere.gameObject.SetActive(true);
            }
        }

        public override void UpdateHand()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;
#endif

            // DELETE ME
            bool xx = false;
            if (++_hack > 100) {
                _hack = 0;
                xx = true;
            }

            Collider palmCollider = palmBody.GetComponent<BoxCollider>();
            Vector3 palmDelta = hand_.PalmPosition.ToVector3() - palmBody.position;

            float massOfHand = palmBody.mass + (N_FINGERS * N_ACTIVE_BONES * _perBoneMass);
            if(!detachHand){
              palmBody.velocity = (palmDelta / Time.fixedDeltaTime);
            }
            palmBody.useGravity = useGravity;
            if (palmBody.freezeRotation) {
                palmBody.MoveRotation(hand_.Basis.CalculateRotation());
            }

            if (!palmCollider.isTrigger) {
                if (uncollideFarBones) {
                    Vector3 error = _lastPalmPosition - palmBody.position;
                    if (error.magnitude > (palmBody.GetComponent<BoxCollider>().size.y * uncollideThreshold)) {
                        palmCollider.isTrigger = true;
                    }
                }
            } else {
                if (xx) {
                    //            bool isClear = Physics.CheckCapsule(bone.PrevJoint.ToVector3(), bone.NextJoint.ToVector3(), bone.Width * 0.5f, _layerMask);
                    //            if (isClear)
                    {
                        palmCollider.isTrigger = false;
                        palmBody.position = hand_.PalmPosition.ToVector3();
                        palmBody.rotation = hand_.Basis.CalculateRotation();
                        palmBody.velocity = Vector3.zero;
                    }
                }
            }
            _lastPalmPosition = hand_.PalmPosition.ToVector3();

            for (int fingerIndex = 0; fingerIndex < N_FINGERS; fingerIndex++) {
                for (int jointIndex = 0; jointIndex < N_ACTIVE_BONES; jointIndex++) {
                    Bone bone = hand_.Fingers[fingerIndex].Bone((Bone.BoneType)(jointIndex + 1));

                    int boneArrayIndex = fingerIndex * N_ACTIVE_BONES + jointIndex;
                    Rigidbody body = _capsuleBodies[boneArrayIndex];
                    Collider collider = body.GetComponent<CapsuleCollider>();


                    if (setFingerVelocity) {
                        Vector3 delta = (bone.Center.ToVector3() - body.position) / Time.fixedDeltaTime;
                        body.velocity = delta.magnitude > _maxVelocity ? (delta / delta.magnitude) * _maxVelocity : delta;
                    }

                    if (body.freezeRotation) {
                        body.MoveRotation(bone.Rotation.ToQuaternion());
                    }
                    if (!collider.isTrigger) {
                        // Compare against intended target, not new tracking position.
                        if (uncollideFarBones) {
                            Vector3 error = _lastPositions[boneArrayIndex] - body.position;
                            if (error.magnitude > (bone.Width * uncollideThreshold) || (jointIndex > 0 && _capsuleBodies[boneArrayIndex - 1].GetComponent<Collider>().isTrigger)) {
                                collider.isTrigger = true;
                            }
                        }

                    } else if (xx) {
                        //            bool isClear = Physics.CheckCapsule(bone.PrevJoint.ToVector3(), bone.NextJoint.ToVector3(), bone.Width * 0.5f, _layerMask);
                        //            if (isClear)
                        {
                            collider.isTrigger = false;
                            body.position = bone.Center.ToVector3();
                            body.rotation = bone.Rotation.ToQuaternion();
                            body.velocity = Vector3.zero;
                        }
                    }

                    _lastPositions[boneArrayIndex] = bone.Center.ToVector3();
                }
            }

            //Drive the Joints for extra friction without setting the rotation
            if (useConstraints) {
                for (int fingerIndex = 0; fingerIndex < N_FINGERS; fingerIndex++) {
                    for (int jointIndex = 0; jointIndex < N_ACTIVE_BONES; jointIndex++) {
                        int boneArrayIndex = fingerIndex * N_ACTIVE_BONES + jointIndex;
                        Bone bone = hand_.Fingers[fingerIndex].Bone((Bone.BoneType)(jointIndex + 1));

                        Rigidbody body = _capsuleBodies[boneArrayIndex].GetComponent<Rigidbody>();

                        if (jointIndex == 0) {
                            Quaternion localRealFinger = (Quaternion.Euler(180f, 180f, 180f) * (Quaternion.Inverse(hand_.Basis.CalculateRotation() * _origPalmToJointRotation[boneArrayIndex]) * bone.Rotation.ToQuaternion()));

                            if (fingerIndex == 0 && hand_.IsRight) {
                                localRealFinger = Quaternion.Euler(localRealFinger.eulerAngles.y, localRealFinger.eulerAngles.x, localRealFinger.eulerAngles.z);
                            } else if (fingerIndex == 0 && hand_.IsLeft) {
                                localRealFinger = Quaternion.Euler(localRealFinger.eulerAngles.y * -1f, localRealFinger.eulerAngles.x * -1f, localRealFinger.eulerAngles.z);
                            } else {
                                localRealFinger = Quaternion.Euler(localRealFinger.eulerAngles.x * -1f, localRealFinger.eulerAngles.y, 0f);
                            }

                            _ConfigurableJoints[boneArrayIndex].targetRotation = localRealFinger;

                        }
                        if (jointIndex < N_ACTIVE_BONES - 1) {
                            Bone nextBone = hand_.Fingers[fingerIndex].Bone((Bone.BoneType)(jointIndex + 2));
                            Quaternion localRealFinger = (Quaternion.Euler(180f, 180f, 180f) * (Quaternion.Inverse(bone.Rotation.ToQuaternion() * _origPalmToJointRotation[boneArrayIndex + 1]) * nextBone.Rotation.ToQuaternion()));
                            localRealFinger = Quaternion.Euler(localRealFinger.eulerAngles.x, localRealFinger.eulerAngles.y * -1f, localRealFinger.eulerAngles.z);
                            _ConfigurableJoints[boneArrayIndex + 1].targetRotation = localRealFinger;
                        }

                    }
                }
            }

            if (showHand) {
                //Update the spheres first
                updateSpheres();
                //Update Arm only if we need to
                if (_showArm) {
                    updateArm();
                }
                //The capsule transforms are deterimined by the spheres they are connected to
                updateCapsules();
            }
        }

        public override void FinishHand()
        {
            GameObject.Destroy(palmBody.gameObject);
            palmBody = null;
            for (int i = _capsuleBodies.Length; i-- != 0; ) {
                _capsuleBodies[i].transform.parent = null;
                GameObject.Destroy(_capsuleBodies[i].gameObject);
            }
            _capsuleBodies = null;

            GameObject.Destroy(HandParent);


            if (showHand) {
                for (int i = 0; i < _capsuleTransforms.Count; i++) {
                    _capsuleTransforms[i].gameObject.SetActive(false);
                }
                for (int i = 0; i < _jointSpheres.Length; i++) {
                    _jointSpheres[i].gameObject.SetActive(false);
                }
                wristPositionSphere.gameObject.SetActive(false);
                mockThumbJointSphere.gameObject.SetActive(false);
                armBackLeft.gameObject.SetActive(false);
                armBackRight.gameObject.SetActive(false);
                armFrontLeft.gameObject.SetActive(false);
                armFrontRight.gameObject.SetActive(false);
                palmPositionSphere.gameObject.SetActive(false);
            }
            base.FinishHand();
        }

    private void updateSpheres() {
      //Update all spheres
        for (int fingerIndex = 0; fingerIndex < N_FINGERS; fingerIndex++) {
            for (int jointIndex = 0; jointIndex < N_ACTIVE_BONES; jointIndex++) {
              int boneArrayIndex = fingerIndex * N_ACTIVE_BONES + jointIndex;

              if(jointIndex==0){
                  Transform knuckleSphere = _jointSpheres[getFingerJointIndex(fingerIndex, jointIndex)];
                  knuckleSphere.position = _capsuleBodies[boneArrayIndex].transform.position - (_capsuleBodies[boneArrayIndex].transform.forward * (_capsuleBodies[boneArrayIndex].GetComponent<CapsuleCollider>().height/2f));
              }

              Transform jointSphere = _jointSpheres[getFingerJointIndex(fingerIndex, jointIndex + 1)];
              jointSphere.position = _capsuleBodies[boneArrayIndex].transform.position + (_capsuleBodies[boneArrayIndex].transform.forward * (_capsuleBodies[boneArrayIndex].GetComponent<CapsuleCollider>().height / 2f));
          }
      }
      palmPositionSphere.position = palmBody.position;

      Vector3 wristPos = palmBody.position;
    wristPositionSphere.position = wristPos;

    Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];

    //Vector3 thumbBaseToPalm = thumbBase.position - hand_.PalmPosition.ToVector3();
    //mockThumbJointSphere.position = hand_.PalmPosition.ToVector3() + Vector3.Reflect(thumbBaseToPalm, hand_.Basis.xBasis.ToVector3().normalized);

    Vector3 thumbBaseToPalm = thumbBase.position - wristPos;
    mockThumbJointSphere.position = wristPos + Vector3.Reflect(thumbBaseToPalm, palmBody.transform.right);
  }

  private void updateArm() {
    var arm = hand_.Arm;
    Vector3 right = arm.Basis.xBasis.ToVector3().normalized * arm.Width * 0.7f * 0.5f;
    Vector3 wrist = arm.WristPosition.ToVector3();
    Vector3 elbow = arm.ElbowPosition.ToVector3();

    float armLength = Vector3.Distance(wrist, elbow);
    wrist -= arm.Direction.ToVector3() * armLength * 0.05f;

    armFrontRight.position = wrist + right;
    armFrontLeft.position = wrist - right;
    armBackRight.position = elbow + right;
    armBackLeft.position = elbow - right;
  }

  private void updateCapsules() {
    for (int i = 0; i < _capsuleTransforms.Count; i++) {
      Transform capsule = _capsuleTransforms[i];
      Transform sphereA = _sphereATransforms[i];
      Transform sphereB = _sphereBTransforms[i];

      Vector3 delta = sphereA.position - sphereB.position;

      MeshFilter filter = capsule.GetComponent<MeshFilter>();
      if (filter.sharedMesh == null) {
        filter.sharedMesh = generateCylinderMesh(delta.magnitude / transform.lossyScale.x);
      }

      capsule.position = sphereA.position;

      if (delta.sqrMagnitude <= Mathf.Epsilon) {
        //Two spheres are at the same location, no rotation will be found
        continue;
      }

      Vector3 perp;
      if (Vector3.Angle(delta, Vector3.up) > 170 || Vector3.Angle(delta, Vector3.up) < 10) {
        perp = Vector3.Cross(delta, Vector3.right);
      } else {
        perp = Vector3.Cross(delta, Vector3.up);
      }

      capsule.rotation = Quaternion.LookRotation(perp, delta);
      capsule.LookAt(sphereB);
    }
  }

  private void updateArmVisibility() {
    for (int i = 0; i < _armRenderers.Count; i++) {
      _armRenderers[i].enabled = _showArm;
    }
  }


  private void createSpheres() {
    //Create spheres for finger joints
    List<Finger> fingers = hand_.Fingers;
    for (int i = 0; i < fingers.Count; i++) {
      Finger finger = fingers[i];
      for (int j = 0; j < 4; j++) {
        int key = getFingerJointIndex((int)finger.Type, j);
        _jointSpheres[key] = createSphere("Joint", SPHERE_RADIUS);
      }
    }

    mockThumbJointSphere = createSphere("MockJoint", SPHERE_RADIUS);
    palmPositionSphere = createSphere("PalmPosition", PALM_RADIUS);
    wristPositionSphere = createSphere("WristPosition", SPHERE_RADIUS);

    armFrontLeft = createSphere("ArmFrontLeft", SPHERE_RADIUS, true);
    armFrontRight = createSphere("ArmFrontRight", SPHERE_RADIUS, true);
    armBackLeft = createSphere("ArmBackLeft", SPHERE_RADIUS, true);
    armBackRight = createSphere("ArmBackRight", SPHERE_RADIUS, true);
  }

  private void createCylinders() {
    //Create cylinders between finger joints
    for (int i = 0; i < 5; i++) {
      for (int j = 0; j < 3; j++) {
        int keyA = getFingerJointIndex(i, j);
        int keyB = getFingerJointIndex(i, j + 1);

        Transform sphereA = _jointSpheres[keyA];
        Transform sphereB = _jointSpheres[keyB];

        createCylinder("Finger Joint", sphereA, sphereB);
      }
    }

    //Create cylinder between finger knuckles
    for (int i = 0; i < 4; i++) {
      int keyA = getFingerJointIndex(i, 0);
      int keyB = getFingerJointIndex(i + 1, 0);

      Transform sphereA = _jointSpheres[keyA];
      Transform sphereB = _jointSpheres[keyB];

      createCylinder("Hand Joints", sphereA, sphereB);
    }

    //Create the rest of the hand
    Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];
    Transform pinkyBase = _jointSpheres[PINKY_BASE_INDEX];
    createCylinder("Hand Bottom", thumbBase, mockThumbJointSphere);
    createCylinder("Hand Side", pinkyBase, mockThumbJointSphere);

    createCylinder("ArmFront", armFrontLeft, armFrontRight, true);
    createCylinder("ArmBack", armBackLeft, armBackRight, true);
    createCylinder("ArmLeft", armFrontLeft, armBackLeft, true);
    createCylinder("ArmRight", armFrontRight, armBackRight, true);
  }

  private int getFingerJointIndex(int fingerIndex, int jointIndex) {
    return fingerIndex * 4 + jointIndex;
  }

  private Transform createSphere(string name, float radius, bool isPartOfArm = false) {
    GameObject sphere = new GameObject(name);
    sphere.AddComponent<MeshFilter>().mesh = _sphereMesh;
    sphere.AddComponent<MeshRenderer>().sharedMaterial = _vismaterial;
    sphere.transform.parent = transform;
    sphere.transform.localScale = Vector3.one * radius * 2;

    sphere.hideFlags = HideFlags.DontSave;

    if (isPartOfArm) {
      _armRenderers.Add(sphere.GetComponent<Renderer>());
    }

    return sphere.transform;
  }

  private void createCylinder(string name, Transform jointA, Transform jointB, bool isPartOfArm = false) {
    GameObject cylinder = new GameObject(name);
    cylinder.AddComponent<MeshFilter>();
    cylinder.AddComponent<MeshRenderer>().sharedMaterial = _vismaterial;
    cylinder.transform.parent = transform;

    _capsuleTransforms.Add(cylinder.transform);
    _sphereATransforms.Add(jointA);
    _sphereBTransforms.Add(jointB);

    cylinder.hideFlags = HideFlags.DontSave;

    if (isPartOfArm) {
      _armRenderers.Add(cylinder.GetComponent<Renderer>());
    }
  }

  private Mesh generateCylinderMesh(float length) {
    Mesh mesh = new Mesh();
    mesh.name = "GeneratedCylinder";

    List<Vector3> verts = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<int> tris = new List<int>();

    Vector3 p0 = Vector3.zero;
    Vector3 p1 = Vector3.forward * length * transform.lossyScale.x;
    for (int i = 0; i < _cylinderResolution; i++) {
      float angle = (Mathf.PI * 2.0f * i) / _cylinderResolution;
      float dx = CYLINDER_RADIUS * Mathf.Cos(angle);
      float dy = CYLINDER_RADIUS * Mathf.Sin(angle);

      Vector3 spoke = new Vector3(dx, dy, 0);

      verts.Add(p0 + spoke);
      verts.Add(p1 + spoke);

      colors.Add(Color.white);
      colors.Add(Color.white);

      int triStart = verts.Count;
      int triCap = _cylinderResolution * 2;

      tris.Add((triStart + 0) % triCap);
      tris.Add((triStart + 2) % triCap);
      tris.Add((triStart + 1) % triCap);
      //
      tris.Add((triStart + 2) % triCap);
      tris.Add((triStart + 3) % triCap);
      tris.Add((triStart + 1) % triCap);
    }

      mesh.SetVertices(verts);
      mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      mesh.Optimize();
      mesh.UploadMeshData(true);

      return mesh;
    }
  }
}