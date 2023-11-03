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


    public int currentId = -1;
    private int previousId = -1;
    private int eventOverride = -1;
    private float eventChance = 0.3f;
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

    public string GetTargetComponent()
    {
        if (currentIndex == -1)
        {
            return null;
        }

        return events[currentIndex].component;
    }

    public void TransmitSubscribers(bool up)
    {
        invokeList = OnEventStart.GetInvocationList();
        foreach (System.Delegate d in invokeList)
        {
            Component target = (Component) d.Method.Invoke(d.Target, null);

            if (target == null || target is EventGateway)
            {
                continue;
            }

            FactoryChannels(target, up);
        }
    }

    public void TransmitSubscriber(EventGateway gateway)
    {
        Component target = (Component)gateway.TransmitTarget();

        if (target == null || target is EventGateway)
        {
            return;
        }

        FactoryChannels(target, true);
    }

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

    public Event GetEvent()
    {
        if (currentIndex == -1)
        {
            return null;
        }

        return events[currentIndex];
    }

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
