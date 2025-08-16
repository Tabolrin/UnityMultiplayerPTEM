using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    const string ENDING_SCENE_NAME = "EndingScene";

    public static GameManager Instance { get; private set; }
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
    [Networked] public AstronautColor Team0Color { get; set; } 
    [Networked] public AstronautColor Team1Color { get; set; }
    
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private UiNotificationTexts uiNotificationTexts;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private CanvasGroup genaralCanvasGroup;
    
    [Header("Panels")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private GameObject closedGamePanel;
    [SerializeField] private GameObject leaderboardPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button quitButton;
    
    [Header("Text Boxes")]
    [SerializeField] private TMP_Text[] leaderboardText;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private TMP_Text infoUiText;
    [SerializeField] private TMP_Text quitButtonText;
    
    private Coroutine runningInfoUiCoroutine;
    
    public AstronautColor GetTeamColor(byte team) => team == 0 ? Team0Color : Team1Color;

    public override void Spawned()
    {
        Instance = this;
        _nRunner = Runner;
        _nRunner.AddCallbacks(this);
        
        if (_nRunner.IsServer)
        {
            PickTeamColorsFromPool();
            ResetSpawnPools();
            SpawnAllPlayers();
            GameStarted = true;
            quitButtonText.text = "Close Room";
        }
        else
        {
            quitButtonText.text = "Quit To Lobby";
        }

        LecturerInfoForUselessJSON forYouLior = new LecturerInfoForUselessJSON
        {
            FinalGrade = 100,
            LecturerName = "Lior",
            LecturerTitles = new List<string> 
            { 
                "Hamartze",
                "Hamore",
                "Sensei",
                "Hamadrich Haruchani",
                "Guru",
                "Hashech",
                "Haechad Vehayachid",
                "Ha'agada",
                "Hamythos"
            }
        };

        string json = JsonUtility.ToJson(forYouLior);
        
        if(_nRunner.IsServer)
            RPC_PrintUselessJSON(json);
        
        StartRoundRequest();
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_PrintUselessJSON(string uselessJson)
    {
        
        
        LecturerInfoForUselessJSON uselessJSONTheRemake = JsonUtility.FromJson<LecturerInfoForUselessJSON>(uselessJson);
        string messege = "but ";
        foreach(string title in uselessJSONTheRemake.LecturerTitles)
        {
            messege += title + ", ";
        }
        
        messege += uselessJSONTheRemake.LecturerName +
                   "!. We couldnt find a usage for sending a JSON! so we sent this instead. We expect to get graded "
                   + uselessJSONTheRemake.FinalGrade + " points";

        notificationText.text = messege;
        ToggleObject(notificationPanel);
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
        if (Runner.IsServer)
        {
            RPC_StartRound();
        }
    }

    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_Goal(byte scoringTeam)
    {
        Debug.Log("RPC got " + scoringTeam);
        SaveWinningTeam.Instance.WinningTeam = scoringTeam;
        sceneManager.OnlineMoveToScene(ENDING_SCENE_NAME);
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
        
        Team0Color = c0;
        Team1Color = c1;

        foreach (var player in _nRunner.ActivePlayers)
        {
            if (PlayerData.Get(_nRunner, player).Team == 0)
                PlayerData.Get(_nRunner, player).TeamColor = Team0Color;
            else
                PlayerData.Get(_nRunner, player).TeamColor = Team1Color;
        }
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

        RPC_UpdateColors();
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_UpdateColors()
    {
        foreach (var player in _nRunner.ActivePlayers)
        {
            var data = PlayerData.Get(_nRunner, player);
            data.Avatar.GetComponent<EnderCharacterController>().SetColor(data.TeamColor);
        }
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
        if(runningInfoUiCoroutine != null)
            StopCoroutine(runningInfoUiCoroutine);
        
        runningInfoUiCoroutine = StartCoroutine(ClearInfoUiTextCoroutine());
        
        if (!Object.HasStateAuthority) return;
        
        Debug.Log( "coroutine star ted");
        SpawnedCharacters.Remove(player);
    }
    
    
    private IEnumerator ClearInfoUiTextCoroutine()
    {
        if (infoUiText == null) yield break;
        yield return new WaitForSeconds(8f);
        Debug.Log( "Clearing info UI text after delay.");
        infoUiText.text = string.Empty;
    }

    
    public void QuitButtonPressed()
    {
        if (_nRunner.IsServer)
        {
            RPC_CloseRoom();
        }
        else
        {
            Debug.Log("Quit To Lobby");
            RPC_UpdateHostPlayerLeft(PlayerData.Get(_nRunner, _nRunner.LocalPlayer).Nickname.Value,
                _nRunner.ActivePlayers.Count() - 1);
        }
    }

    [Rpc (RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_UpdateHostPlayerLeft(string name, int playerCount)
    {
        Debug.Log( "host rpc running Player left: " + name + ", Remaining players: " + playerCount);
        infoUiText.text = name + uiNotificationTexts.PlayerLeftMatch + playerCount;

        if (runningInfoUiCoroutine != null)
            StopCoroutine(runningInfoUiCoroutine);
        
        RPC_UpdatePlayersPlayerLeft(name, playerCount);
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    private void RPC_UpdatePlayersPlayerLeft(string name, int playerCount)
    {
        Debug.Log( "client rpc running Player left: " + name + ", Remaining players: " + playerCount);

        infoUiText.text = name + uiNotificationTexts.PlayerLeftMatch + playerCount;
        
        if (runningInfoUiCoroutine != null)
            StopCoroutine(runningInfoUiCoroutine);
        
        if(PlayerData.Get(_nRunner, _nRunner.LocalPlayer).Nickname == name)
            QuitGame();
    }
    

    [Rpc (RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_CloseRoom()
    {
        ToggleObject(closedGamePanel);
    }
    
    
    public void QuitGame()
    {
        if (_nRunner != null && _nRunner.IsRunning)
            _nRunner.Shutdown();
        
        sceneManager.OfflineMoveToScene(LobbyManager.LOBBY_SCENE_NAME);
    }

    public void OnInput(NetworkRunner r, NetworkInput input)
    {
        inputManager.OnInput(r, input);
    }
    
    public void ToggleObject(GameObject obj)
    {
        if (obj)
        {
            if (obj == notificationPanel || obj == closedGamePanel)
                genaralCanvasGroup.interactable = !genaralCanvasGroup.interactable;
                 
            obj.SetActive(!obj.activeSelf);
        }
        else
        {
            Debug.LogWarning("Object is not assigned.");
        }
    }
    

    #region TheShadowRealm5.0

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnConnectedToServer(NetworkRunner r) {}
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
