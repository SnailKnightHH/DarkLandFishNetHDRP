//using Unity.Netcode.Transports.UTP;
//using Unity.Services.Authentication;
//using Unity.Services.Core;
//using Unity.Services.Relay;
//using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using FishNet.Object;
using System.Threading.Tasks;


public class TestRelay : MonoBehaviour
{
    //    public static TestRelay Instance { get; private set; }

    //    private void Awake()
    //    {
    //        if (Instance != null && Instance != this)
    //        {
    //            Destroy(this);
    //        }
    //        else
    //        {
    //            Instance = this;
    //        }
    //    }

    //    public async Task<string> CreateRelay()
    //    {
    //        try
    //        {
    //            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);

    //            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

    //            Debug.Log(joinCode);

    //            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

    //            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

    //            NetworkManager.Singleton.StartHost();
    //            return joinCode;
    //        }
    //        catch (RelayServiceException e) { Debug.Log(e); }
    //        return null;
    //    }

    //    public async void JoinRelay(string joinCode)
    //    {
    //        try
    //        {
    //            Debug.Log("Joining Relay with " + joinCode);
    //            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

    //            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

    //            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

    //            NetworkManager.Singleton.StartClient();
    //        }
    //        catch (RelayServiceException ex) { Debug.Log(ex); }
    //    }
}
