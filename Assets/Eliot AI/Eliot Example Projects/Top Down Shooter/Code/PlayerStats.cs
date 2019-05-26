using UnityEngine;

public class PlayerStats : MonoBehaviour {

    [Header("Health")]
    [SerializeField] private int max_hp = 3;
    [SerializeField] private int cur_hp;
    [Space(3)]

    [Header("Components & Objects")]
    public PlayerMovementController movementController;

    public Camera playerCamera;

    [Header("Game Over")] 
    public GameObject GameOverCanvas;
    public GameObject Ragdoll;

    public void Start()
    {
        cur_hp = max_hp;
        movementController.Init();
    }

    public void Damage(int power)
    {
        cur_hp -= power;
        if (cur_hp <= 0)
        {
            Instantiate(Ragdoll, movementController.transform.position + Vector3.up, movementController.transform.rotation);
            playerCamera.transform.parent = null;
            var goc = Instantiate(GameOverCanvas) as GameObject;
            goc.transform.SetParent(null, false);
            Time.timeScale = 0;
        }
    }
}
