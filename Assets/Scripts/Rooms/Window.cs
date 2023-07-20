using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("The material this window is while broken")]
    [SerializeField] private Material _brokenMaterial;

    [Tooltip("The material this becomes after locking itself")]
    [SerializeField] private Material _lockedMaterial;

    private Room _parentRoom;
    private bool _broken = false;
    private CanisterHealth _canister;
    private GameObject _glass;

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

    /// <summary>
    /// Tells the room to depressurize
    /// </summary>
    public void Depressurize()
    {
        _broken = true;

        _glass.GetComponent<Renderer>().enabled = false;

        if (_parentRoom)
        {
            _parentRoom.Depressurize();
        }
    }

    /// <summary>
    /// Sets the glass to it's in-between state
    /// </summary>
    public void Pressurize()
    {
        _glass.GetComponent<Renderer>().enabled = true;
        _glass.GetComponent<Renderer>().material = _brokenMaterial;
    }

    /// <summary>
    /// Sets the glass material to the correct locked visual
    /// </summary>
    public bool Lock()
    {
        if (_broken)
        {
            _broken = false;

            GameManager.instance.ToggleGravity(false);

            if (_lockedMaterial)
            {
                transform.GetChild(0).GetComponent<Renderer>().material = _lockedMaterial;
            }

            return true;
        }

        return false;
    }

    #endregion
}
