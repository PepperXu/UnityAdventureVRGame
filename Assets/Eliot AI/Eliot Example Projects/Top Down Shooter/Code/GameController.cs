using System.Linq;
using Eliot.AgentComponents;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    [Header("Init Player")]
    public GameObject GamePrefab;
    public Slider slider;
    public GameObject MenuCanvas;

    public bool CanReset = true;
    public bool Paused = false;
    public bool ufps = false;
    
    private void Start()
    {
        foreach (Agent agent in FindObjectsOfType<Agent>())
            agent.Resources.Death.SetOnDeathAction(gameObject, "BanditIsDead");
    }

    private void Update()
    {
        if (CanReset && Input.GetKeyDown(KeyCode.R))
            Reset();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!ufps)
            {
                if (Paused)
                {
                    MenuCanvas.SetActive(false);
                    Time.timeScale = 1f;
                    Paused = false;
                }
                else
                {
                    MenuCanvas.SetActive(true);
                    Time.timeScale = 0f;
                    Paused = true;
                }
            }
            else
            {
                if (Paused)
                {
                    //Screen.lockCursor = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    MenuCanvas.SetActive(true);
                    Time.timeScale = 1f;
                    Paused = false;
                }
                else
                {
                    //Screen.lockCursor = false;
                    Cursor.lockState = CursorLockMode.None;
                    MenuCanvas.SetActive(true);
                    Time.timeScale = 0f;
                    Paused = true;
                }
            }
        }

        if (ufps && Time.timeScale == 0f)
            //Screen.lockCursor = false;
            Cursor.lockState = CursorLockMode.None;
    }

    public void Reset()
    {
        var protectedObjects = (from go in MenuCanvas.GetComponentsInChildren<Transform>() select go.gameObject).ToList();
        protectedObjects.Add(gameObject);
        foreach (GameObject obj in FindObjectsOfType(typeof(GameObject)))
            if(obj && !protectedObjects.Contains(obj)) 
                Destroy(obj);
        
        if(GamePrefab) Instantiate(GamePrefab);
        
        foreach (Agent agent in FindObjectsOfType<Agent>())
            agent.Resources.Death.SetOnDeathAction(gameObject, "BanditIsDead");
        
        Time.timeScale = 1f;
        GetComponent<tds_WinController>().NumberOfBandits = 13;
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void SetDifficulty()
    {
        var difficulty = Mathf.Clamp(0.33f - slider.value*0.33f, 0f, 0.3f);
        foreach (Agent agent in FindObjectsOfType<Agent>())
            agent.Ping = difficulty;
    }
}
