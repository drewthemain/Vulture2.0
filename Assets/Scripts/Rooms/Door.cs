using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("Should the door begin open or closed?")]
    [SerializeField] private bool _beginOpen = true;

    [Tooltip("Should the door change back to original position after a set amount of time?")]
    [SerializeField] private bool _isTimedDoor = false;

    [Tooltip("The time it takes for the door to revert")]
    [SerializeField] private float _doorTimer = 5f;

    [Tooltip("The time it takes the door to be toggled again")]
    [SerializeField] private float _toggleBuffer = 3f;

    [Header("References")]

    [Tooltip("List of door toggles")]
    [SerializeField] private List<DoorToggle> _toggles;

    [Tooltip("Reference to the door animator")]
    [SerializeField] private Animator _doorAnim;

    // Is the door currently open?
    public bool _isOpen = false;

    // Is the reverter currently in progress?
    private bool _revertTime = false;

    // Is the reverter currently in progress?
    private bool _bufferTime = false;

    private float _bufferTimer = 0;
    private float _revertTimer = 0;

    private bool _isToggleable = true;

    #endregion

    #region Methods

    private void Awake()
    {
        if (_doorAnim)
        {
            _doorAnim.SetBool("Open", _beginOpen);
        }

        _isOpen = _beginOpen;
        RefreshToggles();
    }

    private void Update()
    {
        // Buffer gate
        if (_bufferTime)
        {
            if (_bufferTimer >= _toggleBuffer)
            {
                _isToggleable = true;
                _bufferTime = false;

                _bufferTimer = 0;
            }

            _bufferTimer += Time.deltaTime;
        }

        // Revert gate
        if (_revertTime)
        {
            if (_revertTimer >= _doorTimer)
            {
                _revertTime = false;
                _revertTimer = 0;

                ToggleDoor(true);
            }

            _revertTimer += Time.deltaTime;
        }
    }

    public void ToggleDoor(bool revert)
    {
        if (_isToggleable)
        {
            _isOpen = !_isOpen;

            if (!revert)
            {
                _isToggleable = false;
                _bufferTime = true;
                _bufferTimer = 0;

                if (_isTimedDoor)
                {
                    _revertTime = true;
                    _revertTimer = 0;
                }
            }

            RefreshToggles();
            _doorAnim.SetBool("Open", _isOpen);
        }
    }

    private void RefreshToggles()
    {
        foreach (DoorToggle toggle in _toggles)
        {
            toggle.Refresh();
        }
    }

    #endregion
}
