using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    public bool isTriggered;
    public UnityAction Activate;

    // Start is called before the first frame update
    void Start()
    {
        isTriggered = false;
    }

    //public void ActivateTrigger()
    //{
    //    Activate();
    //}
    //

}
