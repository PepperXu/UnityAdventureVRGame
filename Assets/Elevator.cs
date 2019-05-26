using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public Transform totem;
    private float previousAngle;
    public float rotationElevateMultiplier = 0.1f;
    public float minHeight, maxHeight;
    public bool elevate;
    // Start is called before the first frame update
    void Start()
    {
        previousAngle = NormaliseAngle(totem.transform.eulerAngles.y);
    }

    // Update is called once per frame
    void Update()
    {
        float currentAngle = NormaliseAngle(totem.transform.eulerAngles.y);

        if(Mathf.Sign(currentAngle) != Mathf.Sign(previousAngle) && Mathf.Abs(currentAngle) > 90f)
        {
            currentAngle += currentAngle > previousAngle ? -360f : 360f;
        }

        float elevation = elevate ? (currentAngle - previousAngle) * rotationElevateMultiplier : -(currentAngle - previousAngle) * rotationElevateMultiplier;

        transform.position = new Vector3(transform.position.x,
            Mathf.Clamp(transform.position.y + elevation, minHeight, maxHeight),
            transform.position.z);
        

        previousAngle = NormaliseAngle(currentAngle);
    }

    float NormaliseAngle(float angle)
    {
        float curAngle = angle;
        while (curAngle > 180f)
        {
            curAngle -= 360f;
        }
        while (curAngle <= -180f)
        {
            curAngle += 360f;
        }
        return curAngle;
    }
}
