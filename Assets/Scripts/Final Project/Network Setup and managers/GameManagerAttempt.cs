using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerNew : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static GameManagerNew Instance { get; private set; }
    public static event Action OnRoundStarted;

    private NetworkRunner _nRunner;
    public Dictionary<PlayerRef, NetworkObject> SpawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    [Header("Prefabs & Assets")]
    [SerializeField] private NetworkObject avatarPrefab;
    [SerializeField] private PlayerMaterialsContainer colorPool; 
    
    [Header("Team Spawn Points")]
    [SerializeField] private Transform[] team0SpawnPoints;
    [SerializeField] private Transform[] team1SpawnPoints;

    private List<Transform> _team0Pool, _team1Pool;

    [Header("Match State")]
    [Networked] public bool GameStarted { get; set; }
    
    [Header("Team Colors")]
    [Networked] public byte Team0ColorByte { get; set; } // stores AstronautColor as byte
    [Networked] public byte Team1ColorByte { get; set; }
    
    [SerializeField] private GameObject killGameButton;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private UiNotificationTexts uiNotificationTexts;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificatoinText;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private TMP_Text[] leaderboardText;

    public AstronautColor GetTeamColor(byte team) => (AstronautColor)(team == 0 ? Team0ColorByte : Team1ColorByte);

    public override void Spawned()
    {
        Instance = this;
        _nRunner = Runner;
        _nRunner.AddCallbacks(this);

        foreach (var player in _nRunner.ActivePlayers)
        {
            Debug.Log(PlayerData.Get(_nRunner, player).Nickname);
        }
        
        
        if (_nRunner.IsServer)
        {
            PickTeamColorsFromPool();
            ResetSpawnPools();
            SpawnAllPlayers();
            GameStarted = true;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (runner != null) runner.RemoveCallbacks(this);
        if (Instance == this) Instance = null;
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_StartRound()
    {
        OnRoundStarted?.Invoke();
    }
    
    
    private void StartRoundRequest()
    {
        if (Object.HasStateAuthority)
        {
            RPC_StartRound();
        }
    }

    
    private void PickTeamColorsFromPool()
    {
        const int colorCount = 8;
        
        // pick one for team 0
        AstronautColor c0 = (AstronautColor)(Random.Range(0, colorCount));

        // pick a different one for team 1
        AstronautColor c1 = c0;
        
        int guard = 0;
        while (c1.Equals(c0) && guard++ < colorCount)
            c1 = (AstronautColor)Random.Range(0, colorCount);
        
        Team0ColorByte = (byte)c0;
        Team1ColorByte = (byte)c1;
    }

    private void ResetSpawnPools()
    {
        if (team0SpawnPoints == null || team0SpawnPoints.Length == 0 ||
            team1SpawnPoints == null || team1SpawnPoints.Length == 0)
        {
            Debug.LogError("Team spawn points are not set or empty!");
            return;
        }
        
        _team0Pool = new List<Transform>(team0SpawnPoints);
        _team1Pool = new List<Transform>(team1SpawnPoints);
    }

    
    private Transform TakeSpawn(byte team)
    {
        var pool = (team == 0) ? _team0Pool : _team1Pool;
        var all  = (team == 0) ? team0SpawnPoints : team1SpawnPoints;

        if (all == null || all.Length == 0) return null;

        if (pool.Count == 0)
            return all[Random.Range(0, all.Length)];

        int index = Random.Range(0, pool.Count);
        var transform = pool[index];
        pool.RemoveAt(index);
        return transform;
    }

    private void SpawnAllPlayers()
    {
        foreach (var p in _nRunner.ActivePlayers)
            SpawnOne(p);
    }

    private void SpawnOne(PlayerRef player)
    {
        if (!_nRunner.IsServer) return;

        var data = PlayerData.Get(_nRunner, player);
        if (data == null) return;

        var spawn = TakeSpawn(data.Team);
        var pos   = spawn ? spawn.position : Vector3.zero;
        var rot   = spawn ? spawn.rotation : Quaternion.identity;

        var avatar = _nRunner.Spawn(avatarPrefab, pos, rot, player);
        SpawnedCharacters.Add(player, avatar);
        data.Avatar = avatar;
    }
    

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;
        if (GameStarted) SpawnOne(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;

        var data = PlayerData.Get(runner, player);
        if (data != null && data.Avatar) 
        {
            runner.Despawn(data.Avatar);
            SpawnedCharacters.Remove(player);
            data.Avatar = null;        
        }
    }

    public void MasterKillGame() { RPCKillGameForAll(); }


    [Rpc]
    private void RPCKillGameForAll()
    {
        notificatoinText.text = uiNotificationTexts.MasterKillGame;
        notificationPanel.SetActive(true);
    }

    
    public void KillGame() { _nRunner.Shutdown(); }
    
    
    public void LeaveAfterHost()
    {
        if (_nRunner != null && _nRunner.IsRunning)
        {
            _nRunner.Shutdown(); 
        }
    }

    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        sceneManager.OfflineMoveToScene("Lobby");
    }
    
    

    #region TheShadowRealm5.0

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnInput(NetworkRunner r, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnConnectedToServer(NetworkRunner r) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] t) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> s) {}
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) {}
    public void OnHostMigration(NetworkRunner r, HostMigrationToken t) {}
    public void OnSceneLoadDone(NetworkRunner r) {}
    public void OnSceneLoadStart(NetworkRunner r) {}
    #endregion
}
