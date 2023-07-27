using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorToggle : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("The MAT for a toggled-off button")]
    [SerializeField] private Material _offButtonMAT;

    [Tooltip("The MAT for a toggled-on button")]
    [SerializeField] private Material _onButtonMAT;

    // Reference to the connected door
    Door _door;

    #endregion

    #region Methods

    private void Awake()
    {
        _door = GetComponentInParent<Door>();
    }

    public void ToggleToggle()
    {
        _door.ToggleDoor(false);
    }

    public void Refresh()
    {
        GetComponent<MeshRenderer>().material = _door._isOpen ? _onButtonMAT : _offButtonMAT;
    }

    #endregion
}
