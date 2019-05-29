using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eliot.AgentComponents;
using Eliot.Environment;

public class PlayerManager : MonoBehaviour
{
    public Agent agent;
    public Unit unit;
    public GameObject[] controllers;
    public CharacterController cc;
    public ArmSwing armswing;
    public GameObject fadeScreen;

    // Update is called once per frame
    void Update()
    {
        if(agent.Resources.HealthPoints <= 0f)
        {
            unit.Type = UnitType.Corpse;
            foreach(GameObject obj in controllers)
            {
                obj.SetActive(false);
            }
            armswing.enabled = false;
            fadeScreen.SetActive(true);
            if (cc)
            {
                cc.enabled = false;
            }
        }
    }

    public void Respawn()
    {
        GameManager.lastCheckPoint.ResetState();
        transform.position = GameManager.lastCheckPoint.respawnPoint.position;
        agent.Resources.AddHealth(100);
        unit.Type = UnitType.Agent;
        foreach (GameObject obj in controllers)
        {
            obj.SetActive(true);
        }
        armswing.enabled = true;
        fadeScreen.SetActive(false);
        if (cc)
        {
            cc.enabled = true;
        }
    }
}
