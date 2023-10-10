using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[CreateAssetMenu]
public class Event : ScriptableObject
{
    #region Variables 

    [Header("Options")]

    [Tooltip("The name of the targeted component as a string")]
    public string component;

    [Tooltip("The name of the factory function as a string")]
    public string action;

    [Tooltip("A description for this action")]
    public string description;

    [UDictionary.Split(50, 50)]
    [Tooltip("The mapping of parameters that an event can have")]
    public UDictionary optionalParameters;

    [Serializable]
    public class UDictionary : UDictionary<string, Value> { }

    [Serializable]
    public class Value
    {
        [Tooltip("An optional component reference")]
        public Component component;
        [Tooltip("An optional float field")]
        public float number;
        [Tooltip("An optional string field")]
        public string quote;
    }

    #endregion

}
