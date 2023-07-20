using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    #region Variables

    [Header("References")]

    [Tooltip("The parent of the points this object will be bouncing between")]
    [SerializeField] private Transform _pointsParent;

    [Header("Options")]

    [Tooltip("The time it takes a target to move from point to point")]
    [SerializeField] private float _speed = 5f;

    [Tooltip("Should the points create a full loop? (if not, it back tracks)")]
    [SerializeField] private bool _isFullLoop = true;

    [Tooltip("The time to wait at every point")]
    [SerializeField] private float _waitTime = 2f;

    // The actual list of points
    private List<Vector3> _points = new List<Vector3>();

    // The current point (index) in the list
    private int _currPoint = 0;

    // The direction of the point flow (1 for forward, -1 for backward)
    private int _direction = 1;

    // The current timer of waiting at a point
    private float _currWaitTimer = 0;

    #endregion

    #region Methods

    private void Awake()
    {
        // Populate the points list
        foreach (Transform point in _pointsParent)
        {
            _points.Add(point.position);
        }
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _points[_currPoint]) > 0.2f)
        {
            float step = _speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _points[_currPoint], step);
        }
        else
        {
            // Wait at point!
            if (_currWaitTimer > _waitTime)
            {
                NextPoint();
            }

            _currWaitTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// Chooses the next point in the list of available stops
    /// </summary>
    private void NextPoint()
    {
        if (_isFullLoop)
        {
            _currPoint = _currPoint >= _points.Count - 1 ? 0 : _currPoint + 1;
        }
        else
        {
            // Determines the direction of the current track

            if (_currPoint >= _points.Count - 1)
            {
                _direction = -1;
            }
            else if (_currPoint <= 0)
            {
                _direction = 1;
            }

            _currPoint += _direction;
        }

        _currWaitTimer = 0;
    }

    #endregion
}
