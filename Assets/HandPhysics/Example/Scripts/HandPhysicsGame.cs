using UnityEngine;
using System.Collections;

public class HandPhysicsGame : MonoBehaviour
{

    public GameObject[] Items;
    public GameObject[] Lamps;

    private Light[] _lampLight;

    private bool _helpEnabled;

	void Start () 
    {
        _lampLight = new Light[Lamps.Length];

        for (int i = 0; i < Lamps.Length; i++)
        {
            _lampLight[i] = Lamps[i].transform.Find("Light").gameObject.GetComponent<Light>();
        }

        SwitchLamp(0, false);
        SwitchLamp(1, false);
        SwitchLamp(2, false);
	}

    public void SwitchLamp(int lampId, bool enable)
    {
        if (enable)
        {
            Lamps[lampId].GetComponent<Renderer>().material.color = Color.green;
            _lampLight[lampId].enabled = true;
        }

        else
        {
            Lamps[lampId].GetComponent<Renderer>().material.color = Color.gray;
            _lampLight[lampId].enabled = false;
        }
    }
}
