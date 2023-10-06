using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{

    #region Variables

    [Header("Options")]

    [Tooltip("The amount of force this prop will rise upon low gravity")]
    [SerializeField] private float riseForce = 50;

    [Tooltip("The amount of force this prop will take when shot")]
    [SerializeField] private float shotForce = 100;

    [Tooltip("The speed this prop must be going to damage enemies")]
    [SerializeField] private float damageSpeed = 5;

    // Reference to this rigidbody
    private Rigidbody body;

    // Is the object currently floating?
    private bool isLowGrav = false;

    #endregion

    #region Methods

    private void OnEnable()
    {
        GameManager.OnLowGrav += ToggleFloat;
    }

    private void OnDisable()
    {
        GameManager.OnLowGrav -= ToggleFloat;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        isLowGrav = false;
    }

    /// <summary>
    /// Handles an object turning on/off it's low grav
    /// </summary>
    public void ToggleFloat()
    {
        isLowGrav = !isLowGrav;

        body.useGravity = !isLowGrav;

        float limiter = Mathf.Max(new float[] { transform.localScale.x, transform.localScale.y, transform.localScale.z });

        if (isLowGrav)
        {
            body.AddForce(Vector3.up * riseForce * (1 / limiter));
        }
    }
    
    /// <summary>
    /// Pushes objects from the gun
    /// </summary>
    /// <param name="dir">The direction the shot took place</param>
    public void Pushed(Vector3 dir)
    {
        body.AddForce(dir * shotForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isLowGrav)
        {
            if (collision.transform.GetComponent<EnemyHealth>() && body.velocity.sqrMagnitude >= damageSpeed)
            {
                collision.transform.GetComponent<EnemyHealth>().TakeDamage(Mathf.Ceil(body.velocity.sqrMagnitude - damageSpeed));
            }
        }
    }

    #endregion
}
