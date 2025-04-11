using UnityEngine;

public enum WeaponType
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Rifle
}

public enum ShootType
{
    Single,
    Auto
}

[System.Serializable]
public class Weapon
{
    public WeaponType weaponType;

    #region Regular mode variables
    public ShootType shootType;
    public int bulletsPerShot { get; private set; }

    private float defaultFireRate;
    public float fireRate = 1; // bullets per second
    private float lastShootTime;
    #endregion
    #region Burst mode variables
    private bool burstAvalible;
    public bool burstActive;

    private int burstModeBulletsPerShot;
    private float burstFireRate;
    public float burstFireDelay { get; private set; }
    #endregion

    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    #region Weapon generic info variables

    public float reloadSpeed { get; private set; } // how fast character reloads weapon
    public float equipmentSpeed { get; private set; } // how fast character equips weapon
    public float gunDistance { get; private set; }
    public float cameraDistance { get; private set; }
    #endregion

    #region Weapon spread variables
    [Header("Spread")]
    private float baseSpread = 1;
    private float maximumSpread = 3;
    private float currentSpread = 2;

    private float spreadIncreaseRate = .15f;

    private float lastSpreadUpdatetime;
    private float spreadCooldown = 1;
    #endregion

    public Weapon_Data weaponData {  get; private set; } // serves as default weapon data

    public Weapon(Weapon_Data weaponData)
    {
        bulletsInMagazine = weaponData.bulletsInMagazine;
        magazineCapacity = weaponData.magazineCapacity;
        totalReserveAmmo = weaponData.totalReserveAmmo;

        fireRate = weaponData.fireRate;
        weaponType = weaponData.weaponType;

        bulletsPerShot = weaponData.bulletsPerShot;
        shootType = weaponData.shootType;

        burstAvalible = weaponData.burstAvalible;
        burstActive = weaponData.burstActive;
        burstModeBulletsPerShot = weaponData.burstBulletsPerShot;
        burstFireRate = weaponData.burstFireRate;
        burstFireDelay = weaponData.burstFireDelay;

        baseSpread = weaponData.baseSpread;
        maximumSpread = weaponData.maxSpread;
        spreadIncreaseRate = weaponData.spreadIncreaseRate;
        
        reloadSpeed = weaponData.reloadSpeed;
        equipmentSpeed = weaponData.equipmentSpeed;
        gunDistance = weaponData.gunDistance;
        cameraDistance = weaponData.cameraDistance;

        defaultFireRate = fireRate;
        this.weaponData = weaponData;
    }

    #region Spread methods
    public Vector3 ApplySpread(Vector3 originalDirection)
    {
        UpdateSpread();

        float randomizedValue = Random.Range(-currentSpread, currentSpread);

        Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue, randomizedValue);

        return spreadRotation * originalDirection;
    }

    private void UpdateSpread()
    {

        if (Time.time > lastSpreadUpdatetime + spreadCooldown)
            currentSpread = baseSpread;
        else
            IncreaseSpread();


        lastSpreadUpdatetime = Time.time;
    }

    private void IncreaseSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread + spreadIncreaseRate, baseSpread, maximumSpread);
    }

    #endregion

    #region burst methods

    public bool BurstAtivated()
    {
        if (weaponType == WeaponType.Shotgun)
        {
            burstFireDelay = 0;
            return true;
        }

        return burstActive;
    }

    public void ToggleBurst()
    {
        if (burstAvalible == false)
            return;

        burstActive = !burstActive;

        if (burstActive)
        {
            bulletsPerShot = burstModeBulletsPerShot;
            fireRate = burstFireRate;
        }
        else
        {
            bulletsPerShot = 1;
            fireRate = defaultFireRate;
        }
    }

    #endregion

    public bool CanShoot() => HaveEnoughBullets() && ReadyToFire();
    private bool ReadyToFire()
    {   

        if(Time.time > lastShootTime + 1 / fireRate)
        {
            lastShootTime = Time.time; // 25 seconds
            return true;
        }

        return false;
    }

    #region Reload methods
    public bool CanReload()
    {
        if (bulletsInMagazine == magazineCapacity)
            return false;

        if (totalReserveAmmo > 0)
        {
            return true;
        }
        return false;
    }
    public void RefillBullets()
    {
        // totalReserveAmmo += bulletsInMagazine;
        // this will add bullets in magazine to total amount of bullets
        Debug.Log($"Refilling bullets. Magazine Capacity: {magazineCapacity}, Total Reserve Ammo: {totalReserveAmmo}");

        int bulletsToReload = magazineCapacity;

        if (bulletsToReload > totalReserveAmmo)
            bulletsToReload = totalReserveAmmo;

        totalReserveAmmo -= bulletsToReload;
        bulletsInMagazine = bulletsToReload;

        if (totalReserveAmmo < 0)
            totalReserveAmmo = 0;

        Debug.Log($"After reload - Bullets in Magazine: {bulletsInMagazine}, Remaining Reserve Ammo: {totalReserveAmmo}");
    }
    private bool HaveEnoughBullets() => bulletsInMagazine > 0;
    #endregion
}