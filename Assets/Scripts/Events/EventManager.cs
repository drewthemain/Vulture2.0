using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    #region Variables

    public static EventManager instance;

    [Header("Events")]

    [Tooltip("The list of available events to choose from")]
    [SerializeField] private List<Event> events;

    private int currentIndex = 0;

    public delegate Component EventStart();
    public static event EventStart OnEventStart;

    public delegate void EventEnd();
    public static event EventEnd OnEventEnd;

    private System.Delegate[] invokeList;

    #endregion

    #region Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
        } 
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
    }

    public string GetTargetComponent()
    {
        return events[currentIndex].component;
    }

    public void TransmitSubscribers(bool up)
    {
        invokeList = OnEventStart.GetInvocationList();
        foreach (System.Delegate d in invokeList)
        {
            Component target = (Component) d.Method.Invoke(d.Target, null);
            FactoryChannels(target, up);
        }
    }

    public void TransmitSubscriber(EventGateway gateway)
    {
        Component target = (Component)gateway.TransmitTarget();
        FactoryChannels(target, true);
    }

    public void FactoryChannels(Component target, bool up)
    {
        switch (target)
        {
            case Health:

                switch (events[currentIndex].action)
                {
                    case "AlterMaxHealth":
                        EventFactory.AlterMaxHealth((Health)target, up);
                        break;

                }
                break;
        }
    }

    public void AugmentSubscribers()
    {
        Debug.Log($"Starting event: {events[currentIndex].name}");
        TransmitSubscribers(true);
    }

    public void RestoreSubscribers()
    {
        Debug.Log($"Ending event: {events[currentIndex].name}");
        TransmitSubscribers(false);
    }


    #endregion
}
