using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : NavMeshEnemy
{
    #region Variables

    public enum InRangeActions
    {
        StrafeLeft,
        StrafeRight,
        Charge,
        Stand,
        Back
    }

    public enum CoverActions
    {
        Shoot,
        Wait
    }

    [Header("In Range Behavior")]

    [Tooltip("The current in range action of the soldier")]
    [SerializeField] private InRangeActions inRangeAction = InRangeActions.StrafeLeft;

    [Tooltip("The range in which the player is too close to the enemy")]
    [SerializeField] private float closestDesiredRange = 10f;

    [Tooltip("The max distance a strafe can be performed")]
    [SerializeField] private float maxStrafeDistance = 10;

    [Tooltip("The time constraints of which a strafe can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 strafeTimeConstraints;

    [Tooltip("The time constraints of which a charge can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 chargeTimeConstraints;

    [Tooltip("The time constraints of which a stand can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 standTimeConstraints;

    [Tooltip("Should this enemy shoot while moving?")]
    [SerializeField] private bool shootWhileMoving = true;

    [Header("Cover Behavior")]

    [Tooltip("The current in range action of the soldier")]
    [SerializeField] private CoverActions coverAction = CoverActions.Shoot;

    [Tooltip("Initial chances of recieveing a cover action (should be small, grows over time")]
    [Range(0, 1)]
    [SerializeField] private float initialCoverOdds = 0.05f;

    [Tooltip("Growth to chances of recieveing a cover after every action")]
    [Range(0, 0.1f)]
    [SerializeField] private float coverOddsGrowth = 0.01f;

    [Tooltip("The time constraints of which a cover can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 coverTimeConstraints;

    // The target position reference for pathing (strafing)
    private Vector3 targetPosition;

    // The timer for each action
    private float actionTimer = 0;

    // The point in which a cover will be stopped
    private float actionLimit = 0;

    // An additional timer for more complex actions
    private float additionalTimer = 0;

    // An additional limit for more complex actions
    private float additionalLimit = 0;

    // Have the cover actions been started?
    private bool coverActionsStarted = false;

    // The current odds of choosing a cover
    private float coverOdds = 0.05f;

    #endregion

    #region Override Methods

    /// <summary>
    /// Changes and properly transitions all soldier states
    /// </summary>
    /// <param name="newState">The new state of this enemy</param>
    public override void ChangeState(EnemyStates newState)
    {
        // Resets (clears out) the path of the navmesh for every change
        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            target = null;
            targetPosition = Vector3.zero;
            agent.updateRotation = false;

            // Resets all timers
            actionTimer = 0;
            actionLimit = 0;
        }

        base.ChangeState(newState);

        switch (newState)
        {
            case EnemyStates.OutOfRange:

                // Initial following of the player
                if (agent)
                {
                    target = playerRef;
                    agent.SetDestination(target.position);
                    anim.SetFloat("Speed", 1);
                }

                break;

            case EnemyStates.InRange:

                // Turn firing back on if we weren't shooting while moving
                if (weapon && !shootWhileMoving && playerInSight)
                {
                    weapon.ToggleFiring(true);
                }

                anim.SetFloat("Speed", 1f);
                actionTimer = 0;

                break;

            case EnemyStates.NoGrav:

                if (weapon)
                {
                    weapon.ToggleFiring(false);
                }

                break;
            case EnemyStates.Action:

                // Query cover from room and move
                if (agent)
                {
                    anim.SetFloat("Speed", 1f);

                    if (currentRoom == null)
                    {
                        ChangeState(EnemyStates.InRange);
                        return;
                    }

                    target = currentRoom.QueryCover(playerRef, this.transform, sightMask);

                    if (target == null)
                    {
                        ChangeState(EnemyStates.InRange);
                        return;
                    }

                    if (weapon != null)
                    {
                        weapon.ToggleFiring(false);
                    }

                    agent.speed = baseSpeed * 1.5f;
                    actionLimit = Random.Range(coverTimeConstraints.x, coverTimeConstraints.y);

                    // Cover action resetting
                    coverOdds = initialCoverOdds;
                    coverActionsStarted = false;
                    additionalLimit = 0;
                    additionalTimer = 0;

                    agent.updateRotation = true;

                    agent.SetDestination(target.position);
                }

                break;
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // Start by targeting the player
        if (playerRef)
        {
            target = playerRef;
        }

        if (agent)
        {
            agent.updateRotation = false;
        }

        coverOdds = initialCoverOdds;

        ChangeState(EnemyStates.OutOfRange);
    }

    protected override void Update()
    {
        base.Update();

        if (playerRef)
        {
            switch (state)
            {
                case EnemyStates.OutOfRange:

                    // Soldiers chase the player when out of attack proximity
                    if (agent != null && target != null)
                    {
                        agent.SetDestination(playerRef.position - Vector3.up);
                        LockedLookAt(playerRef);
                    }

                    if (playerInSight && distanceFromPlayer < (inRangeProximity * 2))
                    {
                        if (!weapon.IsFiring())
                        {
                            weapon.ToggleFiring(true);
                        }
                    }
                    
                    break;
                case EnemyStates.InRange:

                    // Handle the logic for in-range thinking
                    ThinkInRange();

                    break;
                case EnemyStates.NoGrav:

                    break;
                case EnemyStates.Action:

                    if (!coverActionsStarted)
                    {
                        // If done moving to cover, start doing actions!
                        if (agent.remainingDistance < 2f)
                        {
                            coverActionsStarted = true;
                            agent.updateRotation = false;

                            additionalTimer = 0;
                            ChangeCoverAction((CoverActions)Random.Range(0, 2));
                        }
                    }
                    else
                    {
                        ThinkCover();
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Changes the current action within a cover status
    /// </summary>
    /// <param name="newAction">The new cover action to perform</param>
    private void ChangeCoverAction(CoverActions newAction)
    {
        coverAction = newAction;
        agent.ResetPath();
        additionalLimit = 0;
        additionalTimer = 0;

        switch (newAction)
        {
            case CoverActions.Shoot:

                additionalLimit = Random.Range(coverTimeConstraints.x / 2, coverTimeConstraints.y / 2);
                anim.SetFloat("Speed", 1f);

                break;
            case CoverActions.Wait:

                // Send the enemy back to cover
                if (agent && target)
                {
                    agent.SetDestination(target.position);
                    anim.SetFloat("Speed", 1f);
                }

                // Stop firing!
                if (weapon && !playerInSight)
                {
                    weapon.ToggleFiring(false);
                }

                // Wait currently has less credence than shoot
                additionalLimit = Random.Range(coverTimeConstraints.x / 4, coverTimeConstraints.y / 4);

                break;
        }
    }

    /// <summary>
    /// Handles constant logic for the cover status
    /// </summary>
    private void ThinkCover()
    {
        // Timer for the OVERALL cover status
        if (actionTimer >= actionLimit)
        {
            currentRoom.ReturnCover(target);
            ChangeState(EnemyStates.InRange);
        }

        // If player rushes enemy in cover, break out of cover
        if (distanceFromPlayer < closestDesiredRange / 2)
        {
            currentRoom.ReturnCover(target);
            ChangeState(EnemyStates.InRange);
            return;
        }

        // Timer for the actions within the cover
        if (additionalTimer >= additionalLimit)
        {
            additionalTimer = 0;
            ChangeCoverAction((CoverActions)Random.Range(0, 2));
        }

        switch (coverAction)
        {
            case CoverActions.Shoot:

                if (!playerInSight)
                {
                    if (agent)
                    {
                        agent.SetDestination(playerRef.position);
                        anim.SetFloat("Speed", 1f);
                    }
                }
                else
                {
                    // If player can be seen, stop and start shooting!
                    agent.ResetPath();

                    if (weapon)
                    {
                        weapon.ToggleFiring(true);
                        anim.SetFloat("Speed", 0);
                    }
                }

                LockedLookAt(playerRef);

                break;
            case CoverActions.Wait:

                LockedLookAt(playerRef);

                if (target)
                {
                    if (Vector3.Distance(target.position, transform.position) < 0.5f)
                    {
                        if (playerInSight)
                        {
                            ChangeState(EnemyStates.InRange);
                        }
                        else
                        {
                            if (weapon)
                            {
                                weapon.ToggleFiring(false);
                                anim.SetFloat("Speed", 0);
                            }
                        }
                    }
                }

                break;
        }

        actionTimer += Time.deltaTime;
        additionalTimer += Time.deltaTime;
    }

    /// <summary>
    /// Changes the in-range action based on parameter
    /// </summary>
    /// <param name="newAction">The new action to perform</param>
    private void ChangeAction(InRangeActions newAction)
    {
        // Resets all action values
        inRangeAction = newAction;
        agent.ResetPath();
        agent.speed = baseSpeed;
        target = null;
        targetPosition = Vector3.zero;

        if (weapon)
        {
            weapon.ToggleFiring(true);
        }

        // Only add to cover odds under normal circumstances
        if (inRangeAction != InRangeActions.Back && currentRoom != null)
        {
            float coverRoll = Random.Range(0f, 1f);

            if (coverRoll < coverOdds)
            {
                ChangeState(EnemyStates.Action);
                return;
            }

            coverOdds += coverOddsGrowth;
        }

        switch (newAction)
        {
            case InRangeActions.StrafeLeft:

                // Set new target position
                actionLimit = Random.Range(strafeTimeConstraints.x, strafeTimeConstraints.y);
                targetPosition = transform.position + (-transform.right * (Random.Range(5, maxStrafeDistance)));
                agent.SetDestination(targetPosition);

                anim.SetFloat("Speed", 1f);

                break;

            case InRangeActions.StrafeRight:

                // Set new target position
                actionLimit = Random.Range(strafeTimeConstraints.x, strafeTimeConstraints.y);
                targetPosition = transform.position + (transform.right * (Random.Range(5, maxStrafeDistance)));
                agent.SetDestination(targetPosition);

                anim.SetFloat("Speed", 1f);

                break;

            case InRangeActions.Charge:

                // Target the player
                actionLimit = Random.Range(chargeTimeConstraints.x, chargeTimeConstraints.y);
                target = playerRef;

                anim.SetFloat("Speed", 1f);

                break;

            case InRangeActions.Stand:

                actionLimit = Random.Range(standTimeConstraints.x, standTimeConstraints.y);

                anim.SetFloat("Speed", 0);

                break;

            case InRangeActions.Back:

                actionLimit = Random.Range(strafeTimeConstraints.x, strafeTimeConstraints.y);

                // Try to flee backwards, else take cover
                targetPosition = GetFleePosition();
                if (targetPosition == Vector3.zero)
                {
                    if (currentRoom != null)
                    {
                        ChangeState(EnemyStates.Action);
                        return;
                    }
                    else
                    {
                        // If no cover, strafe instead!
                        agent.speed = baseSpeed * 1.5f;
                        targetPosition = transform.position + ((Random.Range(0, 2) * 2 - 1) * transform.right * (Random.Range(5, maxStrafeDistance)));
                    }
                }

                agent.SetDestination(targetPosition);

                anim.SetFloat("Speed", -1f);

                break;
        }
    }

    /// <summary>
    /// Handles constant logic for the in-range status
    /// </summary>
    private void ThinkInRange()
    {
        // Checking if the current action is finished
        // If so, start new action
        if (actionTimer > actionLimit)
        {
            actionTimer = 0;

            ChangeAction((InRangeActions)Random.Range(0, 4));
        }

        // If the player is too close to enemy, try to back up or find cover
        if (distanceFromPlayer < closestDesiredRange)
        {
            if (!coverActionsStarted)
            {
                actionTimer = 0;

                ChangeState(EnemyStates.Action);
            }
        }

        // Constant behavior for every action
        switch (inRangeAction)
        {
            case InRangeActions.StrafeLeft:

                LockedLookAt(playerRef);

                break;
            case InRangeActions.StrafeRight:

                LockedLookAt(playerRef);

                break;

            case InRangeActions.Charge:

                LockedLookAt(playerRef);

                if (agent && target != null)
                {
                    agent.SetDestination(target.position);
                }

                break;
            case InRangeActions.Stand:

                LockedLookAt(playerRef);

                break;
            case InRangeActions.Back:

                LockedLookAt(playerRef);

                break;
        }

        // Update the timer to increment between actions
        actionTimer += Time.deltaTime;
    }

    /// <summary>
    /// Sets firing on and off based on if player is in raycast
    /// </summary>
    /// <param name="playerSpotted">Is the player spotted?</param>
    protected override void TogglePlayerSightline(bool playerSpotted)
    {
        base.TogglePlayerSightline(playerSpotted);

        // If moving and we don't want to shoot, turn firing off!
        if (state == EnemyStates.OutOfRange && !shootWhileMoving)
        {
            weapon.ToggleFiring(false);
            return;
        }

        if (!mutable)
        {
            return;
        }

        weapon.ToggleFiring(playerSpotted);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            if (state == EnemyStates.Action)
            {
                currentRoom.ReturnCover(target);
            }
        }

        base.OnTriggerExit(other);
    }

    /// <summary>
    /// If player is too close to enemy, try to find a way out
    /// </summary>
    /// <returns></returns>
    public Vector3 GetFleePosition()
    {
        Vector3 testPos = transform.position - ((transform.forward + transform.right) * (Random.Range(5, maxStrafeDistance)));

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(testPos, path))
        {
            return Vector3.zero;
        }

        return testPos;
    }

    #endregion
}
