using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalButton : Trigger
{
    public string triggerTagName;
    //public bool isTriggered = false;
    private Animator anim;

    private void Start()
    {
        isTriggered = false;
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == triggerTagName)
        {
            isTriggered = true;
        }
        anim.SetBool("Pressed", true);
    }

    private void OnCollisionStay(Collision collision)
    {
        anim.SetBool("Pressed", true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == triggerTagName)
        {
            isTriggered = false;
        }
        anim.SetBool("Pressed", false);
    }

}
