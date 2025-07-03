using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;
    
    //public Transform playerSpawnPoint;
    public SpawnPoint[] tenPlayerSpawnPoints;
    
    public GameObject[] characterPrefabs = new GameObject[10];
    
    public Button[] CharacterSelectButton = new Button[10];

    [SerializeField] private GameObject CharacterSelectPanel;
    [SerializeField] private GameObject killGameButton;
    [SerializeField] private GameObject killGamePanel;
    [SerializeField] private SceneManager sceneManager;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        runner = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
 
        if (!runner.IsRunning)
        {
            Debug.LogWarning("NetworkRunner or GameManager not initialized. Cannot call RPC.");
            return;
        }
        else
        {
            Debug.Log("NetworkRunner initialized successfully.");
        }
        
        runner.AddCallbacks(this);
        
        if(runner.IsSharedModeMasterClient)
            killGameButton.SetActive(true);
    }


    
    public void CallRpc(int playerColorIndex)
    {
        Debug.Log("Calling RPCRequestSpawnPointRpc");
        RPCRequestSpawnPointRpc(playerColorIndex);
        
        CharacterSelectPanel.SetActive(false);
    }
    
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCRequestSpawnPointRpc(int playerColorIndex, RpcInfo info = default)
    {
        Debug.Log("helloooo");
        
        int spawnSpawnIndex = 0;
        SpawnPoint targetSpawnPoint;
        
        do
        {
            spawnSpawnIndex = Random.Range(0, tenPlayerSpawnPoints.Length);
            targetSpawnPoint = tenPlayerSpawnPoints[spawnSpawnIndex];
        } while (targetSpawnPoint.isTaken);
    
        targetSpawnPoint.isTaken = true;
        Debug.Log("byeee");
        RPCSetSpawnPoint(info.Source,spawnSpawnIndex, playerColorIndex);
    }
    
    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // private void RPCSetSpawnPoint(int spawnPointIndex, int playerColorIndex, PlayerRef playerRef)
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)] 
    private void RPCSetSpawnPoint([RpcTarget] PlayerRef playerRef,int spawnPointIndex, int playerCharacterIndex)
    {
        Debug.Log("RPCSetSpawnPoint");
        SpawnPoint targetSpawnPoint = tenPlayerSpawnPoints[spawnPointIndex];

        targetSpawnPoint.isTaken = true;
        
           var obj =  runner.Spawn
            (
                characterPrefabs[playerCharacterIndex],
                targetSpawnPoint.transform.position,
                targetSpawnPoint.transform.rotation
            );
           
            RPCToggleCharSelectButton(playerCharacterIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCToggleCharSelectButton( int playerCharacterIndex)
    {
        CharacterSelectButton[playerCharacterIndex].interactable = false;
    }
    
    
    public void MasterKillGame() { RPCKillGameForAll(); }
    
    
    [Rpc]
    private void RPCKillGameForAll() { killGamePanel.SetActive(true); }

    
    public void KillGame() { runner.Shutdown(); }
    
    
    public void LeaveAfterHost()
    {
        if (runner != null && runner.IsRunning)
        {
            runner.Shutdown(); 
        }
    }

    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        sceneManager.OfflineMoveToScene("Lobby");
    }
    

    #region TheShadowRealm
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
    
    #endregion
}


