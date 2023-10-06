using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("Should the door begin open or closed?")]
    [SerializeField] private bool beginOpen = true;

    [Tooltip("Should the door change back to original position after a set amount of time?")]
    [SerializeField] private bool isTimedDoor = false;

    [Tooltip("The time it takes for the door to revert")]
    [SerializeField] private float doorTimer = 5f;

    [Tooltip("The time it takes the door to be toggled again")]
    [SerializeField] private float toggleBuffer = 3f;

    [Header("References")]

    [Tooltip("List of door toggles")]
    [SerializeField] private List<DoorToggle> toggles;

    [Tooltip("Reference to the door animator")]
    [SerializeField] private Animator doorAnim;

    // Is the door currently open?
    public bool isOpen = false;

    // Is the reverter currently in progress?
    private bool revertTime = false;

    // Is the reverter currently in progress?
    private bool bufferTime = false;

    private float bufferTimer = 0;
    private float revertTimer = 0;

    private bool isToggleable = true;

    #endregion

    #region Methods

    private void Awake()
    {
        if (doorAnim)
        {
            doorAnim.SetBool("Open", beginOpen);
        }

        isOpen = beginOpen;
        RefreshToggles();
    }

    private void Update()
    {
        // Buffer gate
        if (bufferTime)
        {
            if (bufferTimer >= toggleBuffer)
            {
                isToggleable = true;
                bufferTime = false;

                bufferTimer = 0;
            }

            bufferTimer += Time.deltaTime;
        }

        // Revert gate
        if (revertTime)
        {
            if (revertTimer >= doorTimer)
            {
                revertTime = false;
                revertTimer = 0;

                ToggleDoor(true);
            }

            revertTimer += Time.deltaTime;
        }
    }

    public void ToggleDoor(bool revert)
    {
        if (isToggleable)
        {
            isOpen = !isOpen;

            if (!revert)
            {
                isToggleable = false;
                bufferTime = true;
                bufferTimer = 0;

                if (isTimedDoor)
                {
                    revertTime = true;
                    revertTimer = 0;
                }
            }

            RefreshToggles();
            doorAnim.SetBool("Open", isOpen);
        }
    }

    private void RefreshToggles()
    {
        foreach (DoorToggle toggle in toggles)
        {
            toggle.Refresh();
        }
    }

    #endregion
}
