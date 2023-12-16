using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static AudioManager;

[Serializable]
class SoundEnumToClipMapping
{
    public AudioManager.SoundName SoundName;
    public AudioClip[] audioClip;
}

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum SoundName
    {
        Walk,
        Run,
        Jump,
        pistol
    }

    public enum SoundType
    {
        Discrete,
        Continuous
    }

    [SerializeField] private List<SoundEnumToClipMapping> SoundNameToAudioClipMapping;
    //NetworkManager.ClientManager.Connection.ClientId
    private Dictionary<SoundName, AudioClip[]> soundNameToAudioClipDict = new Dictionary<SoundName, AudioClip[]>();
    private Dictionary<int, Dictionary<SoundName, float>> soundTimerDict = new Dictionary<int, Dictionary<SoundName, float>>();
    private Dictionary<int, Dictionary<SoundName, bool>> keepPlayingSoundDict = new Dictionary<int, Dictionary<SoundName, bool>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        foreach (SoundEnumToClipMapping kvp in SoundNameToAudioClipMapping)
        {
            soundNameToAudioClipDict.Add(kvp.SoundName, kvp.audioClip);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        foreach (var kvp in NetworkManager.ClientManager.Clients)
        {
            int ClientId = kvp.Value.ClientId;
            SoundDictInitialize(ClientId);
        }
        int myClientId = NetworkManager.ClientManager.Connection.ClientId;
        SoundDictInitialize(myClientId);
        SoundDictInitializationServerRpc(myClientId);
    }

    private void SoundDictInitialize(int ClientId)
    {
        soundTimerDict[ClientId] = new Dictionary<SoundName, float>();
        soundTimerDict[ClientId][SoundName.Walk] = 0;
        soundTimerDict[ClientId][SoundName.Run] = 0;
        keepPlayingSoundDict[ClientId] = new Dictionary<SoundName, bool>();
        keepPlayingSoundDict[ClientId][SoundName.Walk] = false;
        keepPlayingSoundDict[ClientId][SoundName.Run] = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SoundDictInitializationServerRpc(int ClientId)
    {
        SoundDictInitializationClientRpc(ClientId);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void SoundDictInitializationClientRpc(int ClientId)
    {
        SoundDictInitialize(ClientId);
    }

    private AudioClip GetRandomAudioClip(SoundName soundName)
    {
        return soundNameToAudioClipDict[soundName][UnityEngine.Random.Range(0, soundNameToAudioClipDict[soundName].Length)];
    }

    public void PlayAudioContinuousLocal(AudioSource audioSource, SoundName soundName, int clientId)
    {
        if (CanPlaySound(soundName, clientId))
        {
            audioSource.PlayOneShot(GetRandomAudioClip(soundName));
        }
    }

    public void PlayAudioContinuousNetwork(NetworkObject networkObject, SoundName soundName, bool isPlaying, int clientId)
    {
        keepPlayingSoundDict[clientId][soundName] = isPlaying;
        if (IsServer || IsHost)
        {
            keepPlayingSoundDict[clientId][soundName] = isPlaying;
#if UNITY_EDITOR
            Debug.Log(networkObject.LocalConnection.ClientId + "executed continuous sound " + soundName + " , bool: " + isPlaying);
#endif
            KeepPlayingSound(networkObject.GetComponentInChildren<AudioSource>(), soundName, () => keepPlayingSoundDict[clientId][soundName]);
            UpdatePlayerIsPlayingSoundStatusClientRpc(isPlaying, soundName, networkObject, clientId);
        }
        else
        {
            UpdatePlayerIsPlayingSoundStatusServerRpc(isPlaying, soundName, networkObject, clientId);
        }
    }



    public void PlayAudioDiscrete(NetworkObject networkObject, SoundName soundName)
    {
        if (IsServer || IsHost)
        {
            networkObject.GetComponentInChildren<AudioSource>().PlayOneShot(GetRandomAudioClip(soundName));
            PlayDiscreteSoundClientRpc(soundName, networkObject);
        }
        else
        {
            PlayDiscreteSoundServerRpc(soundName, networkObject);
        }   
    }

    private bool CanPlaySound(SoundName soundName, int clientId)
    {
        float lastTimePlayed;
        if (soundName == SoundName.Walk)
        {
            if (soundTimerDict[clientId].TryGetValue(soundName, out lastTimePlayed))
            {
                float playerMoveTimeInterval = 0.5f;
                if (lastTimePlayed + playerMoveTimeInterval < Time.time)
                {
                    soundTimerDict[clientId][SoundName.Walk] = Time.time;
                    return true;
                }
                else
                {
                    return false;
                }
            }
#if UNITY_EDITOR
            Debug.LogError("Sound Enum not found. This should not happen");
#endif
            return false;
        } else if (soundName == SoundName.Run)
        {
            if (soundTimerDict[clientId].TryGetValue(soundName, out lastTimePlayed))
            {
                float playerMoveTimeInterval = 0.2f;
                if (lastTimePlayed + playerMoveTimeInterval < Time.time)
                {
                    soundTimerDict[clientId][SoundName.Run] = Time.time;
                    return true;
                }
                else
                {
                    return false;
                }
            }
#if UNITY_EDITOR
            Debug.LogError("Sound Enum not found. This should not happen");
#endif
            return false;
        } else
        {
            return true;
        }
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void PlayDiscreteSoundServerRpc(SoundName soundName, NetworkObject networkObject)
    {
#if UNITY_EDITOR
        Debug.Log(networkObject.LocalConnection.ClientId + "executed discrete sound " + soundName);
#endif
        networkObject.GetComponentInChildren<AudioSource>().PlayOneShot(GetRandomAudioClip(soundName));
        PlayDiscreteSoundClientRpc(soundName, networkObject);
    }

    [ObserversRpc(ExcludeServer = true, ExcludeOwner = true, BufferLast = true)]
    public void PlayDiscreteSoundClientRpc(SoundName soundName, NetworkObject networkObject) // Todo: I think initiating client still runs this, so sound played twice
    {
#if UNITY_EDITOR
        Debug.Log(networkObject.LocalConnection.ClientId + "executed discrete sound " + soundName);
#endif
        networkObject.GetComponentInChildren<AudioSource>().PlayOneShot(GetRandomAudioClip(soundName));
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void UpdatePlayerIsPlayingSoundStatusServerRpc(bool isPlaying, SoundName soundName, NetworkObject networkObject, int clientId)
    {
        keepPlayingSoundDict[clientId][soundName] = isPlaying;
#if UNITY_EDITOR
        Debug.Log(networkObject.LocalConnection.ClientId + "executed continuous sound " + soundName + " , bool: " + isPlaying);
#endif
        KeepPlayingSound(networkObject.GetComponentInChildren<AudioSource>(), soundName, () => keepPlayingSoundDict[clientId][soundName]);
        UpdatePlayerIsPlayingSoundStatusClientRpc(isPlaying, soundName, networkObject, clientId);
    }

    [ObserversRpc(ExcludeServer = true)]
    public void UpdatePlayerIsPlayingSoundStatusClientRpc(bool isPlaying, SoundName soundName, NetworkObject networkObject, int clientId)
    {
        keepPlayingSoundDict[clientId][soundName] = isPlaying;
#if UNITY_EDITOR
        Debug.Log(networkObject.LocalConnection.ClientId + "executed continuous sound " + soundName + " , bool: " + isPlaying);
#endif
        KeepPlayingSound(networkObject.GetComponentInChildren<AudioSource>(), soundName, () => keepPlayingSoundDict[clientId][soundName]);
    }

    private IEnumerator KeepPlayingSound(AudioSource audioSource, SoundName soundName, Func<bool> ifKeepPlaying)
    {

        while (ifKeepPlaying())
        {
            audioSource.PlayOneShot(GetRandomAudioClip(soundName));
            yield return null;
        }
    }

    void Update()
    {

    }
}
