using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedDoor : Trigger
{
    public Trigger[] triggers;
    private Animator anim;
    public string KeyTag;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();   
    }

    // Update is called once per frame
    void Update()
    {
        bool isTriggered = true;
        foreach(Trigger t in triggers)
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
