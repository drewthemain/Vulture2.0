using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CURRENTLY DEPRECATED
public class PatrolPoint : MonoBehaviour
{
    [Header("Options")]

    [Tooltip("The time an enemy will spend at this point")]
    public float waitTime = 3f;

    [Header("Debug Options")]

    [Tooltip("Should the sphere be visible while playing?")]
    [SerializeField] private bool visibleInGame = false;

    private void Start()
    {
        GetComponent<Renderer>().enabled = visibleInGame;
    }
}
