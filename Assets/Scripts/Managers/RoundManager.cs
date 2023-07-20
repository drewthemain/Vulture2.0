using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    #region Variables

    public static RoundManager _instance;

    public enum RoundState
    {
        InRound,
        InBetween,
        Paused,
        InMenu,
    }

    [Header("Options")]

    [Tooltip("The current state of the game")]
    [SerializeField] private RoundState _roundState = RoundState.InRound;

    [Tooltip("The amount of time inbetween rounds")]
    [SerializeField] private float _inBetweenLength = 15;

    [Tooltip("Should we fix windows in-between rounds?")]
    [SerializeField] private bool _windowFixPerRound = true;

    [Header("References")]

    [Tooltip("The list of rounds that runs the game loop")]
    [SerializeField] private List<Round> _rounds;

    // The current round
    private int _currentRound = 0;

    // The current segment within the round
    private int _currentSegment = 0;

    // The amount of total enemies remaining this round
    private int _totalEnemiesRemaining = 0;

    // The total enemies that are spawned in the world
    private int _totalEnemiesSpawned = 0;

    // The number of enemies left in this segment
    private int _segmentEnemiesRemaining = 0;

    // The timer for the in-between rounds sequence
    private float _inBetweenTimer = 0;

    // The number of enemies left before early spawning
    private int _spawningEarlyBuffer = 0;

    // The overall current number of rounds
    private int _totalCurrentRound = 1;

    private int _currentLoop = 1;


    #endregion

    #region Methods

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        if (_rounds.Count > 0)
        {
            ChangeRoundState(RoundState.InBetween);
        }
        else
        {
            Debug.LogWarning("RoundManager is missing rounds!");
        }
    }

    /// <summary>
    /// Changes the current state of the round with additonal logic
    /// </summary>
    /// <param name="newState">The new round state</param>
    public void ChangeRoundState(RoundState newState)
    {
        _roundState = newState;

        switch (_roundState)
        {
            case RoundState.InRound:

                // Loop the rounds, currently
                if (_currentRound >= _rounds.Count)
                {
                    _currentRound = 0;
                    _currentLoop++;
                    Debug.Log("Ran out of rounds! Restarting the sequence...");
                }

                // Calculate the total number of enemies this round
                _totalEnemiesRemaining = _rounds[_currentRound].GetTotalEnemies() * _currentLoop;

                Debug.Log($"Current Round: {_rounds[_currentRound].name}");

                // Set the segment value
                if (_rounds[_currentRound]._segments.Count > 0)
                {
                    _currentSegment = 0;
                }
                else
                {
                    Debug.LogWarning("Current round is missing segments!");
                }

                UIManager.instance.UpdateRound(_totalCurrentRound, _rounds[_currentRound].name, _currentLoop);

                SpawnSegment();

                break;

            case RoundState.InBetween:

                _inBetweenTimer = _inBetweenLength;

                UIManager.instance.UpdateRound(-1, "Prepare.", -1);

                if (_windowFixPerRound)
                {
                    SmartMap.instance.FixWindows(false, 1);
                }

                break;

            case RoundState.InMenu:
                break;

            case RoundState.Paused:
                break;
        }
    }


    private void Update()
    {
        switch (_roundState)
        {
            case RoundState.InRound:

                // Overall round check
                if (_totalEnemiesRemaining <= 0)
                {
                    Debug.Log($"Round {_rounds[_currentRound]} over!");
                    _currentRound++;
                    _totalCurrentRound++;
                    ChangeRoundState(RoundState.InBetween);

                    return;
                }

                // Early spawning block
                if (_segmentEnemiesRemaining < _rounds[_currentRound]._maxEnemiesSpawned)
                {
                    if (_currentSegment + 1 < _rounds[_currentRound]._segments.Count)
                    {
                        if (_rounds[_currentRound]._segments[_currentSegment + 1]._allowEarlySpawning)
                        {
                            Debug.Log($"Starting spawning of segment {_currentSegment + 1} early!");

                            _spawningEarlyBuffer = _segmentEnemiesRemaining;

                            _currentSegment += 1;
                            SpawnSegment();
                        }
                    }
                }

                // Segment check
                if (_segmentEnemiesRemaining <= 0)
                {
                    Debug.Log($"Segment {_currentSegment} of {_rounds[_currentRound]} over!");
                    _currentSegment += 1;
                    SpawnSegment();
                }

                break;

            case RoundState.InBetween:

                if (_inBetweenTimer <= 0)
                {
                    ChangeRoundState(RoundState.InRound);
                }

                _inBetweenTimer -= Time.deltaTime;

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
        return _roundState;
    }

    /// <summary>
    /// Called when an enemy dies, updates references
    /// </summary>
    /// <param name="enemyType">Type of enemy (currently unused)</param>
    public void RecordEnemyKill(Order.EnemyTypes enemyType)
    {
        _segmentEnemiesRemaining -= 1;
        _totalEnemiesRemaining -= 1;
        _totalEnemiesSpawned -= 1;

        // Keeps track of when the prior segment ends after early spawning
        if (_spawningEarlyBuffer > 0)
        {
            _spawningEarlyBuffer -= 1;

            if (_spawningEarlyBuffer <= 0)
            {
                Debug.Log($"Spawning early for segment {_currentSegment} has ended!");
                Debug.Log($"Segment {_currentSegment - 1} of {_rounds[_currentRound]} over!");
            }
        }
    }

    /// <summary>
    /// Updates counters when a new enemy is spawned
    /// </summary>
    public void RecordEnemySpawn()
    {
        _totalEnemiesSpawned += 1;
    }

    /// <summary>
    /// Send the current segment to the smartmap for spawning locations
    /// </summary>
    public void SpawnSegment()
    {
        _segmentEnemiesRemaining += _rounds[_currentRound]._segments[_currentSegment].GetTotalEnemies() * _currentLoop;
        SmartMap.instance.AcceptSegment(_rounds[_currentRound]._segments[_currentSegment], _currentLoop);
    }

    /// <summary>
    /// Determines whether there's too many enemies on the screen
    /// </summary>
    /// <returns>Whether more enemies can be spawned</returns>
    public bool CanSpawn()
    {
        if (_currentRound < _rounds.Count)
        {
            return _totalEnemiesSpawned < (_rounds[_currentRound]._maxEnemiesSpawned * _currentLoop) && _roundState != RoundState.Paused;
        }

        return false;
    }

    #endregion
}
