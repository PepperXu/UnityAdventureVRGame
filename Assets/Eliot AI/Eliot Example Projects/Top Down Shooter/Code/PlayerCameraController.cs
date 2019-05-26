using UnityEngine;
using System;

[Serializable] public class PlayerCameraController : MonoBehaviour {

    //__________________________________________Global Variables________________________//

    //movement variables
    private float rotY = 0;

    [Header("Rotate player Mode")]
    [SerializeField] private float lookAtSpeed = 5.0f;
    [SerializeField] private Camera cursorCaster;
    [Space(5)]

    //components
    [Header("Preinitializable components")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraTransform;
    private PlayerMovementController m_PlayerController;
    [Space(5)]

    //camera movement params
    [Header("Camera movement")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float followSpeed = 1.0f;
    [Space(5)]
    [SerializeField] private float distance = 15.0f;
    [Space(5)]
    [SerializeField] private Vector3 start_rot;
    private Vector3 start_cam_pos;


    //__________________________________________Behaviour Building________________________//

    //------------------------------------------------------Initialize
    public void Init(Transform playerT, Transform cameraT)
    {
        if(!playerTransform)
            playerTransform = playerT;
        if (!cameraTransform)
            cameraTransform = cameraT;
        start_cam_pos = cameraT.localPosition;

        m_PlayerController = player.GetComponent<PlayerMovementController>();
    }


    //------------------------------------------------------Main Movement Rules
    //--------------------------------------->Camera Transform<
    //----------------->Position<
    public void UpdatePosition()
    {
        if (!smoothFollow)
        {
            float cX = playerTransform.position.x + start_cam_pos.x;
            float cY = distance;
            float cZ = playerTransform.position.z + start_cam_pos.z;
            cameraTransform.position = new Vector3(cX, cY, cZ);
        }
        else
        {
            float cY = distance;
            float cX = Mathf.LerpUnclamped(
                cameraTransform.position.x, 
                playerTransform.position.x + start_cam_pos.x, 
                followSpeed * Time.deltaTime
                );
            float cZ = Mathf.LerpUnclamped(
                cameraTransform.position.z, 
                playerTransform.position.z + start_cam_pos.z, 
                followSpeed * Time.deltaTime
                );
            cameraTransform.position = new Vector3(cX, cY, cZ);
        }
    }

    public void UpdateCasterPosition()
    {
        float cX = playerTransform.position.x;
        float cY = distance;
        float cZ = playerTransform.position.z;
        cursorCaster.transform.position = new Vector3(cX, cY, cZ);
    }

    //----------------->Rotation<
    public void UpdateRotation()
    {
        //get mouse position in world coordinates
        Vector3 v3 = Input.mousePosition;
        v3.z = playerTransform.position.y + 15.0f;
        v3 = cursorCaster.ScreenToWorldPoint(v3);

        m_PlayerController.LookAt(v3, lookAtSpeed);
        Stabilize();
    }


    //prevents camera from rotation that is driven
    //by child-parent relationship and keeps 
    //its rotation static
    public void Stabilize(float y_rot)
    {
        if(cameraTransform.eulerAngles.y != rotY)
        {
            cameraTransform.eulerAngles = new Vector3(start_rot.x, y_rot, start_rot.y);
        }
    }
    public void Stabilize()
    {
        Stabilize(rotY);
    }

    private Coroutine moveCoroutine;

}