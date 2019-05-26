using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Totem : Trigger
{
    private bool isRotating;
    public float rotateTime;
    public float minAcceptanceAngle, maxAcceptanceAngle;
    // Start is called before the first frame update
    void Start()
    {
        isRotating = false;
        
    }

    private void Update()
    {
        float curAngle = transform.eulerAngles.y;
        while(curAngle > 180f)
        {
            curAngle -= 360f;
        }
        while (curAngle <= -180f)
        {
            curAngle += 360f;
        }
        //Debug.Log(transform.eulerAngles);
        if (curAngle > minAcceptanceAngle && curAngle < maxAcceptanceAngle)
        {
            isTriggered = true;
        } else
        {
            isTriggered = false;
        }
    }

    public void Rotate()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateTotem());
        }
    }

    IEnumerator RotateTotem()
    {
        isRotating = true;
        Vector3 startAngles = transform.eulerAngles;
        float timer = 0f;
        while(timer <= rotateTime)
        {
            transform.eulerAngles = startAngles + new Vector3(0, 0, Mathf.Clamp(timer / rotateTime * 120f, 0f, 120f));
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isRotating = false;
    }

    
}
