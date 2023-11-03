using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EventManager : MonoBehaviour
{
    #region Variables

    public static EventManager instance;

    [Header("Events")]

    [Tooltip("The list of available events to choose from")]
    [SerializeField] private List<Event> events;

    [Tooltip("The base chances of a random event occuring")]
    [SerializeField] private float baseEventChance = 0.3f;

    [Tooltip("The growth in chances after a non-event round")]
    [SerializeField] private float eventChanceGrowth = 0.1f;

    public bool isEventHappening = false;

    // The id of the event (part of it's name)
    public int currentId = -1;
    // The previous event id, saved for anti-repeating
    private int previousId = -1;
    // An optional override to set the event
    private int eventOverride = -1;
    // The actual chances of an event
    private float eventChance = 0.3f;
    // The current index of the event in the list
    private int currentIndex = -1;

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
            eventChance = baseEventChance;
            
        } 
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
    }

    /// <summary>
    /// Gets a string representation of the component to be targeted
    /// </summary>
    /// <returns>A component name, or null if no index</returns>
    public string GetTargetComponent()
    {
        if (currentIndex == -1)
        {
            return null;
        }

        return events[currentIndex].component;
    }

    /// <summary>
    /// Accepts all subscribers and facilitiates the factory to begin workingt on the components
    /// </summary>
    /// <param name="up">Whether we are starting or ending the event</param>
    public void TransmitSubscribers(bool up)
    {
        invokeList = OnEventStart.GetInvocationList();
        foreach (System.Delegate d in invokeList)
        {
            Component target = (Component) d.Method.Invoke(d.Target, null);

            // EventGateway is given to cancel the subscription
            if (target == null || target is EventGateway)
            {
                continue;
            }

            FactoryChannels(target, up);
        }
    }

    /// <summary>
    /// Transmits only one subscriber, called on spawned entities during runtime
    /// Brings a spawned entity "up to speed" on the state of events
    /// </summary>
    /// <param name="gateway">The spawned entities gateway</param>
    public void TransmitSubscriber(EventGateway gateway)
    {
        Component target = (Component)gateway.TransmitTarget();

        if (target == null || target is EventGateway)
        {
            return;
        }

        FactoryChannels(target, true);
    }

    /// <summary>
    /// Does the nasty work of widdling down each event to a proper factory
    /// </summary>
    /// <param name="target">The target component to be passed to a factory</param>
    /// <param name="up">Whether the event is starting or stopping</param>
    public void FactoryChannels(Component target, bool up)
    {
        if (currentIndex == -1)
        {
            return;
        }

        Event.UDictionary parameters = events[currentIndex].optionalParameters;

        switch (target)
        {
            case Health:

                switch (events[currentIndex].action)
                {
                    case "IncreaseEnemyMaxHealth":
                        EventFactory.IncreaseEnemyMaxHealth((EnemyHealth)target, up, parameters);
                        break;

                    default:
                        Debug.Log("No factory function found. Make sure the action in your event matches the name of the factory function, and it has an entry in FactoryChannels!");
                        break;
                }
                break;

            case NavMeshAgent:

                switch (events[currentIndex].action)
                {
                    case "EnemySpeedBoost":
                        EventFactory.EnemySpeedBoost((NavMeshAgent)target, up, parameters);
                        break;

                    default:
                        Debug.Log("No factory function found. Make sure the action in your event matches the name of the factory function, and it has an entry in FactoryChannels!");
                        break;
                }
                break;

            case Enemy:

                switch (events[currentIndex].action)
                {
                    case "ToggleDoubleBullets":
                        EventFactory.ToggleDoubleBullets((Soldier)target, up, parameters);
                        break;

                    default:
                        Debug.Log("No factory function found. Make sure the action in your event matches the name of the factory function, and it has an entry in FactoryChannels!");
                        break;
                }
                break;

            case GameManager:

                switch (events[currentIndex].action)
                {
                    case "ToggleGravity":
                        EventFactory.ToggleGravity(up, parameters);
                        break;

                    default:
                        Debug.Log("No factory function found. Make sure the action in your event matches the name of the factory function, and it has an entry in FactoryChannels!");
                        break;
                }
                break;

            case PlayerController:

                switch (events[currentIndex].action)
                {
                    case "BoostOnDamage":
                        EventFactory.BoostOnDamage((PlayerController)target, up, parameters);
                        break;

                    case "PlayerSpeedBoost":
                        EventFactory.PlayerSpeedBoost((PlayerController)target, up, parameters);
                        break;

                    default:
                        Debug.Log("No factory function found. Make sure the action in your event matches the name of the factory function, and it has an entry in FactoryChannels!");
                        break;
                }
                break;

        }
    }

    /// <summary>
    /// Calculates the chances of an event happening
    /// </summary>
    public void EventChance()
    {
        float chanceCheck = Random.Range(0f, 1f);
        if (chanceCheck >= eventChance && eventOverride == -1)
        {
            isEventHappening = false;
            currentId = -1;
            currentIndex = -1;
            eventChance += eventChanceGrowth;
            return;
        }

        isEventHappening = true;
    }

    /// <summary>
    /// Kicks off the event process, calculates next event (if there is one) and transmits result to subscribers
    /// </summary>
    public void AugmentSubscribers()
    {
        if (events.Count == 0)
        {
            Debug.Log("Events are turned on, but missing events!");
            return;
        }

        if (!isEventHappening)
        {
            return;
        }


        int safety = 0;
        do
        {
            if (eventOverride == -1)
            {
                currentId = events[Random.Range(0, events.Count)].GetId();
            }
            else
            {
                currentId = eventOverride;
                eventOverride = -1;
                break;
            }

            if (events.Count <= 1 || safety >= 50)
            {
                break;
            }

            safety++;
        } 
        while (currentId == previousId);

        currentIndex = GetEventIndexById(currentId);

        Debug.Log("Starting event: ".Color("white").Size(12) +  $"{events[currentIndex].GetDisplayName()}".Bold().Color("orange").Size(13));
        TransmitSubscribers(true);
    }

    /// <summary>
    /// Turns the event off for all subscribers
    /// </summary>
    public void RestoreSubscribers()
    {
        if (currentId == -1)
        {
            return;
        }

        Debug.Log("Ending event: ".Color("white").Size(12) + $"{events[currentIndex].name}".Bold().Color("orange").Size(13));
        TransmitSubscribers(false);

        previousId = currentId;
        currentId = -1;
        currentIndex = -1;
    }

    /// <summary>
    /// Called through devlog or by a round, bypasses the chance system
    /// </summary>
    /// <param name="eventId">The event ID of the next event</param>
    /// <returns>The string name of the event</returns>
    public string SetOverride(int eventId)
    {
        int indexOverride = GetEventIndexById(eventId);

        if (indexOverride == -1)
        {
            return "";
        }

        eventOverride = eventId;

        return events[indexOverride].GetDisplayName();
    }

    /// <summary>
    /// Gets the current event
    /// </summary>
    /// <returns>An event instance</returns>
    public Event GetEvent()
    {
        if (currentIndex == -1)
        {
            return null;
        }

        return events[currentIndex];
    }

    /// <summary>
    /// Maps an events ID to it's place in the list
    /// </summary>
    /// <param name="id">The id of the event</param>
    /// <returns>The index in this instances event list</returns>
    public int GetEventIndexById(int id)
    {
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i].GetId() == id)
            {
                return i;
            }
        }

        return -1;
    }


    #endregion
}
