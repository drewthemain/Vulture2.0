using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollProhibitor : MonoBehaviour
{
    private Rigidbody body;
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (body)
        {
            body.velocity = Vector3.zero;
        }
    }
}
