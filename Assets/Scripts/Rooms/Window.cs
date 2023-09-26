using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("The material this window is while broken")]
    [SerializeField] private Material _normalMaterial;

    [Tooltip("The material this window is while broken")]
    [SerializeField] private Material _brokenMaterial;

    [Tooltip("The target for the enemy pull when destroyed")]
    public GameObject _pullTarget;

    [Tooltip("The amount of time the room will remain depressurized")]
    [SerializeField] private float _depressurizeTime = 15f;

    private Room _parentRoom;
    private bool _broken = false;
    private CanisterHealth _canister;
    private GameObject _glass;

    // The timer for the depressurize countdown
    private float _depressureTimer = 0;

    #endregion

    #region Methods

    private void Awake()
    {
        if (GetComponentInParent<Room>())
        {
            _parentRoom = GetComponentInParent<Room>();
        }

        _canister = GetComponentInChildren<CanisterHealth>();
        _glass = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (_broken)
        {
            // Timer for the initial depressurization suck
            _depressureTimer += Time.deltaTime;

            if (_depressureTimer > _depressurizeTime)
            {
                _broken = false;
                _depressureTimer = 0;

                Pressurize();
            }
        }
    }

    /// <summary>
    /// Tells the room to depressurize
    /// </summary>
    public void Depressurize()
    {
        _broken = true;

        _glass.GetComponent<Renderer>().enabled = false;

        if (_parentRoom)
        {
            _parentRoom.Depressurize(this);
        }
    }

    /// <summary>
    /// Sets the glass to it's in-between state
    /// </summary>
    public void Pressurize()
    {
        _glass.GetComponent<Renderer>().enabled = true;
        _glass.GetComponent<Renderer>().material = _brokenMaterial;

        GameManager.instance.ToggleGravity(false, this);
    }

    public void FixWindow()
    {
        if (!_canister.gameObject.activeSelf)
        {
            _glass.GetComponent<Renderer>().material = _normalMaterial;
            _canister.gameObject.SetActive(true);
        }
    }

    #endregion
}
