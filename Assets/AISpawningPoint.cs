using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Eliot.Environment;
using Eliot.AgentComponents;

public class AISpawningPoint : Trigger
{
    public GameObject currentAgent;
    public GameObject agentPrefab;
    public WaypointsGroup wpg;

    public void RespawnAI()
    {
        if(currentAgent != null)
        {
            Destroy(currentAgent);
        }
        currentAgent = Instantiate(agentPrefab) as GameObject;
        currentAgent.GetComponent<Agent>().enabled = false;
        currentAgent.GetComponent<NavMeshAgent>().enabled = false;
        currentAgent.GetComponent<Agent>().Waypoints = wpg;
        currentAgent.transform.parent = this.transform;
        currentAgent.transform.localPosition = Vector3.zero;
        currentAgent.transform.localRotation = Quaternion.identity;
        currentAgent.GetComponent<Agent>().enabled = true;
        currentAgent.GetComponent<NavMeshAgent>().enabled = true;
    }

    private void Update()
    {
        isTriggered = (currentAgent == null);
    }
}
