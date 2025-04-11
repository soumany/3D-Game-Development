using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    private PlayerWeaponVisuals visualController;
    private PlayerWeaponController weaponController;

    private void Start()
    {
        visualController = GetComponentInParent<PlayerWeaponVisuals>();
        weaponController = GetComponentInParent<PlayerWeaponController>();

        if (weaponController == null)
            Debug.LogError("PlayerAnimationEvent: weaponController is NULL! Make sure it exists in the parent object.");
    }

    public void ReloadIsOver()
    {
        if (visualController == null || weaponController == null)
        {
            Debug.LogError("ReloadIsOver: visualController or weaponController is NULL!");
            return;
        }

        Weapon currentWeapon = weaponController.CurrentWeapon();

        if (currentWeapon == null)
        {
            Debug.LogError("ReloadIsOver: No weapon equipped! Cannot reload.");
            return;
        }

        visualController.MaximizeRigWeight();
        currentWeapon.RefillBullets();
        weaponController.SetWeaponReady(true);
    }

    public void ReturnRig()
    {
        if (visualController != null)
        {
            visualController.MaximizeRigWeight();
            visualController.MaximizeLeftHandWeight();
        }
    }

    public void WeaponEquipingIsOver()
    {
        if (visualController != null)
        {

            weaponController.SetWeaponReady(true);
        }
    }

    public void SwitchOnWeaponModel() => visualController.SwitchOnCurrentWeaponModel();
}
