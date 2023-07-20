using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    [Header("Options")]

    [Tooltip("The time an enemy will spend at this point")]
    public float _waitTime = 3f;

    [Header("Debug Options")]

    [Tooltip("Should the sphere be visible while playing?")]
    [SerializeField] private bool _visibleInGame = false;

    private void Start()
    {
        GetComponent<Renderer>().enabled = _visibleInGame;
    }
}
