using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolMover : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("List of patrol points to traverse")]
    [SerializeField] private Transform _pointsParent;

    [Tooltip("Should the points list wrap-around, or backtrack?")]
    [SerializeField] private bool _wrapAround = true;

    // Is the guard currently moving?
    private bool _isMoving = true;

    // Current index of the patrol point
    private int _currPoint = -1;

    // The current running timer for waiting
    private float _currWaitTimer = 0f;

    // The time to wait at the current point (pulled from the PatrolPoint script)
    private float _currWaitTime = 0f;

    // If no wraparound points, helps keep track of the current direction of the list
    private int _pathDir = 1;

    // Reference to the NavMeshAgent for movement
    private NavMeshAgent _agent;

    // The actual list of patrol points
    private List<PatrolPoint> _points = new List<PatrolPoint>();

    #endregion

    #region Methods

    private void Awake()
    {
        // Sets agent values
        _agent = GetComponent<NavMeshAgent>();

        if (_pointsParent == null)
        {
            Debug.LogWarning($"Mover '{transform.name}' is missing a patrol parent!");
        }

        // Add all points to overall list
        foreach (Transform point in _pointsParent)
        {
            _points.Add(point.GetComponent<PatrolPoint>());
        }

        if (_points.Count != 0)
        {
            NextPoint();
        }
        else
        {
            Debug.LogWarning($"Mover '{transform.name}' is missing patrol points!");
        }
    }

    private void Update()
    {
        if (_points.Count != 0)
        {
            MovementCheck();
        }
    }

    /// <summary>
    /// Handles the waiting sequence of the patrolling state
    /// </summary>
    public void MovementCheck()
    {
        // If agent stops moving (while patrolling), start wait sequence
        if (_agent.velocity.magnitude == 0)
        {
            // Resets wait timers
            if (_isMoving)
            {
                _isMoving = false;

                _currWaitTime = _points[_currPoint]._waitTime;
                _currWaitTimer = 0f;
            }

            _currWaitTimer += Time.deltaTime;

            // Decide next move after waiting is done
            if (_currWaitTimer >= _currWaitTime)
            {
                NextPoint();
            }
        }
    }

    /// <summary>
    /// Chooses the next patrol point and updates agent
    /// </summary>
    public void NextPoint()
    {
        if (_wrapAround)
        {
            _currPoint = _currPoint >= _points.Count - 1 ? 0 : _currPoint + 1;
        }
        else
        {
            // Determines the direction of the current track

            if (_currPoint >= _points.Count - 1)
            {
                _pathDir = -1;
            }
            else if (_currPoint <= 0)
            {
                _pathDir = 1;
            }

            _currPoint += _pathDir;
        }

        // Just in case a PatrolPoint is missing
        if (_points[_currPoint] is null)
        {
            NextPoint();
            return;
        }

        // Update agent with it's next destination
        _isMoving = true;

        _agent.SetDestination(_points[_currPoint].transform.position);
    }

    #endregion
}
