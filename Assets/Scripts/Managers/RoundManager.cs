using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    #region Variables

    public static RoundManager instance;

    public enum RoundState
    {
        InRound,
        InBetween,
        Paused,
        InMenu,
    }

    [Header("Options")]

    [Tooltip("The current state of the game")]
    [SerializeField] private RoundState roundState = RoundState.InRound;

    [Tooltip("The amount of time inbetween rounds")]
    [SerializeField] private float inBetweenLength = 15;

    [Header("References")]

    [Tooltip("The list of rounds that runs the game loop")]
    [SerializeField] private List<Round> rounds;

    // Reference to the player health
    private PlayerHealth playerHealth;

    // The index in the list of the current round
    public int currentRound = 0;

    // The current segment within the round
    public int currentSegment = 0;

    // The amount of total enemies remaining this round
    public int totalEnemiesRemaining = 0;

    // The total enemies that are spawned in the world
    public int totalEnemiesSpawned = 0;

    // The number of enemies left in this segment
    public int segmentEnemiesRemaining = 0;

    // The timer for the in-between rounds sequence
    private float inBetweenTimer = 0;

    // The number of enemies left before early spawning
    public int spawningEarlyBuffer = 0;

    // The overall current number of rounds
    private int totalCurrentRound = 1;

    private int currentLoop = 1;

    private int nextRoundOverride = -1;


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
        if (rounds.Count > 0)
        {
            ChangeRoundState(RoundState.InBetween);
        }
        else
        {
            Debug.LogWarning("RoundManager is missing rounds!");
        }
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        if (playerHealth == null) { Debug.LogWarning("Missing a player in the scene!"); }
    }

    /// <summary>
    /// Changes the current state of the round with additonal logic
    /// </summary>
    /// <param name="newState">The new round state</param>
    public void ChangeRoundState(RoundState newState)
    {
        roundState = newState;

        switch (roundState)
        {
            case RoundState.InRound:

                // Loop the rounds, currently
                if (currentRound >= rounds.Count)
                {
                    currentRound = 0;
                    currentLoop++;
                    Debug.Log("Ran out of rounds! Restarting the sequence...".Color("red").Italic());
                }

                // Calculate the total number of enemies this round
                totalEnemiesRemaining = rounds[currentRound].GetTotalEnemies() * currentLoop;

                Debug.Log("Current Round: ".Color("white").Size(12) + $"{rounds[currentRound].GetDisplayName()}".Bold().Color("green").Size(13));

                // Set the segment value
                if (rounds[currentRound].segments.Count > 0)
                {
                    currentSegment = 0;
                }
                else
                {
                    Debug.LogWarning("Current round is missing segments!");
                }

                if (EventManager.instance)
                {
                    EventManager.instance.AugmentSubscribers();
                }

                Event currEvent = EventManager.instance.GetEvent();
                UIManager.instance.UpdateRound(totalCurrentRound, currEvent ? currEvent.GetDisplayName() : "", currEvent ? currEvent.description : "");

                SpawnSegment();

                break;

            case RoundState.InBetween:

                if (EventManager.instance)
                {
                    EventManager.instance.RestoreSubscribers();

                    if (rounds[currentRound].connectedEvent != -1)
                    {
                        EventManager.instance.SetOverride(rounds[currentRound].connectedEvent);
                    }
                }

                inBetweenTimer = inBetweenLength;

                UIManager.instance.UpdateRound(-1, "Prepare", "");

                GameManager.instance.ResetGravity();

                SmartMap.instance.FixWindows();

                EventManager.instance.EventChance();

                break;

            case RoundState.InMenu:
                break;

            case RoundState.Paused:
                break;
        }
    }


    private void Update()
    {
        switch (roundState)
        {
            case RoundState.InRound:

                // Overall round check
                if (totalEnemiesRemaining <= 0)
                {
                    Debug.Log($"Round {rounds[currentRound]} over!".Color("green").Bold());

                    if (nextRoundOverride == -1)
                    {
                        currentRound++;
                    }
                    else
                    {
                        currentRound = nextRoundOverride;
                        nextRoundOverride = -1;
                    }

                    totalCurrentRound++;
                    playerHealth.EndOfTurnHeal();
                    ChangeRoundState(RoundState.InBetween);

                    return;
                }

                // Early spawning block
                if (segmentEnemiesRemaining < rounds[currentRound].maxEnemiesSpawned)
                {
                    if (currentSegment + 1 < rounds[currentRound].segments.Count)
                    {
                        if (rounds[currentRound].segments[currentSegment + 1].allowEarlySpawning)
                        {
                            Debug.Log($"Starting spawning of segment {currentSegment + 1} early!".Color("yellow").Italic());

                            spawningEarlyBuffer = segmentEnemiesRemaining;

                            currentSegment += 1;
                            SpawnSegment();
                        }
                    }
                }

                // Segment check
                if (segmentEnemiesRemaining <= 0)
                {
                    Debug.Log($"Segment {currentSegment} of {rounds[currentRound]} over!".Color("yellow").Italic());
                    currentSegment += 1;
                    SpawnSegment();
                }

                break;

            case RoundState.InBetween:

                inBetweenTimer -= Time.deltaTime;

                if (inBetweenTimer <= 0)
                {
                    //Stop animations
                    //UIManager.instance.ToggleWarningBanner(false);
                    UIManager.instance.TriggerRoundAnimation(false, false);

                    ChangeRoundState(RoundState.InRound);
                }
                else if (inBetweenTimer <= 1)
                {
                    UIManager.instance.TriggerRoundAnimation(EventManager.instance.isEventHappening, true);

                    UIManager.instance.ToggleWarningBanner(false);
                }
                else if (inBetweenTimer <= 3 && EventManager.instance.isEventHappening)
                {
                    UIManager.instance.ToggleWarningBanner(true);
                }

                break;

            case RoundState.InMenu:
                break;

            case RoundState.Paused:
                break;
        }
    }

    /// <summary>
    /// Getter for current round state
    /// </summary>
    /// <returns>The round state</returns>
    public RoundState GetRoundState()
    {
        return roundState;
    }

    /// <summary>
    /// Called when an enemy dies, updates references
    /// </summary>
    /// <param name="enemyType">Type of enemy (currently unused)</param>
    public void RecordEnemyKill(Order.EnemyTypes enemyType)
    {
        segmentEnemiesRemaining -= 1;
        totalEnemiesRemaining -= 1;
        totalEnemiesSpawned -= 1;

        // Keeps track of when the prior segment ends after early spawning
        if (spawningEarlyBuffer > 0)
        {
            spawningEarlyBuffer -= 1;

            if (spawningEarlyBuffer <= 0)
            {
                Debug.Log($"Spawning early for segment {currentSegment} has ended!".Color("yellow").Italic());
                Debug.Log($"Segment {currentSegment - 1} of {rounds[currentRound]} over!".Color("yellow").Italic());
            }
        }
    }

    /// <summary>
    /// Updates counters when a new enemy is spawned
    /// </summary>
    public void RecordEnemySpawn()
    {
        totalEnemiesSpawned += 1;
    }

    /// <summary>
    /// Send the current segment to the smartmap for spawning locations
    /// </summary>
    public void SpawnSegment()
    {
        segmentEnemiesRemaining += rounds[currentRound].segments[currentSegment].GetTotalEnemies() * currentLoop;
        SmartMap.instance.AcceptSegment(rounds[currentRound].segments[currentSegment], currentLoop);
    }

    /// <summary>
    /// Determines whether there's too many enemies on the screen
    /// </summary>
    /// <returns>Whether more enemies can be spawned</returns>
    public bool CanSpawn()
    {
        if (currentRound < rounds.Count)
        {
            return totalEnemiesSpawned < (rounds[currentRound].maxEnemiesSpawned * currentLoop) && roundState != RoundState.Paused;
        }

        return false;
    }

    /// <summary>
    /// Force ends the current round and resets all values
    /// </summary>
    public void ForceQuit()
    {
        SmartMap.instance.ClearAll();

        totalEnemiesRemaining = 0;
        totalEnemiesSpawned = 0;
        segmentEnemiesRemaining = 0;
    }

    /// <summary>
    /// Sets an override for the next round
    /// </summary>
    /// <param name="roundId">The id of the next round</param>
    /// <returns>The name of the next round</returns>
    public string SetOverride(int roundId)
    {
        int nextIndex = GetRoundIndexById(roundId);

        if (nextIndex == -1)
        {
            Debug.LogWarning("Round override used an invalid ID");
            return "";
        }

        if (roundState == RoundState.InBetween)
        {
            currentRound = nextIndex;
        }
        else
        {
            nextRoundOverride = nextIndex;
        }

        return rounds[nextIndex].name;
    }

    public int GetRoundIndexById(int id)
    {
        for (int i = 0; i < rounds.Count; i++)
        {
            if (rounds[i].GetId() == id)
            {
                return i;
            }
        }

        return -1;
    }

    #endregion
}
