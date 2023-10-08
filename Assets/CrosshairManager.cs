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
    [Tooltip("Max distance that the crosshair switches when an enemy is recognized (raycast distance)")]
    [SerializeField] private float maxCrosshairChangeDist;
    [Tooltip("Enemy layermask for crosshair color change")]
    [SerializeField] private LayerMask hitLayers;

    // General References
    // Reference to the player gameobject
    private PlayerController controller;
    // Reference for the camera transform
    private Camera cam;
    // Storage for the color change raycast to use
    private RaycastHit hit;
    // Storage for the last color used
    private Color originalColor;

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
        cam = Camera.main;
        originalColor = crosshair.GetComponentInChildren<Image>().color;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCrosshairColor();
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

    private void UpdateCrosshairColor()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxCrosshairChangeDist, hitLayers))
        {
            if (hit.collider.gameObject.layer == 7)
            {
                ChangeChildrenColors(crosshair, Color.red);
                ChangeChildrenColors(ADScrosshair, Color.red);
            }
            else
            {
                ChangeChildrenColors(crosshair, originalColor);
                ChangeChildrenColors(ADScrosshair, originalColor);
            }
        }
        else
        {
            ChangeChildrenColors(crosshair, originalColor);
            ChangeChildrenColors(ADScrosshair, originalColor);
        }
    }

    private void ChangeChildrenColors(GameObject parent, Color color)
    {
        Image[] children;
        children = parent.GetComponentsInChildren<Image>();
        foreach(Image img in children)
        {
            img.color = color;
        }
    }
}
