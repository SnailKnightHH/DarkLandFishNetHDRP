using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupableObject : Carryable, IThrowable
{
    [SerializeField] private float throwForce = 30;
    [SerializeField] private Item _item;
    private int _numOfItem = 1;

    [SyncVar(Channel = FishNet.Transporting.Channel.Reliable, ReadPermissions = ReadPermission.Observers, WritePermissions = WritePermission.ServerOnly)]
    private bool IsPickedUp;
    public bool isPickedUp => IsPickedUp;
    public int numOfItem
    {
        get
        {
            return _numOfItem;
        }
        set
        {
            _numOfItem = value;
        }
    }

    public Item objectItem
    {
        get { return _item; }
    }
    protected Transform followCarryMountTransform;
    protected Transform followCameraViewTransform;   
    private Rigidbody rb;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        rb = GetComponent<Rigidbody>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePickUpStatusServerRpc(bool isPickedUp)
    {
        IsPickedUp = isPickedUp;
    }

    // Parameter needs to be Transform (a reference type)
    public void SetCarryMountTransform(Transform followTransform) 
    {
        //Debug.Log("ServerRpc SetFollowTransformServerRpc called");
        this.followCarryMountTransform = followTransform;
    }

    public void SetCameraViewTransform(Transform followTransform)
    {
        this.followCameraViewTransform = followTransform;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeObjectOwnershipServerRpc(NetworkConnection sender = null)
    {
        GetComponent<NetworkObject>().GiveOwnership(sender); 
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveClientOwnershipServerRpc()
    {
        GetComponent<NetworkObject>().RemoveOwnership();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyNetworkObjectServerRpc()
    {
        base.Despawn();
    }

    //public void SetObjectPosition(Vector3 position)
    //{
    //    transform.position = position;
    //    SetObjectPositionServerRpc(position);
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void SetObjectPositionServerRpc(Vector3 position)
    //{
    //    transform.position = position;
    //}

    public virtual void DisableOrEnableMesh(bool state)
    {
        GetComponentInChildren<MeshRenderer>().enabled = state;
    }

    public virtual void Dropoff(int numberOfItems)
    {
        GetComponentInChildren<Collider>().isTrigger = false;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<PickupableObject>().UpdatePickUpStatusServerRpc(false);
        GetComponent<PickupableObject>().SetCarryMountTransform(null);
        GetComponent<PickupableObject>().SetCameraViewTransform(null);
        GetComponent<PickupableObject>().numOfItem = numberOfItems;                
        GetComponent<PickupableObject>().RemoveClientOwnershipServerRpc();
    }

    private void LateUpdate()
    {
        if (followCarryMountTransform == null)
        {
            return;
        }

        PickulableObjectFollowTransform();
    }

    protected virtual void PickulableObjectFollowTransform()
    {       
        transform.position = followCarryMountTransform.position;
        if (followCameraViewTransform != null)
        {
            transform.rotation = followCameraViewTransform.rotation;
        }        
    }

    public void Throw(Transform throwTransform)
    {
        Debug.Log("Throw " + _item.ItemName + " reached");
        rb.AddRelativeForce(new Vector3(0, 0.5f, 1) * throwForce, ForceMode.VelocityChange); // Don't need to define item weight separately since ForceMode.Impulse takes rb.mass into consideration        
    }
}
