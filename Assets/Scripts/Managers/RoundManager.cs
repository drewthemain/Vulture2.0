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

    // The current round
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

                Debug.Log("Current Round: ".Color("white").Size(12) + $"{rounds[currentRound].name}".Bold().Color("green").Size(13));

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

                UIManager.instance.UpdateRound(totalCurrentRound, rounds[currentRound].name, currentLoop);

                SpawnSegment();

                break;

            case RoundState.InBetween:

                if (EventManager.instance)
                {
                    EventManager.instance.RestoreSubscribers();
                }

                inBetweenTimer = inBetweenLength;

                UIManager.instance.UpdateRound(-1, "Prepare.", -1);

                GameManager.instance.ResetGravity();

                SmartMap.instance.FixWindows();

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
                    currentRound++;
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

                if (inBetweenTimer <= 0)
                {
                    UIManager.instance.ToggleWarningBanner(false);

                    ChangeRoundState(RoundState.InRound);
                }
                else if (inBetweenTimer <= 2)
                {
                    UIManager.instance.ToggleWarningBanner(true);
                }

                inBetweenTimer -= Time.deltaTime;

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

    #endregion
}
