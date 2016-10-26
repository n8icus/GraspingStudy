using UnityEngine;
using System.Collections;

public class DistanceField : MonoBehaviour {
  public float ContactOppositionThreshold = 0.1f;
  public float SphereScale = 0.02f;
  public float SurfaceDilation = 0f;

  [SerializeField]
  private Mesh _sphereMesh;
  [SerializeField]
  private Material _sphereMaterial;

  [System.Serializable]
  public struct GrabTestPoint {
    public Transform HandPoint;
    [HideInInspector]
    public bool Touching;
    [HideInInspector]
    public Vector3 Gradient;
    [HideInInspector]
    public Transform Marker;
  }

  [SerializeField]
  public GrabTestPoint[] Points;
  public Transform Palm;

  public bool isCapsule = false;
  bool Grabbing = false;

  //Local Coordinate Distance Field for Unit Cube
  float distanceField(Vector3 pos) {
    if (!isCapsule) {
      Vector3 d = new Vector3(Mathf.Abs(pos.x), Mathf.Abs(pos.y), Mathf.Abs(pos.z)) - (Vector3.one * 0.5f);
      return Mathf.Min(Mathf.Max(d.x, Mathf.Max(d.y, d.z)), 0f) +
             new Vector3(Mathf.Max(d.x, 0f), Mathf.Max(d.y, 0f), Mathf.Max(d.z, 0f)).magnitude - SurfaceDilation;
    } else {
      Vector3 pa = pos - (Vector3.up * 0.5f), ba = (Vector3.up * -0.5f) - (Vector3.up * 0.5f);
      float h = Mathf.Clamp01(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba));
      return ((pa - ba * h).magnitude - 0.5f) - SurfaceDilation;
    }
  }

  //Local-Local Space Gradient
  Vector3 gradient(Vector3 pos) {
    float grad_step = 0.001f;
    Vector3 dx = Vector3.right * grad_step;
    Vector3 dy = Vector3.up * grad_step;
    Vector3 dz = Vector3.forward * grad_step;
    return Vector3.Normalize(
      new Vector3(
        distanceField(pos + dx) - distanceField(pos - dx),
        distanceField(pos + dy) - distanceField(pos - dy),
        distanceField(pos + dz) - distanceField(pos - dz)
      )
    );
  }

  //World Position to World Surface Point
  Vector3 nearestSurfPointWorld(Vector3 worldPos) {
    Vector3 localTarget = transform.InverseTransformPoint(worldPos);
    return transform.TransformPoint(localTarget - (gradient(localTarget) * distanceField(localTarget)));
  }

  //World Position to Local Surface Point
  Vector3 nearestSurfPointLocal(Vector3 worldPos) {
    Vector3 localTarget = transform.InverseTransformPoint(worldPos);
    return localTarget - (gradient(localTarget) * distanceField(localTarget));
  }

  //World Position to Local Gradient
  Vector3 localGradient(Vector3 worldPos) {
    Vector3 localTarget = transform.InverseTransformPoint(worldPos);
    return gradient(localTarget);
  }

  //World Position - Inside or Outside of Object?
  bool touching(Vector3 worldPos) {
    float dist = distanceField(transform.InverseTransformPoint(worldPos));
    return dist<0f;
  }

  //Make the little Diagnostic Spheres
  void Start() {
    for (int i = 0; i < Points.Length; i++) {
      Points[i].Marker = new GameObject("Marker " + i).transform;
      Points[i].Marker.parent = transform;
      Points[i].Marker.localPosition = Vector3.zero;
      Points[i].Marker.localRotation = Quaternion.identity;
      Points[i].Marker.gameObject.AddComponent<MeshFilter>().mesh = _sphereMesh;
      Points[i].Marker.gameObject.AddComponent<MeshRenderer>().sharedMaterial = _sphereMaterial;
    }
  }

	// Update is called once per frame
  void Update() {
    for (int i = 0; i < Points.Length; i++) {
      //Set the scale of the child test points
      if (Points[i].Marker) { Points[i].Marker.localScale = new Vector3(SphereScale / transform.lossyScale.x, SphereScale / transform.lossyScale.y, SphereScale / transform.lossyScale.z); }

      //Update Index Finger Touching/Gradient State
      if (touching(Points[i].HandPoint.position)) {
        if (Points[i].Marker) {
          Points[i].Marker.gameObject.GetComponent<Renderer>().material.color = Color.green;
        }
        Points[i].Touching = true;
      } else {
        if (Points[i].Marker) {
          Points[i].Marker.localPosition = nearestSurfPointLocal(Points[i].HandPoint.position);
          Points[i].Marker.gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        touching(Points[i].Gradient = localGradient(Points[i].HandPoint.position));
        Points[i].Touching = false;
      }
    }

    //Check if two opposing fingers are touching
    bool isGrabbing = false;
    for (int i = 1; i < Points.Length; i++) {
      if ((Points[0].Touching) && ((Points[i].Touching && Vector3.Dot(Points[i].Gradient, Points[0].Gradient) < ContactOppositionThreshold))) {
        isGrabbing = true;
      }
      if ((Points[Points.Length - 1].Touching) && ((Points[i].Touching && Vector3.Dot(Points[i].Gradient, Points[Points.Length - 1].Gradient) < ContactOppositionThreshold))) {
        isGrabbing = true;
      }
    }

    //Execute the grab event or something
    if (isGrabbing && !Grabbing) {
      Grabbing = true;
      transform.parent = Palm;
      GetComponent<Renderer>().material.color = Color.green;
    } else if (!isGrabbing && Grabbing) {
      Grabbing = false;
      transform.parent = null;
      GetComponent<Renderer>().material.color = Color.white;
    }
  }
}
