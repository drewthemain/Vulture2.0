using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestructor : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("The amount of time until destruction")]
    [SerializeField] private float timeTilDestroy = 5;

    #endregion


    #region Methods

    private void Start()
    {
        Destroy(this.gameObject, timeTilDestroy);
    }

    #endregion
}
