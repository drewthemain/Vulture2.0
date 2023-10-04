using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Event : ScriptableObject
{
    #region Variables 

    [Header("Options")]

    [Tooltip("The name of the targeted component as a string")]
    public string component;

    [Tooltip("The name of the factory function as a string")]
    public string action;

    #endregion

    #region Methods

    #endregion
}
