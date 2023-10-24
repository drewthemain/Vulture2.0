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


    public int currentIndex = -1;
    private int previousIndex = -1;
    private int indexOverride = -1;
    private float eventChance = 0.3f;

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
        if (chanceCheck >= eventChance && indexOverride == -1)
        {
            isEventHappening = false;
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
            if (indexOverride == -1)
            {
                currentIndex = Random.Range(0, events.Count);
            }
            else
            {
                currentIndex = indexOverride;
                indexOverride = -1;
                break;
            }

            if (events.Count <= 1 || safety >= 50)
            {
                break;
            }

            safety++;
        } 
        while (currentIndex == previousIndex);

        Debug.Log("Starting event: ".Color("white").Size(12) +  $"{events[currentIndex].name}".Bold().Color("orange").Size(13));
        TransmitSubscribers(true);
    }

    public void RestoreSubscribers()
    {
        if (currentIndex == -1)
        {
            return;
        }

        Debug.Log("Ending event: ".Color("white").Size(12) + $"{events[currentIndex].name}".Bold().Color("orange").Size(13));
        TransmitSubscribers(false);

        previousIndex = currentIndex;
        currentIndex = -1;
    }

    public string SetOverride(int nextIndex)
    {
        indexOverride = nextIndex;

        return events[indexOverride].name;
    }

    public Event GetEvent()
    {
        if (currentIndex == -1)
        {
            return null;
        }

        return events[currentIndex];
    }


    #endregion
}
