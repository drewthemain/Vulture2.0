using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{

    #region Variables

    [Header("Options")]

    [Tooltip("The amount of force this prop will rise upon low gravity")]
    [SerializeField] private float _riseForce = 50;

    [Tooltip("The amount of force this prop will take when shot")]
    [SerializeField] private float _shotForce = 100;

    [Tooltip("The speed this prop must be going to damage enemies")]
    [SerializeField] private float _damageSpeed = 5;

    // Reference to this rigidbody
    private Rigidbody _body;

    // Is the object currently floating?
    private bool _isLowGrav = false;

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
        _body = GetComponent<Rigidbody>();
        _isLowGrav = false;
    }

    /// <summary>
    /// Handles an object turning on/off it's low grav
    /// </summary>
    public void ToggleFloat()
    {
        _isLowGrav = !_isLowGrav;

        _body.useGravity = !_isLowGrav;

        float limiter = Mathf.Max(new float[] { transform.localScale.x, transform.localScale.y, transform.localScale.z });

        if (_isLowGrav)
        {
            _body.AddForce(Vector3.up * _riseForce * (1 / limiter));
        }
    }
    
    /// <summary>
    /// Pushes objects from the gun
    /// </summary>
    /// <param name="dir">The direction the shot took place</param>
    public void Pushed(Vector3 dir)
    {
        _body.AddForce(dir * _shotForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<EnemyHealth>() && _body.velocity.sqrMagnitude >= _damageSpeed)
        {
            collision.transform.GetComponent<EnemyHealth>().TakeDamage(Mathf.Ceil(_body.velocity.sqrMagnitude - _damageSpeed));
        }
    }

    #endregion
}
