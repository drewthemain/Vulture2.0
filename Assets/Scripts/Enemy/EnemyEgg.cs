using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEgg : Enemy
{
    public override void ChangeState(EnemyStates newState)
    {
        base.ChangeState(newState);

        state = newState;

        switch (newState)
        {
            case EnemyStates.OutOfRange:
            case EnemyStates.NoGrav:
            case EnemyStates.Stop:
                break;
            case EnemyStates.Action:
                // TODO: EXPLODE
                TriggerExplodeAnim();
                break;
            case EnemyStates.InRange:
                ChangeState(EnemyStates.Action);
                break;
        }
    }

    public void TriggerExplodeAnim()
    {
        anim.SetTrigger("Explode");
    }

    public void Explode()
    {
        Debug.Log("EXPLODE");
    }
}
