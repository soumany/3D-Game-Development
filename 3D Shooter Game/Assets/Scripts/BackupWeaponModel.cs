using UnityEngine;

public enum HangType { LowBackHang, BackHange, SideHang }

public class BackupWeaponModel : MonoBehaviour
{
    public WeaponType weaponType;
    [SerializeField] private HangType hangType;

    public void Activate(bool activated) => gameObject.SetActive(activated);
    public bool HangTypeIs(HangType hangType) => this.hangType == hangType;
}
