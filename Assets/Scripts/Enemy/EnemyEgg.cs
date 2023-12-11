using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEgg : Enemy
{

    #region Variables

    [Tooltip("The gas cloud to appear on destruction")]
    [SerializeField] private GasCloud cloud;

    #endregion

    #region Methods
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
        cloud.transform.parent = null;
        cloud.gameObject.SetActive(true);
        Debug.Log("EXPLODE");
        Destroy(this.gameObject);
    }

    #endregion
}
