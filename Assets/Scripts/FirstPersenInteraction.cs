using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersenInteraction : MonoBehaviour
{
    private FirstPersonInteractable currentInteractable, previousInteractable, pressedInteractable, attachedInteractable;
    private Transform attachedInteractableParent;
    public Transform objectAttachPoint;
    public float baseThrowForce;
    private float throwForceCharger = 1f;
    private float throwForce;
    public LayerMask interactableLayer;
    //public GameObject lr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(attachedInteractable != null)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                DetachObject();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                throwForce = baseThrowForce;
            }

            if (Input.GetKey(KeyCode.G))
            {
                throwForce += Time.deltaTime * throwForceCharger;
            }

            if (Input.GetKeyUp(KeyCode.G))
            {
                ThrowObject();
            }

        } else
        {
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    GameObject obj = Instantiate(lr) as GameObject;
            //    LineRenderer line = lr.GetComponent<LineRenderer>();
            //    line.SetPosition(0, transform.position);
            //    line.SetPosition(1, transform.position + transform.forward * 10f);
            //    StartCoroutine(DestroyObject(obj));
            //}



            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, interactableLayer))
            {
                //Debug.Log(hit.collider.gameObject);
                currentInteractable = hit.transform.GetComponent<FirstPersonInteractable>();
                //Debug.Log(currentInteractable);
                if (previousInteractable != currentInteractable)
                {
                    currentInteractable.HoverIn();
                    if (previousInteractable != null)
                    {
                        previousInteractable.HoverOut();
                    }
                } else
                {
                    currentInteractable.Hover();
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    //lr.enabled = true;
                    if (pressedInteractable != null)
                    {
                        pressedInteractable = null;
                    }
                    else
                    {
                        currentInteractable.PressDown();
                        pressedInteractable = currentInteractable;
                    }
                }

                if (Input.GetKey(KeyCode.E))
                {
                    if (currentInteractable == pressedInteractable)
                    {
                        currentInteractable.Press();
                    }
                }

                if (Input.GetKeyUp(KeyCode.E))
                {
                    //lr.enabled = false;
                    if (currentInteractable == pressedInteractable)
                    {
                        currentInteractable.PressUp();
                    }
                    pressedInteractable = null;
                }
            }
            else
            {
                if(previousInteractable != null)
                {
                    previousInteractable.HoverOut();
                }
                currentInteractable = null;
            }
            
            previousInteractable = currentInteractable;
        }
        
    }



    public void AttachObject(FirstPersonInteractable interactable)
    {
        attachedInteractableParent = interactable.transform.parent;
        interactable.transform.parent = objectAttachPoint;
        if (interactable.attachmentPoint)
        {
            interactable.transform.localEulerAngles = -interactable.attachmentPoint.localEulerAngles;
            interactable.transform.position += objectAttachPoint.transform.position - interactable.attachmentPoint.position;
        }
        else
        {
            interactable.transform.localPosition = Vector3.zero;
        }
        interactable.enabled = false;
        attachedInteractable = interactable;
    }

    private void DetachObject()
    {
        attachedInteractable.transform.parent = attachedInteractableParent;
        attachedInteractable.enabled = true;
        attachedInteractable = null;
        attachedInteractableParent = null;
    }

    private void ThrowObject()
    {
        attachedInteractable.transform.parent = attachedInteractableParent;
        attachedInteractable.enabled = true;
        Rigidbody rigid = attachedInteractable.GetComponent<Rigidbody>();
        if (rigid != null)
        {
            rigid.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }
        attachedInteractable = null;
        attachedInteractableParent = null;
    }

    //private IEnumerator DestroyObject(GameObject object1)
    //{
    //    yield return new WaitForSeconds(2f);
    //    Destroy(object1);
    //}
}
