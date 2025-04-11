using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;

    [Header("Aim Visual - Laser")]
    [SerializeField] private LineRenderer aimLaser;

    [Header("Aim control")]
    [SerializeField] private Transform aim;

    [SerializeField] private bool isAimingPrecisly;
    [SerializeField] private bool isLockingToTarget;

    [Header("Camera control")]
    [SerializeField] private Transform cameraTarget;
    [Range(.5f,1)]
    [SerializeField] private float minCameraDistance = 1.5f;
    [Range(1,3f)]
    [SerializeField] private float maxCameraDistance = 4;

    [Range(3f,5f)]
    [SerializeField] private float cameraSensetivity = 5f;

    [Space]

    [SerializeField] private LayerMask aimLayerMask;

    private Vector2 mouseInput;
    private RaycastHit lastKnownMouseHit;

    private void Start()
    {
        player = GetComponent<Player>();
        AssignInputEvent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            isAimingPrecisly = !isAimingPrecisly;

        if(Input.GetKeyDown(KeyCode.L))
            isLockingToTarget = !isLockingToTarget;

        UpdateAimVisuals();
        UpdateAimPosition();
        UpdateCameraPosition();
    }

    private void UpdateAimVisuals()
    {
        aimLaser.enabled = player.weapon.WeaponReady();

        if (aimLaser.enabled == false)
            return;

        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();

        weaponModel.transform.LookAt(aim);
        weaponModel.gunPoint.LookAt(aim);

        Transform gunPoint = player.weapon.GunPoint();
        Vector3 laserDirection = player.weapon.BulletDirection();

        float laserTipLenght = .5f;
        float gunDistance = player.weapon.CurrentWeapon().gunDistance;


        Vector3 endPoint = gunPoint.position + laserDirection * gunDistance;

        if(Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hit, gunDistance))
        {
            endPoint = hit.point;
            laserTipLenght = 0;
        }

        aimLaser.SetPosition(0, gunPoint.position);
        aimLaser.SetPosition(1, endPoint);
        aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLenght);
    }
    private void UpdateAimPosition()
    {
        Transform target = Target();

        if (target != null && isLockingToTarget)
        {
            if(target.GetComponent<Renderer>() != null)
            {
                aim.position = target.GetComponent<Renderer>().bounds.center;
            }
            else
                aim.position = target.position;


            return;
        }

        aim.position = GetMouseHitInfo().point;


        if (!isAimingPrecisly)
            aim.position = new Vector3(aim.position.x, transform.position.y + 1, aim.position.z);
    }



    public Transform Target()
    {
        Transform target = null;

        if(GetMouseHitInfo().transform.GetComponent<Target>() != null)
        {
            target = GetMouseHitInfo().transform;
        }

        return target;
    }
    public Transform Aim() => aim;
    public bool CanAimPrecisly() => isAimingPrecisly;

    public RaycastHit GetMouseHitInfo()
    {
        Ray ray = Camera.main.ScreenPointToRay(mouseInput);

        if(Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
        {
            lastKnownMouseHit = hitInfo;
            return hitInfo;
        }

        return lastKnownMouseHit;
    }

    #region Camera Region

    private void UpdateCameraPosition()
    {
        cameraTarget.position = DesieredCameraPosition();
    }

    private Vector3 DesieredCameraPosition()
    {
        float actualMaxCameraDistance = player.movement.moveInput.y < -.5f ? minCameraDistance : maxCameraDistance;

        Vector3 DesieredCameraPosition = GetMouseHitInfo().point;
        Vector3 aimDirection = (DesieredCameraPosition - transform.position).normalized;

        float distanceToDesierdPosition = Vector3.Distance(transform.position, DesieredCameraPosition);
        float clampDistance = Mathf.Clamp(distanceToDesierdPosition, minCameraDistance, actualMaxCameraDistance);

        DesieredCameraPosition = transform.position + aimDirection * clampDistance;
        DesieredCameraPosition.y = transform.position.y + 1;

        return DesieredCameraPosition;
    }
    #endregion
    private void AssignInputEvent()
    {
        controls = player.controls;

        controls.Character.Aim.performed += context => mouseInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => mouseInput = Vector2.zero;
    }
}
