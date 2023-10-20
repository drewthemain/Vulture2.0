using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventGateway : MonoBehaviour
{
    #region Variables

    // INSTANTIATED PREFABS IN THE ROUND SHOULD BE TRUE
    [Tooltip("Should this query the EventManager for the event on start?")]
    [SerializeField] private bool transmitOnStart = false;

    #endregion

    #region Methods

    /// <summary>
    /// Adds the proper function call to the EventManager delegate 
    /// </summary>
    private void OnEnable()
    {
        EventManager.OnEventStart += TransmitTarget;
    }

    /// <summary>
    /// Removes the proper function call to the EventManager delegate 
    /// </summary>
    private void OnDisable()
    {
        EventManager.OnEventStart -= TransmitTarget;
    }

    private void Start()
    {
        if (transmitOnStart)
        {
            EventManager.instance.TransmitSubscriber(this);
        }
    }

    /// <summary>
    /// Passes the proper component to the EventManager for manipulation
    /// </summary>
    public Component TransmitTarget()
    {
        if (transmitOnStart)
        {
            transmitOnStart = false;
        }

        string target = EventManager.instance.GetTargetComponent();
        if (target == null)
        {
            return this;
        }

        return GetComponent(EventManager.instance.GetTargetComponent()) as Component;
    }

    #endregion
}
