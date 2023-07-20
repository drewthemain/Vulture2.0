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
    private bool reloading;

    // References
    [Tooltip("Point used for spawning the muzzle flash")]
    [SerializeField] private Transform attackPoint;
    [Tooltip("Layers that the gun is able to shoot")]
    [SerializeField] private LayerMask hitLayers;
    [Tooltip("Visual effects for various parts of the gun")]
    [SerializeField] private GameObject muzzleFlash, bulletHole;
    private RaycastHit hit;
    private InputManager input;
    private UIManager ui;

    [Tooltip("Textbox being used for displaying ammo (temporary placeholder until we have a UI manager)")]
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        bulletsLeft = magSize;
        readyToShoot = true;
        input = InputManager.instance;
        ui = UIManager.instance;
    }

    private void Update()
    {
        UpdateInput();
        ui.UpdateAmmoText(bulletsLeft, magSize);
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

                if (hit.collider.GetComponent<EnemyHealth>())
                {
                    multiplier = hit.collider.GetComponent<EnemyHealth>().HitLocation(hit.point);
                }

                hit.collider.GetComponent<Health>().TakeDamage(damage, multiplier);
            }

            if (hit.collider.GetComponent<Prop>())
            {
                hit.collider.GetComponent<Prop>().Pushed(direction);
            }
        }

        // Add camera shake call here

        // VFX Spawning
        Instantiate(bulletHole, hit.point, Quaternion.LookRotation(-hit.normal, transform.up));
        Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity, transform);

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
        Invoke("EndReload", reloadTime);
    }

    /// <summary>
    /// Called at the end of the reload timer, refills the mag and sets the reloading bool to false
    /// </summary>
    private void EndReload()
    {
        bulletsLeft = magSize;
        reloading = false;
    }
}
