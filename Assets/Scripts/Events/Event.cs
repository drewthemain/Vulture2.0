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

    #region Methods

    /// <summary>
    /// Strips the name and removes the ID
    /// </summary>
    /// <returns>The string portion of the name</returns>
    public string GetDisplayName()
    {
        int indexOf = name.IndexOf('_');
        if (indexOf >= 0)
        {
            return name.Substring(indexOf + 1);
        }

        return "";
    }

    /// <summary>
    /// Gets the id section of the name as an integer
    /// </summary>
    /// <returns>The ID of the event as an int</returns>
    public int GetId()
    {
        int indexOf = name.IndexOf('_');
        if (indexOf >= 0)
        {

            return int.Parse(name.Substring(0, indexOf));
        }

        return -1;
    }

    #endregion

}
