/*
 * This class is aimed to control player's
 * movement & rotation and keep all customizable
 * variables, related to them in it.
 */

using UnityEngine;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovementController : MonoBehaviour {

    //__________________________________________Global Variables________________________//
    //movement variables
    [Header("Walk")]
    [SerializeField] private float movSpeed = 5.0f;
    [Space(10)]
    [Header("Clamp Y pos")]
    [SerializeField] private float minY = -25.0f;
    [SerializeField] private float maxY = 2.0f;
    [Space(10)]
    [Header("Gravity")]
    [SerializeField] private float m_StickToGroundForce = 1.0f;
    [SerializeField] private float m_GravityMultiplier = 1.0f;
    [Space(10)]
    [SerializeField] private PlayerCameraController m_CamController;

    //components
    private CharacterController m_CharacterController;

    //global trackable variables
    private Vector3 m_MoveDir = Vector3.zero;
    private Vector2 m_Input;
    [SerializeField] private Camera m_Camera;




    //__________________________________________Behaviour Building________________________//

    //------------------------------------------------------Initialize
    //initialize values of variables that are required
    //for proper functioning of this controller
    public void Init()
    {
        m_CharacterController = GetComponent<CharacterController>();
        if (!m_Camera)
        {
            m_Camera = Camera.main;
        }
        m_CamController.Init(transform, m_Camera.transform);
    }



    //------------------------------------------------------Movement
    //--------------------------------------->Gravity<
    //control the Y position of a player.
    //-> imitate gravity
    //-> control the movement vector in air and on ground
    void UpdateYPos()
    {
        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }

        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, Mathf.Clamp(pos.y, minY, maxY), pos.z);
    }


    //--------------------------------------->Walking<
    //control the X & Z position of the player.
    //-> emitate walking
    void UpdateMove(float speed = 0)
    {           
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = Vector3.forward * m_Input.y + Vector3.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;

        UpdateYPos();

        m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
    }



    //--------------------------------------->Rotation<
    public void Rotate(float dY)
    {
        transform.localEulerAngles += new Vector3(0f, dY, 0f);
    }

    public void LookAt(Vector3 targetPoint, float rotSpeed = 2.0f)
    {
        Vector3 relativePos = targetPoint - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        Quaternion targetRotation = new Quaternion(0, rotation.y, 0, rotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
    }



    //------------------------------------------------------Input
    private void GetInput()
    {
        // Read axis input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
    }



    //------------------------------------------------------Main Thread
    // Update is called once per frame
    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        UpdateMove(movSpeed);
    }

    private void LateUpdate()
    {
        //update camera
        m_CamController.UpdatePosition();
        m_CamController.UpdateCasterPosition();
        m_CamController.UpdateRotation();
    }
}
