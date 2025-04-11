using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;
    private const float REFERENCE_BULLET_SPEED = 20;
    //This is the default speed from which our mass formula is derived.

    [SerializeField] private Weapon_Data defaultWeaponData;
    [SerializeField] private Weapon currentWeapon;
    private bool weaponReady;
    private bool isShooting;

    [Header("Bullet details")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    [SerializeField] private Transform weaponHolder;

    [Header("Inventory")]
    [SerializeField] private int maxSlots = 2;
    [SerializeField] private List<Weapon> weaponSlots;

    [SerializeField] private GameObject weaponPickPrefab;

    [System.Obsolete]
    private void Start()
    {
        player = GetComponent<Player>();
        AssignInputEvents();

        Invoke("EquipStartingWeapon", .1f);

        if (weaponSlots.Count > 0)
        {
            currentWeapon = weaponSlots[0]; 
        }

        if (currentWeapon != null)
        {
            currentWeapon.bulletsInMagazine = currentWeapon.magazineCapacity;
        }
        else
        {
            Debug.LogError("PlayerWeaponController: No weapon found in weapon slots!");
        }
    }

    [System.Obsolete]
    private void Update()
    {
        if (isShooting)
            Shoot();
    }

    #region Slots management - Pickup\Equip\Drop Weapon

    private void EquipStartingWeapon()
    {
        weaponSlots[0] = new Weapon(defaultWeaponData);
        EquipWeapon(0);
    }
    private void EquipWeapon(int i)
    {
        if (i >= weaponSlots.Count)
            return;

        SetWeaponReady(false);

        if (i < 0 || i >= weaponSlots.Count)
        {
            Debug.LogError($"EquipWeapon: Invalid slot index {i}.");
            return;
        }

        currentWeapon = weaponSlots[i];
        player.weaponVisuals.PlayWeaponEquipAnimation();

        CameraManager.instance.ChangeCameraDistance(currentWeapon.cameraDistance);
    }

    public void PickupWeapon(Weapon newWeapon)
    {


        if (WeaponInSlots(newWeapon.weaponType) != null)
        {
            WeaponInSlots(newWeapon.weaponType).totalReserveAmmo += newWeapon.bulletsInMagazine;
            return;
        }



        if (weaponSlots.Count >= maxSlots && newWeapon.weaponType != currentWeapon.weaponType)
        {
            int weaponIndex = weaponSlots.IndexOf(currentWeapon);

            player.weaponVisuals.SwitchOffWeaponModels();
            weaponSlots[weaponIndex] = newWeapon;

            CreateWeaponOnTheGround();
            EquipWeapon(weaponIndex);
            return;
        }


        weaponSlots.Add(newWeapon);
        player.weaponVisuals.SwitchOnBackupWeaponModel();
    }

    private void DropWeapon()
    {
        if (HasOnlyOneWeapon())
            return;

        CreateWeaponOnTheGround();

        weaponSlots.Remove(currentWeapon);
        EquipWeapon(0);
    }

    private void CreateWeaponOnTheGround()
    {
        GameObject droppedWeapon = ObjectPool.instance.GetObject(weaponPickPrefab);
        droppedWeapon.GetComponent<Pickup_Weapon>()?.SetupPickWeapon(currentWeapon, transform);
    }

    public void SetWeaponReady(bool ready) => weaponReady = ready;
    public bool WeaponReady() => weaponReady;

    #endregion

    private IEnumerator BurstFire()
    {
        SetWeaponReady(false);

        for (int i = 1; i <= currentWeapon.bulletsPerShot; i++)
        {
            FireSingleBullet();

            yield return new WaitForSeconds(currentWeapon.burstFireDelay);

            if (i >= currentWeapon.bulletsPerShot)
                SetWeaponReady(true);
        }
    }

    [System.Obsolete]
    private void Shoot()
    {
        if (WeaponReady() == false)
            return;

        if (currentWeapon.CanShoot() == false)
            return;

        player.weaponVisuals.PlayFireAnimation();

        if (currentWeapon.shootType == ShootType.Single)
            isShooting = false;

        if(currentWeapon.BurstAtivated() == true)
        {
            StartCoroutine(BurstFire());
            return;
        }

        FireSingleBullet();
    }

    private void FireSingleBullet()
    {
        currentWeapon.bulletsInMagazine--;

        GameObject newBullet = ObjectPool.instance.GetObject(bulletPrefab);

        newBullet.transform.position = GunPoint().position;
        newBullet.transform.rotation = Quaternion.LookRotation(GunPoint().forward);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.BulletSetup(currentWeapon.gunDistance);

        Vector3 bulletsDirection = currentWeapon.ApplySpread(BulletDirection());

        rbNewBullet.mass = REFERENCE_BULLET_SPEED / bulletSpeed;
        rbNewBullet.linearVelocity = bulletsDirection * bulletSpeed;
    }

    private void Reload()
    {
        SetWeaponReady(false);
        player.weaponVisuals.PlayReloadAnimation();
    }

    public Vector3 BulletDirection()
    {
        if (player == null || player.aim == null)
        {
            Debug.LogError("BulletDirection: player or aim is NULL!");
            return Vector3.forward; // Default direction
        }

        Transform aim = player.aim.Aim();
        Vector3 direction = (aim.position - GunPoint().position).normalized;

        if (!player.aim.CanAimPrecisly() && player.aim.Target() == null)
            direction.y = 0;

        return direction;
    }


    public bool HasOnlyOneWeapon() => weaponSlots.Count <= 1;
    public Weapon WeaponInSlots(WeaponType weaponType)
    {
        foreach(Weapon weapon in weaponSlots)
        {
            if(weapon.weaponType == weaponType)
                return weapon;
        }

        return null;
    }

    public Weapon CurrentWeapon() => currentWeapon;
    public Transform GunPoint() => player.weaponVisuals.CurrentWeaponModel().gunPoint;

    #region Input Events
    [System.Obsolete]
    private void AssignInputEvents()
    {
        PlayerControls controls = player.controls;

        controls.Character.Fire.performed += context => isShooting = true;
        controls.Character.Fire.canceled += context => isShooting = false;

        controls.Character.EquipSlot1.performed += context => EquipWeapon(0);
        controls.Character.EquipSlot2.performed += context => EquipWeapon(1);
        controls.Character.EquipSlot3.performed += context => EquipWeapon(2);
        controls.Character.EquipSlot4.performed += context => EquipWeapon(3);
        controls.Character.EquipSlot5.performed += context => EquipWeapon(4);


        controls.Character.DropCurrentWeapon.performed += context => DropWeapon();

        controls.Character.Reload.performed += context =>
        {
            if (currentWeapon != null && currentWeapon.CanReload() && WeaponReady())
            {
                Reload();
            }
        };

        controls.Character.ToogleWeaponMode.performed += context => currentWeapon.ToggleBurst();

    }


    #endregion
}
