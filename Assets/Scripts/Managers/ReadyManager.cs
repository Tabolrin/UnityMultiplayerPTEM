using System;
using Fusion;
using UnityEngine;

public class ReadyManager : NetworkBehaviour
{
    public event Action onReadyCounterReachedMax;
    public int readyCounter = 0;
    
    [Rpc]
    public void SetReadyRPC(RpcInfo info = default)
    {
        Debug.Log($"Player id {info.Source.PlayerId} is ready");
        readyCounter++;
        if(readyCounter >= 2)
            onReadyCounterReachedMax?.Invoke();
    }

    public override void Spawned()
    {
        base.Spawned();
       // LobbyManager.Instance.readyManagerInstance = this;
     //   onReadyCounterReachedMax += LobbyManager.Instance.MaxPlayersReady;
    }
    
}
