using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;

    public SpawnPoint[] team1SpawnPoints;
    public SpawnPoint[] team2SpawnPoints;    
    public GameObject[] characterPrefabs = new GameObject[10];

    public Button[] CharacterSelectButton = new Button[10];

    private List<NetworkObject> characters = new List<NetworkObject>();

    private Dictionary<PlayerRef, int> playerTeams = new Dictionary<PlayerRef, int>();

    [SerializeField] private GameObject CharacterSelectPanel;
    [SerializeField] private GameObject killGameButton;
    [SerializeField] private GameObject killGamePanel;
    [SerializeField] private SceneManager sceneManager;

    public static event Action OnRoundOver;
    public static event Action OnRoundStarted;


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



    //===Changed for teams=====
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPCRequestSpawnPointRpc(int playerCharacterIndex, RpcInfo info = default)
    {
        int team = playerTeams[info.Source];
        SpawnPoint[] spawnList = (team == 1) ? team1SpawnPoints : team2SpawnPoints;

        SpawnPoint targetSpawnPoint;
        int spawnPointIndex;

        do
        {
            spawnPointIndex = Random.Range(0, spawnList.Length);
            targetSpawnPoint = spawnList[spawnPointIndex];
        }
        while (targetSpawnPoint.isTaken);

        targetSpawnPoint.isTaken = true;

        var obj = runner.SpawnAsync(
            characterPrefabs[playerCharacterIndex],
            targetSpawnPoint.transform.position,
            targetSpawnPoint.transform.rotation,
            info.Source
        );

        characters.Add(obj.Object);
        RPCToggleCharSelectButton(playerCharacterIndex);
    }



    /* [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)] 
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
     }*/


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
