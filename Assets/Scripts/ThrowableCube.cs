using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableCube : MonoBehaviour
{
    public GameObject NoiseMaker;
    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(NoiseMaker, transform.position, Quaternion.identity);
    }
}
