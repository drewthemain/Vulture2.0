using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    #region Variables

    [Header("Options")]

    [Tooltip("The material this window is while broken")]
    [SerializeField] private Material normalMaterial;

    [Tooltip("The material this window is while broken")]
    [SerializeField] private Material brokenMaterial;

    [Tooltip("The target for the enemy pull when destroyed")]
    public GameObject pullTarget;

    [Tooltip("The amount of time the room will remain depressurized")]
    [SerializeField] private float depressurizeTime = 15f;

    // References
    private Room parentRoom;
    private bool broken = false;
    private CanisterHealth canister;
    private GameObject glass;

    // The timer for the depressurize countdown
    private float depressureTimer = 0;

    #endregion

    #region Methods

    private void Awake()
    {
        if (GetComponentInParent<Room>())
        {
            parentRoom = GetComponentInParent<Room>();
        }

        canister = GetComponentInChildren<CanisterHealth>();
        glass = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (broken)
        {
            // Timer for the initial depressurization suck
            depressureTimer += Time.deltaTime;

            if (depressureTimer > depressurizeTime)
            {
                broken = false;
                depressureTimer = 0;

                Pressurize(true);
            }
        }
    }

    /// <summary>
    /// Tells the room to depressurize
    /// </summary>
    public void Depressurize()
    {
        broken = true;

        glass.GetComponent<Renderer>().enabled = false;

        if (parentRoom)
        {
            parentRoom.Depressurize(this);
        }
    }

    // Force closes the window and repressurizes
    public void ForceClose()
    {
        broken = false;
        depressureTimer = 0;

        Pressurize(false);
    }

    /// <summary>
    /// Sets the glass to it's in-between state
    /// </summary>
    public void Pressurize(bool toggleGrav)
    {
        glass.GetComponent<Renderer>().enabled = true;
        glass.GetComponent<Renderer>().material = brokenMaterial;

        if (toggleGrav)
        {
            GameManager.instance.ToggleGravity(false, this);
        }
    }

    /// <summary>
    /// Resets the window by resetting gameobject states
    /// </summary>
    public void FixWindow()
    {
        if (!canister.gameObject.activeSelf)
        {
            glass.GetComponent<Renderer>().material = normalMaterial;
            canister.gameObject.SetActive(true);
        }
    }

    #endregion
}
