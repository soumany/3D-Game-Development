using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponVisuals : MonoBehaviour
{
    private Player player;
    private Animator anim;

    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private BackupWeaponModel[] backupWeaponModels;

    [Header("Rig")]
    [SerializeField] private float rigWeightIncreaseRate;
    private bool shouldIncrease_RigWeight;
    private Rig rig;

    [Header("Left Hand IK")]
    [SerializeField] private float leftHandIkWeightIncreaseRate;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandIK_Target;
    private bool shouldIncrease_LeftHandIKWeight;

    private void Start()
    {
        player = GetComponent<Player>();
        anim = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        backupWeaponModels = GetComponentsInChildren<BackupWeaponModel>(true);
    }

    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandIKWeight();
    }

    public void PlayFireAnimation() => anim.SetTrigger("Fire");
    public void PlayReloadAnimation()
    {
        
        float reloadSpeed = player.weapon.CurrentWeapon().reloadSpeed;

        anim.SetFloat("ReloadSpeed", reloadSpeed);
        anim.SetTrigger("Reload");
        ReduceRigWeight();
    }


    public void PlayWeaponEquipAnimation()
    {
        EquipType equipType = CurrentWeaponModel().equipAnimationType;

        float equipmentSpeed = player.weapon.CurrentWeapon().equipmentSpeed;

        leftHandIK.weight = 0;
        ReduceRigWeight();
        anim.SetTrigger("EquipWeapon");
        anim.SetFloat("EquipType", ((float)equipType));
        anim.SetFloat("EquipSpeed", equipmentSpeed);
    }

    public void SwitchOnCurrentWeaponModel()
    {
        WeaponModel currentWeapon = CurrentWeaponModel();
        if (currentWeapon == null)
        {
            Debug.LogError("CurrentWeaponModel() returned null!");
            return;
        }

        int animationIndex = ((int)currentWeapon.holdType);

        SwitchOffWeaponModels();
        SwitchOffBackupWeaponModels();

        if (player.weapon != null && !player.weapon.HasOnlyOneWeapon())
        {
            SwitchOnBackupWeaponModel();
        }

        SwitchAnimationLayer(animationIndex);
        currentWeapon.gameObject.SetActive(true);
        AttachLeftHand();
    }

    public void SwitchOffWeaponModels()
    {
        foreach (WeaponModel weaponModel in weaponModels)
        {
            if (weaponModel != null)
            {
                weaponModel.gameObject.SetActive(false);
            }
        }
    }

    private void SwitchOffBackupWeaponModels()
    {
        foreach (BackupWeaponModel backupModel in backupWeaponModels)
        {
            backupModel.Activate(false);
        }
    }

    public void SwitchOnBackupWeaponModel()
    {
        SwitchOffBackupWeaponModels();

        BackupWeaponModel lowHangWeapon = null;
        BackupWeaponModel backHangWeapon = null;
        BackupWeaponModel SideHangeWeapon = null;

        foreach (BackupWeaponModel backupModel in backupWeaponModels)
        {
            if (backupModel.weaponType == player.weapon.CurrentWeapon().weaponType)
                continue;


            if (player.weapon.WeaponInSlots(backupModel.weaponType) != null)
            {
                if (backupModel.HangTypeIs(HangType.LowBackHang))
                    lowHangWeapon = backupModel;

                if (backupModel.HangTypeIs(HangType.BackHange))
                    backHangWeapon = backupModel;

                if (backupModel.HangTypeIs(HangType.SideHang))
                    SideHangeWeapon= backupModel;
            }
        }

        lowHangWeapon?.Activate(true);
        backHangWeapon?.Activate(true);
        SideHangeWeapon?.Activate(true);
    }

    private void SwitchAnimationLayer(int layerIndex)
    {
        if (anim == null)
        {
            Debug.LogError("Animator component is null!");
            return;
        }

        for (int i = 1; i < anim.layerCount; i++)
        {
            anim.SetLayerWeight(i, 0);
        }

        anim.SetLayerWeight(layerIndex, 1);
    }

    public WeaponModel CurrentWeaponModel()
    {
        if (player.weapon == null)
        {
            Debug.LogError("Player weapon is null!");
            return null;
        }

        WeaponType weaponType = player.weapon.CurrentWeapon().weaponType;

        foreach (WeaponModel weaponModel in weaponModels)
        {
            if (weaponModel.weaponType == weaponType)
                return weaponModel;
        }

        Debug.LogWarning("No matching weapon model found for weapon type: " + weaponType);
        return null;
    }

    #region Animation Rigging Methods
    private void AttachLeftHand()
    {
        WeaponModel currentWeapon = CurrentWeaponModel();
        if (currentWeapon == null)
        {
            Debug.LogError("Cannot attach left hand - CurrentWeaponModel() returned null!");
            return;
        }

        Transform targetTransform = currentWeapon.holdPoint;
        if (targetTransform == null)
        {
            Debug.LogError("Weapon holdPoint is null!");
            return;
        }

        leftHandIK_Target.localPosition = targetTransform.localPosition;
        leftHandIK_Target.localRotation = targetTransform.localRotation;
    }

    private void UpdateLeftHandIKWeight()
    {
        if (shouldIncrease_LeftHandIKWeight)
        {
            leftHandIK.weight += leftHandIkWeightIncreaseRate * Time.deltaTime;

            if (leftHandIK.weight >= 1)
            {
                shouldIncrease_LeftHandIKWeight = false;
            }
        }
    }

    private void UpdateRigWeight()
    {
        if (shouldIncrease_RigWeight)
        {
            rig.weight += rigWeightIncreaseRate * Time.deltaTime;

            if (rig.weight >= 1)
            {
                shouldIncrease_RigWeight = false;
            }
        }
    }

    private void ReduceRigWeight()
    {
        if (rig != null)
        {
            rig.weight = .15f;
        }
        else
        {
            Debug.LogError("Rig component is null!");
        }
    }

    public void MaximizeRigWeight() => shouldIncrease_RigWeight = true;
    public void MaximizeLeftHandWeight() => shouldIncrease_LeftHandIKWeight = true;
    #endregion
}
