using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    #region Variables

    [Header("References")]

    [Tooltip("The parent of the points this object will be bouncing between")]
    [SerializeField] private Transform pointsParent;

    [Header("Options")]

    [Tooltip("The time it takes a target to move from point to point")]
    [SerializeField] private float speed = 5f;

    [Tooltip("Should the points create a full loop? (if not, it back tracks)")]
    [SerializeField] private bool isFullLoop = true;

    [Tooltip("The time to wait at every point")]
    [SerializeField] private float waitTime = 2f;

    // The actual list of points
    private List<Vector3> points = new List<Vector3>();

    // The current point (index) in the list
    private int currPoint = 0;

    // The direction of the point flow (1 for forward, -1 for backward)
    private int direction = 1;

    // The current timer of waiting at a point
    private float currWaitTimer = 0;

    #endregion

    #region Methods

    private void Awake()
    {
        // Populate the points list
        foreach (Transform point in pointsParent)
        {
            points.Add(point.position);
        }
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, points[currPoint]) > 0.2f)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, points[currPoint], step);
        }
        else
        {
            // Wait at point!
            if (currWaitTimer > waitTime)
            {
                NextPoint();
            }

            currWaitTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// Chooses the next point in the list of available stops
    /// </summary>
    private void NextPoint()
    {
        if (isFullLoop)
        {
            currPoint = currPoint >= points.Count - 1 ? 0 : currPoint + 1;
        }
        else
        {
            // Determines the direction of the current track

            if (currPoint >= points.Count - 1)
            {
                direction = -1;
            }
            else if (currPoint <= 0)
            {
                direction = 1;
            }

            currPoint += direction;
        }

        currWaitTimer = 0;
    }

    #endregion
}
