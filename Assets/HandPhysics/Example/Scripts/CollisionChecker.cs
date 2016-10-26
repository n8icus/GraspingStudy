using UnityEngine;
using System.Collections;

public class CollisionChecker : MonoBehaviour
{
    public int ItemId;
    public string ItemName;

    private HandPhysicsGame _hpGame;

    void Start()
    {
        _hpGame = GameObject.Find("HandPhysicsGame").GetComponent<HandPhysicsGame>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name == ItemName)
        {
            _hpGame.SwitchLamp(ItemId, true);
        }
        
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.name == ItemName)
        {
            _hpGame.SwitchLamp(ItemId, false);
        }

    }
}
