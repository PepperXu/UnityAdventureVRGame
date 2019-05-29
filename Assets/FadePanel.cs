using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanel : MonoBehaviour
{

    public PlayerManager pm;

    public void Respawn()
    {
        pm.Respawn();
    }
}
