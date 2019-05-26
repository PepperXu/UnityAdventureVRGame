using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(Rigidbody))]
public class FirstPersonInteractable : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent OnHoverIn, OnHoverOut, OnHover, OnPressUp, OnPressDown, OnPress;

    public Transform attachmentPoint;

    private Collider col;
    private Rigidbody rigid;

    private void OnEnable()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        
        col = GetComponent<Collider>();
        rigid = GetComponent<Rigidbody>();

        
        if (!GetComponent<MeshCollider>() || GetComponent<MeshCollider>().convex)
        {
            col.isTrigger = false;
        } 

        if (rigid)
        {
            rigid.isKinematic = false;
        }
    }

    public void HoverIn()
    {
        OnHoverIn?.Invoke();
    }
    public void Hover()
    {
        OnHover?.Invoke();
    }
    public void HoverOut()
    {
        OnHoverOut?.Invoke();
    }
    public void PressUp()
    {
        OnPressUp?.Invoke();
    }
    public void Press()
    {
        OnPress?.Invoke();
    }
    public void PressDown()
    {
        OnPressDown?.Invoke();
    }

    private void OnDisable()
    {
        if (!GetComponent<MeshCollider>() || GetComponent<MeshCollider>().convex)
        {
            col.isTrigger = true;
        }

        if (rigid)
        {
            rigid.isKinematic = true;
        }
    }
}
