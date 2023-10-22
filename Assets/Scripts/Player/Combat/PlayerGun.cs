using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerGun : MonoBehaviour
{
    [Header("Gun Attributes")]
    [Tooltip("The amount of damage each bullet will deal")]
    [SerializeField] private int damage;
    [Tooltip("The time between each bullet")]
    [SerializeField] private float fireRate;
    [Tooltip("Amount of random innacuracy the bullets will have (eventually hoping to change to a pattern)")]
    [SerializeField] private float spread;
    [Tooltip("Distance that the gun can shoot, technically the length of the raycast")]
    [SerializeField] private float range;
    [Tooltip("Time taken to reload")]
    [SerializeField] private float reloadTime;
    [Tooltip("Used if we want to make the gun burst fire - needs more bullets per tap as well")]
    [SerializeField] private float timeBetweenShots;
    [Tooltip("Number of bullets in one full magazine")]
    [SerializeField] private int magSize;
    [Tooltip("Number of bullets that are fired per click, used for shotguns / burst fire customizations")]
    [SerializeField] private int bulletsPerTap;
    [Tooltip("Allows the fire button to be held down")]
    [SerializeField] private bool allowAutomatic;
    // Bullets remaining in the current magazine
    private int bulletsLeft;
    // Bullets shot from the current magazine
    private int bulletsShot;

    // Bools for state checking
    // True if the player is currently shooting
    private bool shooting;
    // True if the player is currently able to shoot
    private bool readyToShoot;
    // True if the player is currently reloading
    public bool reloading;

    [Header("Recoil Values")]
    [Tooltip("The length of the upwards portion of the recoil (gun moves up and back down for each shot)")]
    [SerializeField] private float recoilUpDuration;
    [Tooltip("The length of the downwards portion of the recoil (gun moves up and back down for each shot)")]
    [SerializeField] private float recoilDownDuration;
    [Tooltip("The amount that the gun will be shifted vertically (on the X axis in this case")]
    [SerializeField] private float targetRecoilHeight;
    [Tooltip("The amount that the gun will be shifted vertically while ADS'ing (on the X axis in this case")]
    [SerializeField] private float targetADSRecoilHeight;

    // References
    [Tooltip("Reference to the physical body of the gun")]
    [SerializeField] private Transform gunBody;
    [Tooltip("Point used for spawning the muzzle flash")]
    [SerializeField] private Transform attackPoint;
    [Tooltip("Layers that the gun is able to shoot")]
    [SerializeField] private LayerMask hitLayers;
    [Tooltip("Layers that will spawn a bullethole")]
    [SerializeField] private LayerMask bulletholeLayers;
    [Tooltip("Visual effects for various parts of the gun")]
    [SerializeField] private GameObject muzzleFlash, bulletHole;
    // Storage for the raycast to use
    private RaycastHit hit;
    // Reference to the input manager
    private InputManager input;
    // Reference to the UI manager
    private UIManager ui;
    // Reference to the recoil shake
    private GunRecoilShake recoilShake;
        // Reference to the ADS
    private PlayerADS ads;
    // Reference for the camera transform
    private Transform cameraTransform;
    // Initial gun rotation values
    private Quaternion initialRotation;
    // Reference to the animator on the gun body
    private Animator animator;

    [Tooltip("Textbox being used for displaying ammo (temporary placeholder until we have a UI manager)")]
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        recoilShake = GetComponent<GunRecoilShake>();
        ads = GetComponent<PlayerADS>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        bulletsLeft = magSize;
        readyToShoot = true;
        input = InputManager.instance;
        ui = UIManager.instance;
        cameraTransform = Camera.main.transform;
        initialRotation = Quaternion.Euler(gunBody.transform.rotation.x, gunBody.transform.rotation.y, gunBody.transform.rotation.z);
    }

    private void Update()
    {
        UpdateInput();
        ui.UpdateAmmo(bulletsLeft, magSize);
    }

    /// <summary>
    /// Checks the player input manager and updates the state of the gun accordingly:
    /// Starts reloading if the player presses R or automatically starts if the player is holding down fire when the mag becomes empty
    /// Sets shooting bool to true if the player is holding or presses click
    /// </summary>
    private void UpdateInput()
    {
        if (allowAutomatic)
        {
            shooting = input.PlayerIsFiring();
        }
        else
        {
            shooting = input.PlayerStartedFiring();
        }
        if(input.PlayerStartedReloading() && bulletsLeft < magSize && !reloading)
        {
            Reload();
        }
        else if (input.PlayerIsFiring() && bulletsLeft == 0 && !reloading)
        {
            Reload();
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    /// <summary>
    /// Uses a raycast in the direction of the camera to shoot. 
    /// Checks the result of the raycast for a health component, and deals damage if it is found.
    /// Randomly calculates spread within the range of the spread variable.
    /// </summary>
    private void Shoot()
    {
        readyToShoot = false;

        // Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Calculate Direction with Spread
        Vector3 direction = Camera.main.transform.forward + new Vector3(x, y, 0);

        // Raycast 
        if (Physics.Raycast(Camera.main.transform.position, direction, out hit, range, hitLayers))
        {
            if (hit.collider.GetComponent<Health>())
            {
                float multiplier = 1;

                hit.collider.GetComponent<Health>().TakeDamage(damage, multiplier);
                CrosshairManager.instance.EnableHitMarker();
            }
            else if (hit.collider.GetComponent<LimbCollider>())
            {
                hit.collider.GetComponent<LimbCollider>().SignalDamage(damage);
                CrosshairManager.instance.EnableHitMarker();
            }
            else if (hit.collider.GetComponent<Prop>())
            {
                hit.collider.GetComponent<Prop>().Pushed(direction);
            }
            else if (hit.collider.GetComponent<DoorToggle>())
            {
                hit.collider.GetComponent<DoorToggle>().ToggleToggle();
            }
        }

        if (Physics.Raycast(Camera.main.transform.position, direction, out hit, range, bulletholeLayers))
        {
            SpawnBullethole(hit);
        }


        // Camera shake
        recoilShake.ScreenShake();

        // Object shake
        StartRecoilShake();

        // VFX Spawning
        Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity, transform);

        // Wwise Post SFX
        AkSoundEngine.PostEvent("Weapon_AR_SingleBurst", this.gameObject);

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", fireRate);

        if(bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }
    
    /// <summary>
    /// Resets the gun to being ready to shoot
    /// </summary>
    private void ResetShot()
    {
        readyToShoot = true;
    }

    /// <summary>
    /// Begins the reload timer and sets the reloading bool to true
    /// </summary>
    private void Reload()
    {
        reloading = true;
        ads.StopADS();
        animator.SetBool("Reload", true);

        Invoke("EndReload", reloadTime);
    }

    /// <summary>
    /// Called at the end of the reload timer, refills the mag and sets the reloading bool to false
    /// </summary>
    private void EndReload()
    {
        bulletsLeft = magSize;
        reloading = false;
        animator.SetBool("Reload", false);
    }
    

    /// <summary>
    /// The upwards portion of the recoil
    /// Recoil can be interrupted by another shot, causing it to restart the upward portion and cancel the current status
    /// </summary>
    /// <param name="target">Target position for the gun to rotate into</param>
    /// <param name="duration">The length that the recoil lasts</param>
    /// <returns></returns>
    IEnumerator RecoilLerpUp(Quaternion target, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            gunBody.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(gunBody.transform.localRotation.x, gunBody.transform.localRotation.y, gunBody.transform.localRotation.z), target, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(RecoilLerpDown(target, recoilDownDuration));
    }

    /// <summary>
    /// The downwards portion of the recoil
    /// Recoil can be interrupted by another shot, causing it to restart the upward portion and cancel the current status
    /// </summary>
    /// <param name="target">Target position for the gun to rotate into</param>
    /// <param name="duration">The length that the recoil lasts</param>
    /// <returns></returns>
    IEnumerator RecoilLerpDown(Quaternion target, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            gunBody.transform.localRotation = Quaternion.Lerp(target, initialRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Begin the recoil when another shot is fired, ending current coroutine and restarting from the bottom
    /// </summary>
    private void StartRecoilShake()
    {
        StopAllCoroutines();
        Vector3 rot = gunBody.transform.localEulerAngles;
        float height = targetRecoilHeight;
        if(ads.aiming) { height = targetADSRecoilHeight; }
        Quaternion target = Quaternion.Euler(rot.x - height, rot.y, rot.z);
        StartCoroutine(RecoilLerpUp(target, recoilUpDuration));
    }
    
    /// <summary>
    /// Spawn a bullethole facing outwards from the target it hits (layers determined by raycast layermask)
    /// </summary>
    /// <param name="hit">The hit information from performing the raycast</param>
    private void SpawnBullethole(RaycastHit hit)
    {
        Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit.collider.gameObject.transform);
    }
}