using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class CheckPoint : MonoBehaviour
{
    public Transform respawnPoint;
    public AISpawningPoint[] AiToRespawn;
    public ObjectResetter[] relatedStaticObjects;

    private void OnTriggerEnter(Collider other)
    {
        GameManager.lastCheckPoint = this;
        GetComponent<Collider>().enabled = false;
        Debug.Log("Game Saved");
    }

    public void ResetState()
    {
        foreach(AISpawningPoint sp in AiToRespawn)
        {
            sp.RespawnAI();
        }
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Ragdoll"))
        {
            Destroy(obj);
        }
        foreach(ObjectResetter obj in relatedStaticObjects)
        {
            obj.DelayedReset(0.2f);
        }
    }
}
