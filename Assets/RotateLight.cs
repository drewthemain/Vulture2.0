using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLight : MonoBehaviour
{
    //public GameObject light;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(speed*Time.deltaTime, 0, 0, Space.Self);
    }
}
