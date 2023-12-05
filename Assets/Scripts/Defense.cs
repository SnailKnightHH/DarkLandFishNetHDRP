using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class Defense : PickupableObject, ITriggerCollider
{
    [HideInInspector] public bool isDeployed;
    private float rotateSpeed = 10f;
    [SerializeField] private LayerMask walkableLayerMask;
    private Vector3 HitGroundLocation = Vector3.zero;
    private Vector3 LastHitGroundLocation = Vector3.zero;
    [SerializeField] private float heightOffset; // if offset too small object will fly when deployed, don't know why

    // Materials
    // Material Idx for RPCs: 1: canDeployMaterial; 2: cannotDeployMaterial; 3: DeployedMaterial
    [SerializeField] protected Material DeployedMaterial;
    [SerializeField] protected Material canDeployMaterial;
    [SerializeField] protected Material cannotDeployMaterial;
    protected MeshRenderer[] meshRenders;
    private int numOfObjectsInMeshTriggerCollider = 0;

    public override void DisableOrEnableMesh(bool state)
    {
        foreach (MeshRenderer meshRenderer in meshRenders)
        {
            meshRenderer.enabled = state;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeMaterialServerRpc(int materialIdx)
    {
        ChangeMaterialObserversRpc(materialIdx);
    }

    [ObserversRpc]
    private void ChangeMaterialObserversRpc(int materialIdx)
    {
        Material EquipMaterial = DeployedMaterial;
        switch (materialIdx)
        {
            case 1:
                EquipMaterial = canDeployMaterial;
                break;
            case 2:
                EquipMaterial = cannotDeployMaterial;
                break;
            case 3:
                EquipMaterial = DeployedMaterial;
                break;
            default:
                break;
        }
        foreach (MeshRenderer meshRenderer in meshRenders)
        {
            meshRenderer.material = EquipMaterial;
        }
    }

    public bool IfCanDeploy()
    {
        if (numOfObjectsInMeshTriggerCollider == 0)
        {
            if (IsHost)
            {
                ChangeMaterialObserversRpc(1);
            } else
            {
                ChangeMaterialServerRpc(1);
            }
            return true;

        }
        else
        {
            if (IsHost)
            {
                ChangeMaterialObserversRpc(2);
            }
            else
            {
                ChangeMaterialServerRpc(2);
            }
            return false;
        }
    }

    private bool CanDeploy = false;

    public override void Dropoff(int numberOfItems)
    {        
        GetComponentInChildren<Collider>().isTrigger = false;
        GetComponent<PickupableObject>().UpdatePickUpStatusServerRpc(false);
        GetComponent<PickupableObject>().SetCarryMountTransform(null);
        GetComponent<PickupableObject>().SetCameraViewTransform(null);
        GetComponent<PickupableObject>().numOfItem = numberOfItems;
        GetComponent<Defense>().isDeployed = true;
        GetComponent<PickupableObject>().RemoveClientOwnershipServerRpc();
    }

    public virtual void onEnterDetectionZone(Collider other, GameObject initiatingGameObject)
    {
        // Since there are multiple trigger colliders, we distinguish which one by initiatingGameObject's name
        if (!isDeployed && initiatingGameObject.name == "Mesh" && other.tag != Utilities.IGNORED_BY_TRIGGER_COLLIDER) // Todo: fixed string, not robust?
        {
            numOfObjectsInMeshTriggerCollider++;
            CanDeploy = IfCanDeploy();
        }
    }

    public virtual void onExitDetectionZone(Collider other, GameObject initiatingGameObject)
    {
        if (!isDeployed && initiatingGameObject.name == "Mesh" && other.tag != Utilities.IGNORED_BY_TRIGGER_COLLIDER) // Todo: fixed string, not robust?
        {
            numOfObjectsInMeshTriggerCollider--;
            CanDeploy = IfCanDeploy();
        }
    }

    public virtual void onStayDetectionZone(Collider other, GameObject initiatingGameObject)
    {
    }

    protected override void PickulableObjectFollowTransform()
    {
        if (HitGroundLocation == Vector3.zero)
        {
            transform.position = followCarryMountTransform.position;
        } else
        {
            transform.position = new Vector3(followCarryMountTransform.position.x, HitGroundLocation.y + heightOffset, followCarryMountTransform.position.z);
            //Debug.Log("Normal debug: " + transform.position + " " + HitGroundLocation);
        }
    }    
    

    private void OnDrawGizmos()
    {
        if (HitGroundLocation != Vector3.zero)
        {
            Gizmos.DrawWireSphere(HitGroundLocation, 0.5f);
        }
    }

    public virtual bool Deploy()
    {
        if (!CanDeploy) { return false; }

        if (IsHost)
        {
            ChangeMaterialObserversRpc(3);
        } else
        {
            ChangeMaterialServerRpc(3);
        }
        return true;
    }


    protected virtual void Update()
    {
        if (!isDeployed)
        {
            if (RotaryHeart.Lib.PhysicsExtension.Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, Mathf.Infinity, walkableLayerMask, QueryTriggerInteraction.Ignore, RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Both))
            {                
                HitGroundLocation = LastHitGroundLocation = hitInfo.point;
                Quaternion rotationBasedOnSurface = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), rotateSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(rotationBasedOnSurface.eulerAngles.x, transform.eulerAngles.y, rotationBasedOnSurface.eulerAngles.z);
            } else
            {
                HitGroundLocation = LastHitGroundLocation;
            }
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        meshRenders = GetComponentsInChildren<MeshRenderer>();
        CanDeploy = IfCanDeploy();
    }
}
