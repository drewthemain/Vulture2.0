using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tracer : Enemy
{
    #region Variables

    public enum WallActions
    {
        Charge,
        Traverse,
        Climb,
        Drop,
    }

    public enum GroundActions
    {
        Charge,
        Zig,
        Zag,
    }

    [Header("General Tracer Options")]

    //[Tooltip("The damage done by one strike")]
    //[SerializeField] private float strikeDamage = 5f;

    [Tooltip("The time constraints of which a charge can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 chargeTimeConstraints;

    [Tooltip("Initial chances of moving from floor to wall or vice versa (should be small, grows over time")]
    [Range(0, 1)]
    [SerializeField] private float initialSwapOdds = 0.05f;

    [Tooltip("Growth to chances of swapping from floor to wall or vice versa")]
    [Range(0, 0.1f)]
    [SerializeField] private float swapOddsGrowth = 0.01f;

    [Header("Grounded Behavior")]

    [Tooltip("The current grounded action of the tracer")]
    [SerializeField] private GroundActions groundAction = GroundActions.Charge;

    [Tooltip("The max distance of a diagonal pattern")]
    [SerializeField] private float maxZigZagDistance = 10;

    [Tooltip("The time constraints of which a zigzag can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 zigZagTimeConstraints;

    [Header("Wall Behavior")]

    [Tooltip("The current on wall action of the tracer")]
    [SerializeField] private WallActions wallAction = WallActions.Charge;

    [Tooltip("The time constraints of which a climb can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 climbTimeConstraints;

    [Tooltip("The time constraints of which a drop can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 dropTimeConstraints;

    [Tooltip("The time constraints of which a traverse can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 traverseTimeConstraints;

    [Header("Layer Masks")]

    [Tooltip("The layermask for detecting walls to climb")]
    [SerializeField] private LayerMask wallMask;

    [Tooltip("The layermask for detecting floors")]
    [SerializeField] private LayerMask floorMask;

    // The target position reference for pathing (strafing)
    private Vector3 targetPosition;

    // The timer for each action
    private float actionTimer = 0;

    // The point in which a cover will be stopped
    private float actionLimit = 0;

    // Is the enemy currently grounded?
    public bool grounded = true;

    //// Was the enemy previously grounded when switching orientation?
    //private bool previousGrounded = true;

    // The current odds of choosing a cover
    private float swapOdds = 0.05f;

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();

        if (agent)
        {
            agent.updateRotation = true;
        }

        // Start by targeting the player
        if (playerRef)
        {
            target = playerRef;
        }

        ChangeState(EnemyStates.OutOfRange);
    }

    protected override void Update()
    {
        base.Update();

        GroundedCheck();

        switch (state)
        {
            case EnemyStates.OutOfRange:

                if (grounded)
                {
                    ThinkGrounded();
                    return;
                }

                ThinkWall();

                break;
            case EnemyStates.InRange:

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Covering:

                if (Vector3.Distance(transform.position, targetPosition) < 2f)
                {
                    ChangeState(EnemyStates.OutOfRange);
                }

                break;
        }
    }

    public override void ChangeState(EnemyStates newState)
    {
        base.ChangeState(newState);

        // Resets (clears out) the path of the navmesh for every change
        if (agent != null)
        {
            agent.ResetPath();
            target = null;
            targetPosition = Vector3.zero;

            // Resets all timers
            actionTimer = 0;
            actionLimit = 0;
        }

        switch (state)
        {
            case EnemyStates.OutOfRange:

                if (playerRef)
                {
                    target = playerRef;
                }

                break;
            case EnemyStates.InRange:
                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Covering:

                TriggerSwap();

                break;
        }
    }

    /// <summary>
    /// Changes the ground action based on parameter
    /// </summary>
    /// <param name="newAction">The new action to perform</param>
    private void ChangeGroundAction(GroundActions newAction)
    {
        // Resets all action values
        groundAction = newAction;
        agent.ResetPath();
        agent.speed = baseSpeed;
        target = null;
        targetPosition = Vector3.zero;

        if (agent)
        {
            agent.updateRotation = true;
        }

        float swapRoll = Random.Range(0f, 1f);

        if (swapRoll < swapOdds)
        {
            // previousGrounded = true;
            ChangeState(EnemyStates.Covering);
            return;
        }

        swapOdds += swapOddsGrowth;

        switch (newAction)
        {
            case GroundActions.Charge:

                // Target the player
                actionLimit = Random.Range(chargeTimeConstraints.x, chargeTimeConstraints.y);
                target = playerRef;

                break;

            case GroundActions.Zig:

                if (agent)
                {
                    agent.updateRotation = false;
                }

                actionLimit = Random.Range(zigZagTimeConstraints.x, zigZagTimeConstraints.y);

                targetPosition = GetDiagonalPosition(true);
                agent.SetDestination(targetPosition);

                break;

            case GroundActions.Zag:

                if (agent)
                {
                    agent.updateRotation = false;
                }

                actionLimit = Random.Range(zigZagTimeConstraints.x, zigZagTimeConstraints.y);

                targetPosition = GetDiagonalPosition(false);
                agent.SetDestination(targetPosition);

                break;
        }
    }

    /// <summary>
    /// Changes the wall action based on parameter
    /// </summary>
    /// <param name="newAction">The new action to perform</param>
    private void ChangeWallAction(WallActions newAction)
    {
        // Resets all action values
        wallAction = newAction;
        agent.ResetPath();
        agent.speed = baseSpeed;
        target = null;
        targetPosition = Vector3.zero;

        float swapRoll = Random.Range(0f, 1f);

        if (swapRoll < swapOdds)
        {
            // previousGrounded = false;
            ChangeState(EnemyStates.Covering);
            return;
        }

        swapOdds += swapOddsGrowth;

        switch (newAction)
        {
            case WallActions.Charge:

                // Target the player
                actionLimit = Random.Range(chargeTimeConstraints.x, chargeTimeConstraints.y);
                target = playerRef;

                break;

            case WallActions.Climb:

                actionLimit = Random.Range(climbTimeConstraints.x, climbTimeConstraints.y);

                targetPosition = transform.position + (transform.forward * (Random.Range(5, maxZigZagDistance)));
                agent.SetDestination(targetPosition);

                break;

            case WallActions.Drop:

                actionLimit = Random.Range(dropTimeConstraints.x, dropTimeConstraints.y);

                targetPosition = transform.position + (-transform.forward * (Random.Range(5, maxZigZagDistance)));
                agent.SetDestination(targetPosition);

                break;

            case WallActions.Traverse:

                actionLimit = Random.Range(traverseTimeConstraints.x, traverseTimeConstraints.y);

                targetPosition = transform.position + ((Random.Range(0, 2) * 2 - 1) * transform.right * (Random.Range(5, maxZigZagDistance)));
                agent.SetDestination(targetPosition);

                break;
        }
    }

    /// <summary>
    /// Handles constant logic for the in-range status
    /// </summary>
    private void ThinkGrounded()
    {
        // Checking if the current action is finished
        // If so, start new action
        if (actionTimer > actionLimit)
        {
            actionTimer = 0;

            ChangeGroundAction((GroundActions)Random.Range(0, 2));
        }

        // Constant behavior for every action
        switch (groundAction)
        {
            case GroundActions.Zig:

                targetPosition = GetDiagonalPosition(true);
                agent.SetDestination(targetPosition);

                transform.LookAt(playerRef);

                break;
            case GroundActions.Zag:

                targetPosition = GetDiagonalPosition(false);
                agent.SetDestination(targetPosition);

                transform.LookAt(playerRef);

                break;

            case GroundActions.Charge:

                if (agent && target != null)
                {
                    agent.SetDestination(target.position);
                }

                break;
        }

        // Update the timer to increment between actions
        actionTimer += Time.deltaTime;
    }

    /// <summary>
    /// Handles constant logic for the in-range status
    /// </summary>
    private void ThinkWall()
    {
        // Checking if the current action is finished
        // If so, start new action
        if (actionTimer > actionLimit)
        {
            actionTimer = 0;

            ChangeWallAction((WallActions)Random.Range(0, 4));
        }

        // Constant behavior for every action
        switch (wallAction)
        {
            case WallActions.Charge:
                break;
            case WallActions.Climb:
                break;
            case WallActions.Drop:
                break;
            case WallActions.Traverse:
                break;
        }

        // Update the timer to increment between actions
        actionTimer += Time.deltaTime;
    }

    /// <summary>
    /// Gets a diagonal position based on zig zag patterns
    /// </summary>
    /// <param name="isLeft">Is the enemy tying to diagonal left?</param>
    /// <returns>A position of the target diagonal location</returns>
    private Vector3 GetDiagonalPosition(bool isLeft)
    {

        Vector3 half = (playerRef.position + transform.position) / 2;
        Vector3 directDir = playerRef.position - transform.position;
        Vector3 dir = Vector3.Cross(directDir, Vector3.up).normalized;

        if (!isLeft)
        {
            dir *= -1;
        }

        Vector3 offset = half + (dir * Random.Range(5, maxZigZagDistance));

        return offset;
    }

    /// <summary>
    /// Triggers a swap from floor to wall (or vice versa)
    /// </summary>
    private void TriggerSwap()
    {
        swapOdds = initialSwapOdds;

        if (grounded)
        {
            targetPosition = GetWallPosition();

            if (targetPosition == Vector3.zero)
            {
                ChangeState(EnemyStates.OutOfRange);
                return;
            }

            agent.SetDestination(targetPosition);
        }
    }

    /// <summary>
    /// Gets a wall position for the enemy to initially target
    /// </summary>
    /// <returns>Position on a wall of a viable space</returns>
    private Vector3 GetWallPosition()
    {
        Vector3 upVec = new Vector3(0, Random.Range(agent.height, agent.height * 2), 0);

        RaycastHit hit;

        Vector3 directDir = (playerRef.position + upVec) - (transform.position + upVec);
        Vector3 dir = Vector3.Cross(directDir, Vector3.up).normalized;

        // Left or right direction
        dir *= (Random.Range(0, 2) * 2 - 1);

        Debug.DrawRay((transform.position + upVec), dir * 100, Color.green);

        if (Physics.Raycast((transform.position + upVec), dir, out hit, 100, wallMask, QueryTriggerInteraction.Ignore))
        {
            NavMeshPath path = new NavMeshPath();

            GameObject test = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            test.transform.position = hit.point;

            if (agent.CalculatePath(hit.point, path))
            {
                return hit.point;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Checks whether the enemy is currently grounded
    /// </summary>
    private void GroundedCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, agent.height, floorMask))
        {
            grounded = true;
            return;
        }

        grounded = false;
    }

    /// <summary>
    /// CheckProximity override that has different logic for covering
    /// </summary>
    protected override void CheckProximity()
    {
        switch (state)
        {
            case EnemyStates.OutOfRange:

                if (distanceFromPlayer <= inRangeProximity && (playerInSight || distanceFromPlayer < 2f))
                {
                    ChangeState(EnemyStates.InRange);
                }
                return;

            case EnemyStates.InRange:

                if (distanceFromPlayer > inRangeProximity)
                {
                    ChangeState(EnemyStates.OutOfRange);
                }
                return;

            case EnemyStates.Covering:

                return;
        }
    }

    #endregion
}
