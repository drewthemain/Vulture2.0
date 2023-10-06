using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorToggle : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("The MAT for a toggled-off button")]
    [SerializeField] private Material offButtonMAT;

    [Tooltip("The MAT for a toggled-on button")]
    [SerializeField] private Material onButtonMAT;

    // Reference to the connected door
    Door door;

    #endregion

    #region Methods

    private void Awake()
    {
        door = GetComponentInParent<Door>();
    }

    public void ToggleToggle()
    {
        door.ToggleDoor(false);
    }

    public void Refresh()
    {
        GetComponent<MeshRenderer>().material = door.isOpen ? onButtonMAT : offButtonMAT;
    }

    #endregion
}
