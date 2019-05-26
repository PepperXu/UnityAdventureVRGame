using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ArmSwing : MonoBehaviour
{
    public SteamVR_TrackedObject leftHand, rightHand;
    private Vector3 preLeftLoc, preRightLoc;
    public bool LeftActive, RightActive;
    public float speedscale;

    private enum HandType
    {
        left,
        right,
        both
    }

    public void SetLeftActive(bool isActive)
    {
        LeftActive = isActive;
    }

    public void SetRightActive(bool isActive)
    {
       RightActive = isActive;
    }
    // Update is called once per frame
    void Update()
    {
        if (LeftActive && RightActive)
        {
            transform.position += CheckControllerDirection(HandType.both) * CheckControllerDistance(HandType.both);
        }
        else if (LeftActive)
        {
            transform.position += CheckControllerDirection(HandType.left) * CheckControllerDistance(HandType.left);
        }
        else if (RightActive)
        {
            transform.position += CheckControllerDirection(HandType.right) * CheckControllerDistance(HandType.right);
        }
        preLeftLoc = leftHand.transform.position;
        preRightLoc = rightHand.transform.position;
    }

    float CheckControllerDistance(HandType type)
    {
        switch (type)
        {
            case HandType.left:
                return Vector3.Distance(leftHand.transform.position, preLeftLoc) * speedscale;
            case HandType.right:
                return Vector3.Distance(rightHand.transform.position, preRightLoc) * speedscale;
            case HandType.both:
                return Vector3.Distance(leftHand.transform.position, preLeftLoc) * speedscale + Vector3.Distance(rightHand.transform.position, preRightLoc) * speedscale;
            default:
                return 0f;
        }
    }

    Vector3 CheckControllerDirection(HandType type)
    {
        switch (type)
        {
            case HandType.left:
                return new Vector3(leftHand.transform.forward.x, 0, leftHand.transform.forward.z).normalized;
            case HandType.right:
                return new Vector3(rightHand.transform.forward.x, 0, rightHand.transform.forward.z).normalized;
            case HandType.both:
                return (new Vector3(rightHand.transform.forward.x, 0, rightHand.transform.forward.z).normalized + new Vector3(leftHand.transform.forward.x, 0, leftHand.transform.forward.z).normalized).normalized;
            default:
                return Vector3.zero;
        }
    }


}
