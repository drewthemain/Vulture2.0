using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    // User Inputs
    public bool useRandFrequency;
    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    // Position Storage Variables
    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    // Use this for initialization
    void Start()
    {
        gameObject.tag = "Floater";
        // Store the starting position & rotation of the object
        posOffset = transform.position;

        if (useRandFrequency)
        {
            frequency = Random.Range(0.25f, 0.80f);
        }

        foreach (Transform t in transform)
        {
            t.gameObject.tag = "Floater";
            foreach (Transform child in t)
            {
                child.gameObject.tag = "Floater";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Spin object around Y-Axis
        transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);

        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;
    }
}
