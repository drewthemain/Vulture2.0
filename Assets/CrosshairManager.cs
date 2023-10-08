using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager instance;

    [Header("Crosshair References")]
    [Tooltip("The reference to the continue button")]
    [SerializeField] private Canvas crosshairCanvas;

    [Tooltip("The reference to the hipfire crosshair parent")]
    [SerializeField] private GameObject crosshair;

    [Tooltip("The reference to the ADS crosshair parent")]
    [SerializeField] private GameObject ADScrosshair;

    [Tooltip("The reference to the crosshair canvas position transform")]
    [SerializeField] private Transform crosshairCanvasPosition;

    // General References
    // Reference to the player gameobject
    private PlayerController controller;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        if (controller == null)
        {
            Debug.LogWarning("Missing a player in the scene!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Switch the crosshair to display on the gun rather than on the camera
    /// </summary>
    public void EnableADSWorldspaceCrosshair()
    {
        crosshairCanvas.renderMode = RenderMode.WorldSpace;
        ADScrosshair.SetActive(true);
        crosshair.SetActive(false);
        Vector3 pos = crosshairCanvasPosition.localPosition;
        crosshairCanvas.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, pos.y, pos.z);
    }

    /// <summary>
    /// Switch the crosshair to display on the screenspace rather than on the gun
    /// </summary>
    public void EnableHipfireScreenCamCrosshair()
    {
        crosshairCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        ADScrosshair.SetActive(false);
        crosshair.SetActive(true);
    }
}
