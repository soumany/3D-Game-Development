using UnityEngine;

public class Pickup_Weapon : Interactable
{
    private PlayerWeaponController weaponController;
    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private Weapon weapon;

    [SerializeField] private BackupWeaponModel[] models;

    private bool oldWeapon;

    private void Start()
    {
        if(oldWeapon == false)
            weapon = new Weapon(weaponData);


        UpdateGameObject();
    }

    public void SetupPickWeapon(Weapon weapon, Transform transform)
    {
        oldWeapon = true;

        this.weapon = weapon;
        weaponData = weapon.weaponData;

        this.transform.position = transform.position + new Vector3 (0, .75f, 0);
    }

    [ContextMenu("Update Item Model")]
    public void UpdateGameObject()
    {
        gameObject.name = "Pickup_Weapon - " + weaponData.weaponType.ToString();

        UpdateItemModel();
    }

    public void UpdateItemModel()
    {
        foreach (BackupWeaponModel model in models)
        {
            model.gameObject.SetActive(false);

            if (model.weaponType == weaponData.weaponType)
            {
                model.gameObject.SetActive(true);
                UpdateMeshAndMaterial(model.GetComponent<MeshRenderer>());
            }
        }
    }
    public override void Interaction()
    {
        weaponController.PickupWeapon(weapon);

        ObjectPool.instance.ReturnObject(gameObject);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (weaponController == null)
            weaponController = other.GetComponent<PlayerWeaponController>();
    }
}
