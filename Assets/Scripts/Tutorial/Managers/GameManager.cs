using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;
    
    public SpawnPoint[] tenPlayerSpawnPoints;
    
    public GameObject[] characterPrefabs = new GameObject[10];

    public Button[] CharacterSelectButton = new Button[10];

    private List<NetworkObject> characters = new List<NetworkObject>();
    
    [SerializeField] private GameObject CharacterSelectPanel;
    [SerializeField] private GameObject killGameButton;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private UiNotificationTexts uiNotificationTexts;
    [SerializeField] private TMP_Text notificatoinText;
    
    
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
    
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPCRequestSpawnPointRpc(int playerCharacterIndex, RpcInfo info = default)
    {
        int spawnPointIndex = 0;
        SpawnPoint targetSpawnPoint;
        
        do
        {
            spawnPointIndex = Random.Range(0, tenPlayerSpawnPoints.Length);
            targetSpawnPoint = tenPlayerSpawnPoints[spawnPointIndex];
        } while (targetSpawnPoint.isTaken);
    
        targetSpawnPoint.isTaken = true;
        
        var obj =  runner.SpawnAsync
        (
            characterPrefabs[playerCharacterIndex],
            targetSpawnPoint.transform.position,
            targetSpawnPoint.transform.rotation,
            info.Source
        );
           
        characters.Add(obj.Object);
            
        RPCToggleCharSelectButton(playerCharacterIndex);
        
        //RPCSetSpawnPoint(info.Source,spawnSpawnIndex, playerColorIndex);
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)] 
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
           
            characters.Add(obj);
            
            RPCToggleCharSelectButton(playerCharacterIndex);
    }

    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCToggleCharSelectButton( int playerCharacterIndex)
    {
        CharacterSelectButton[playerCharacterIndex].interactable = false;
    }
    
    
    public void MasterKillGame() { RPCKillGameForAll(); }


    [Rpc]
    private void RPCKillGameForAll()
    {
        notificatoinText.text = uiNotificationTexts.MasterKillGame;
        notificationPanel.SetActive(true);
    }

    
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
