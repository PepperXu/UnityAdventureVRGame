using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimatedDoor : Trigger
{
    public Trigger[] stateTriggers;
    private Animator anim;
    public string KeyTag;


    void Start()
    {
        anim = GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        bool isTriggered = true;
        foreach(Trigger t in stateTriggers)
        {
            isTriggered = isTriggered && t.isTriggered;
        }
        if (isTriggered)
        {
            anim.SetTrigger("Open");
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.tag == KeyTag)
        {
            isTriggered = true;
        }
    }
}
