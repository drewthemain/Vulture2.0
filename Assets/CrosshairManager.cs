using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [Tooltip("The updated width/height of the crosshair while walking")]
    [SerializeField] private float walkingCrosshairScale;
    [Tooltip("The updated width/height of the crosshair while moving")]
    [SerializeField] private float movingCrosshairScale;
    [Tooltip("How long (seconds) the crosshair will take to change scale")]
    [SerializeField] private float crosshairScalingDuration;
    [Tooltip("Hitmarker game object")]
    [SerializeField] private GameObject hitMarker;
    [Tooltip("Reference to the player gun script")]
    [SerializeField] private PlayerGun gun;

    // General References
    // Reference to the player gameobject
    private PlayerController controller;
    // Reference for the camera transform
    private Camera cam;
    // Storage for the color change raycast to use
    private RaycastHit hit;
    // Storage for the last color used
    private Color originalColor;
    // Storage for the change raycast color
    private Color rayHitColor = Color.red;
    // Storage for the hitmarker default color
    private Color originalHitmarkerColor;
    // Storage for the headshot hitmarker color
    private Color headshotHitmarkerColor = Color.red;
    // The original width/height of the crosshair
    private float originalCrosshairHeightWidth;
    // The original scale of the crosshair rect transform
    private Vector3 originalCrosshairScale;
    // The current state of the crosshair spread
    public CrosshairState state;
    // The coroutine making crosshair size changes
    private Coroutine scaleCoroutine;
    // The coroutine making hitmarkers
    private Coroutine hitmarkerCoroutine;
    // Boolean checking whether or not a shot was a headshot
    [HideInInspector] public bool headshot;

    public enum CrosshairState
    {
        standing,
        walking,
        moving
    }

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
        originalHitmarkerColor = hitMarker.GetComponentInChildren<Image>().color;
        originalCrosshairHeightWidth = crosshair.GetComponent<LayoutElement>().preferredHeight;
        originalCrosshairScale = crosshair.GetComponent<RectTransform>().localScale;
        state = CrosshairState.walking;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCrosshairColor();
        UpdateCrosshairSpread();
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

    /// <summary>
    /// Update the color of the crosshair depending on if the raycast hits an enemy
    /// </summary>
    private void UpdateCrosshairColor()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxCrosshairChangeDist, hitLayers))
        {
            if (hit.collider.gameObject.layer == 7)
            {
                ChangeChildrenColors(crosshair, rayHitColor);
                ChangeChildrenColors(ADScrosshair, rayHitColor);
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

    /// <summary>
    /// Change the colors of all children with an image component under the parent provided
    /// </summary>
    /// <param name="parent">Object to get children from</param>
    /// <param name="color">Color to change children to</param>
    private void ChangeChildrenColors(GameObject parent, Color color)
    {
        Image[] children;
        children = parent.GetComponentsInChildren<Image>();
        foreach(Image img in children)
        {
            img.color = color;
        }
    }

    /// <summary>
    /// Change the crosshair spread based on the current state:
    /// standing - standing still
    /// walking - walking speed
    /// moving - anything more than walking (sprinting, wallrun, etc)
    /// </summary>
    private void UpdateCrosshairSpread()
    {
        if(crosshair.activeInHierarchy)
        {
            if (gun.reloading)
            {
                crosshair.GetComponent<RectTransform>().localScale = Vector3.zero;
            }
            else if (!gun.reloading)
            {
                crosshair.GetComponent<RectTransform>().localScale = originalCrosshairScale;
            }
            if (controller.state == PlayerController.MovementState.walking)
            {
                // standing still
                if (state != CrosshairState.standing && controller.IsPlayerStandingStill())
                {
                    state = CrosshairState.standing;
                    if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                    scaleCoroutine = StartCoroutine(LerpCrosshairScale(crosshair.GetComponent<LayoutElement>().preferredHeight, originalCrosshairHeightWidth, crosshairScalingDuration));
                }
                // walking
                else if (state != CrosshairState.walking && !controller.IsPlayerStandingStill())
                {
                    state = CrosshairState.walking;
                    if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                    scaleCoroutine = StartCoroutine(LerpCrosshairScale(crosshair.GetComponent<LayoutElement>().preferredHeight, walkingCrosshairScale, crosshairScalingDuration));
                }

            }
            else if (controller.state != PlayerController.MovementState.walking)
            {
                // standing still
                if (state != CrosshairState.standing && controller.IsPlayerStandingStill())
                {
                    state = CrosshairState.standing;
                    if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                    scaleCoroutine = StartCoroutine(LerpCrosshairScale(crosshair.GetComponent<LayoutElement>().preferredHeight, originalCrosshairHeightWidth, crosshairScalingDuration));
                }
                // moving faster than walking
                else if (state != CrosshairState.moving && !controller.IsPlayerStandingStill())
                {
                    state = CrosshairState.moving;
                    if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
                    scaleCoroutine = StartCoroutine(LerpCrosshairScale(crosshair.GetComponent<LayoutElement>().preferredHeight, movingCrosshairScale, crosshairScalingDuration));
                }
            }
        }
    }
    
    /// <summary>
    /// Coroutine to lerp the crosshair into a new position
    /// </summary>
    /// <param name="start">Starting crosshair position</param>
    /// <param name="target">Target crosshair position</param>
    /// <param name="duration">Duration of the move</param>
    /// <returns></returns>
    IEnumerator LerpCrosshairScale(float start, float target, float duration)
    {
        while(crosshair.activeInHierarchy)
        {
            float time = 0;
            while (time < duration)
            {
                crosshair.GetComponent<LayoutElement>().preferredHeight = Mathf.Lerp(crosshair.GetComponent<LayoutElement>().preferredHeight, target, time / duration);
                crosshair.GetComponent<LayoutElement>().preferredWidth = Mathf.Lerp(crosshair.GetComponent<LayoutElement>().preferredWidth, target, time / duration);

                time += Time.deltaTime;
                yield return null;
            }

            crosshair.GetComponent<LayoutElement>().preferredHeight = target;
            crosshair.GetComponent<LayoutElement>().preferredWidth = target;
        }
    }

    /// <summary>
    /// Turn on the hitmarker
    /// </summary>
    public void EnableHitMarker(bool headshot)
    {
        hitMarker.SetActive(true);
        if(hitmarkerCoroutine != null) StopCoroutine(hitmarkerCoroutine);
        if(headshot)
        {
            hitmarkerCoroutine = StartCoroutine(FlashHitMarker(.1f, headshotHitmarkerColor));
        }
        else
        {
            hitmarkerCoroutine = StartCoroutine(FlashHitMarker(.1f, originalHitmarkerColor));
        }
    }

    /// <summary>
    /// Turn off the hitmarker
    /// </summary>
    private void DisableHitMarker()
    {
        hitMarker.SetActive(false);
    }

    /// <summary>
    /// Coroutine for how long the hitmarker will stay active
    /// </summary>
    /// <param name="duration">How long the hitmarker is active</param>
    IEnumerator FlashHitMarker (float duration, Color color)
    {
        ChangeChildrenColors(hitMarker, color);
        yield return new WaitForSeconds(duration);
        DisableHitMarker();
    }

    /// <summary>
    /// Changes crosshair opacity and updates it in PlayerPrefs
    /// </summary>
    /// <param name="opacityPercentage"></param>
    public void ChangeCrosshairOpacity(float opacityPercentage)
    {
        PlayerPrefs.SetFloat("crosshairOpacity", opacityPercentage);
        originalColor.a = opacityPercentage;
        rayHitColor.a = opacityPercentage;
    }
}
