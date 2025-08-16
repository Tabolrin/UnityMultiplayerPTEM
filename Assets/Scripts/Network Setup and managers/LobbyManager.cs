using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Multiplayer;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using Random = System.Random;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public const string LOBBY_SCENE_NAME = "Lobby";
    public const string HARD_GAME_SCENE_NAME = "HarderMap";
    public const string EASY_GAME_SCENE_NAME = "EasyMap";
    public const string END_SCREEN_SCENE_NAME = "EndingScene";

    
    const string EndersLobby = "Ender's";
    const string MoxieLobby  = "Moxie's";
    const string BillsLobby  = "Bill's";

    private string currentLobby;
    private bool NewSessionCreation = false;
    private List<SessionInfo> sessionList = new List<SessionInfo>();

    [Header("Critical Dependencies")] 
    [SerializeField] private GameObject networkRunnerPrefab;
    [SerializeField] private NetworkRunner _runner;
    [SerializeField] private GameObject sessionButtonPrefab;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private UiNotificationTexts uiNotificationTexts;
    
    [Header("Player Data")]
    [SerializeField] private NetworkObject playerDataPrefab;

    [Header("Panels")]
    [SerializeField] private CanvasGroup GenaralCanvasGroup;
    [SerializeField] private GameObject sessionListPanel;
    [SerializeField] private GameObject Lobbies;
    [SerializeField] private GameObject MidSessionPanel;
    [SerializeField] private GameObject SessionButtonLocations;
    [SerializeField] private GameObject newSessionPanel;
    [SerializeField] private GameObject PlayerRegistrationPanel;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationPanelText;
    
    [Header("Buttons")]
    [SerializeField] private Button[] lobbyButtons;
    [SerializeField] private Button startSessionButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button HostStartGameButton;
    private List<Button> existingSessionButtons = new List<Button>();
    
    [Header("Player Id's")]
    [SerializeField] private TMP_Text[] playerNamesTexts;
    
    [Header("New Session Input")]
    [SerializeField] private TMP_InputField newSessionNameInput;
    [SerializeField] private TMP_InputField numberOfPlayersInput;
    [SerializeField] private Toggle publicSessionToggle;
    
    [Header("New Player Input")]
    [SerializeField] private TMP_InputField playerNicknameInput;

    [Header("New session restriction settings")]
    [SerializeField] private int minimumPlayers = 4;
    [SerializeField] private int maximumPlayers = 8;

    private string pendingNickname;
    private string pendingSessionName = null;
    
    private bool shouldUpdatePlayerList = false;
    private float updatePlayerListTimer = 0;
    private int updatePlayerListInterval = 3; 
    
    private bool shouldPushName = false;
    private float updatePushNameTimer = 0;
    private int updatePushNameInterval = 3; 
    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if (_runner != null)
            _runner.AddCallbacks(this);
    }
    
    async Task<StartGameResult> StartGameTutorial(GameMode mode, string SessionNameInput)
    {
        var scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        
        StartGameArgs args = new StartGameArgs()
        {
            GameMode       = mode,
            SessionName    = SessionNameInput,
            CustomLobbyName= _runner.LobbyInfo != null ? _runner.LobbyInfo.Name : null,
            IsVisible      = publicSessionToggle != null ? publicSessionToggle.isOn : true,
            //TODO: REMOVE IF not needed
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        if (mode == GameMode.Host)
            args.PlayerCount = int.Parse(numberOfPlayersInput.text);

        StartGameResult resTask = await _runner.StartGame(args);

        return resTask;
    }

    public async Task StartNewSession()
    {
        ToggleButtonInteractivity(startSessionButton);
        
        StartGameResult resTask = await StartGameTutorial(GameMode.Host, newSessionNameInput.text);

        if (resTask.Ok)
        {
            TogglePanelVisibility(newSessionPanel);
            OnGameStarted(_runner);
            
            //TryApplyPendingNickname();
        }
        else
        {
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
            DataValidationError(uiNotificationTexts.FailedToStartSession);
            ToggleButtonInteractivity(startSessionButton);
        }
    }

    bool NewSessionDataValidation()
    {
        if (string.IsNullOrEmpty(newSessionNameInput.text) || string.IsNullOrEmpty(numberOfPlayersInput.text))
            return false;
        
        if (newSessionNameInput.text.IsNullOrEmpty())
        {
            DataValidationError(uiNotificationTexts.InvalidSessionName);
            return false;
        }
        
        foreach (SessionInfo session in sessionList)
        {
            if (session.Name == newSessionNameInput.text)
            {
                DataValidationError(uiNotificationTexts.SessionNameAlreadyExists);
                return false;
            }
        }
        
        if (int.Parse(numberOfPlayersInput.text) > maximumPlayers)
        {
            DataValidationError(uiNotificationTexts.MaximalPlayerCountExceeded);
            return false;
        }
        
        if (int.Parse(numberOfPlayersInput.text) < minimumPlayers)
        {
            DataValidationError(uiNotificationTexts.MinimalPlayerCountNotReached);
            return false;
        }
        
        return true;
    }
    
    private void DataValidationError(string errorMessage)
    {
        Debug.LogError(errorMessage);
        notificationPanelText.text = errorMessage;
        TogglePanelVisibility(notificationPanel);
    }
    
    public async Task JoinSession(string sessionName)
    {
        ToggleButtonInteractivity(existingSessionButtons);
        
        StartGameResult resTask = await StartGameTutorial(GameMode.Client, sessionName);
        
        if (resTask.Ok)
        {
            OnGameStarted(_runner);
        }
        else
        {
            if (resTask.ShutdownReason == ShutdownReason.GameIsFull)
            {
                notificationPanelText.text = uiNotificationTexts.SessionFull;
                TogglePanelVisibility(notificationPanel);
            }
            
            ResetNetworkRunner();
            
            ToggleButtonInteractivity(existingSessionButtons);
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
        }
    }
    
    
    private void ResetNetworkRunner()
    {
        _runner = Instantiate(networkRunnerPrefab).GetComponent<NetworkRunner>();
        _runner.AddCallbacks(this); // ensure callbacks keep firing after reset
    }
    
    
    public async void JoinLobby(string lobbyName)
    {
        StartGameResult result = await _runner.JoinSessionLobby(SessionLobby.Custom, lobbyName);
        
        if (result.Ok)
        {
            TogglePanelVisibility(Lobbies);
            TogglePanelVisibility(sessionListPanel);
            Debug.Log("Joined Lobby!" + lobbyName);
            currentLobby = lobbyName;
        }
        else
        {
            Debug.LogError($"Game join failed: {result.ShutdownReason}");
            ToggleButtonInteractivity(lobbyButtons);
        }
    }


    private void Update()
    {
        updatePlayerListTimer -= Time.deltaTime;
        updatePushNameTimer -= Time.deltaTime;
        
        if (shouldUpdatePlayerList && updatePlayerListTimer <= 0)
        {
            updatePlayerListTimer = updatePlayerListInterval;
            UpdatePlayersList();
        }

        if (shouldPushName && updatePushNameTimer <= 0)
        {
            updatePushNameTimer = updatePushNameInterval;
            TryApplyPendingNickname();
        }
    }


    public void TogglePanelVisibility(GameObject panel)
    {
        if (panel)
        {
            if (panel == notificationPanel || panel == PlayerRegistrationPanel)
                GenaralCanvasGroup.interactable = !GenaralCanvasGroup.interactable;
                 
            panel.SetActive(!panel.activeSelf);
        }
        else
        {
            Debug.LogWarning("Panel is not assigned.");
        }
    }
    
    public void ToggleButtonInteractivity(Button button)
    {
        if (button)
            button.interactable = !button.interactable;
        else
            Debug.LogWarning("Button is not assigned");
    }
    
    public void ToggleButtonInteractivity(Button[] buttons)
    {
        if (buttons != null)
            foreach (Button button in buttons)
                ToggleButtonInteractivity(button);
        else
            Debug.LogWarning("Button is not assigned");
    }
    
    public void ToggleButtonInteractivity(List<Button> buttons)
    {
        if (buttons != null)
            foreach (Button button in buttons)
                ToggleButtonInteractivity(button);
        else
            Debug.LogWarning("Button is not assigned");
    }

    private void OnGameStarted(NetworkRunner obj)
    {
        Debug.Log("Game Started");
        
        TogglePanelVisibility(sessionListPanel);
        TogglePanelVisibility(MidSessionPanel);
        
        shouldUpdatePlayerList = true;
        
        if (_runner.IsServer)
        {
            HostStartGameButton.gameObject.SetActive(true);
            startGameButton.gameObject.SetActive(true);
        }
    }

    public void OnCreateSessionButton()
    {
        if (!NewSessionDataValidation())
            return;
        
        NewSessionCreation = true;
        TogglePanelVisibility(PlayerRegistrationPanel);
    }
    
    public void OnJoinSessionButton(string sessionName)
    {
        pendingSessionName = sessionName;
        NewSessionCreation = false;
        TogglePanelVisibility(PlayerRegistrationPanel);
    }
    
    public async void ApplyPlayerNickname()
    {
        string nick = playerNicknameInput != null ? playerNicknameInput.text?.Trim() : null;

        if (string.IsNullOrEmpty(nick))
        {
            DataValidationError(uiNotificationTexts.NicknameEmptyOrNull);
            return;
        }
        
        if (nick.Length < 2 || nick.Length > 10)
        {
            DataValidationError(uiNotificationTexts.NicknameLengthError);
            return;
        }

        pendingNickname = nick;

        TogglePanelVisibility(PlayerRegistrationPanel);

        if (NewSessionCreation)
        {
            await StartNewSession();  
            NewSessionCreation = false;
        }
        else
        {
            if(!pendingSessionName.IsNullOrEmpty())
                await JoinSession(pendingSessionName); // join existing session
            pendingSessionName = null; // reset after joining
        }
    }
    
    
    private void TryApplyPendingNickname()
    {
        if (_runner == null || string.IsNullOrEmpty(pendingNickname))
            return;

        shouldPushName = true;

        var data = PlayerData.Local(_runner);
        
        if (data != null)
        {
            Debug.Log($"Setting nickname for player: {data.Nickname} to {pendingNickname}");
            PlayerData.SetLocal(_runner, pendingNickname);
            pendingNickname = null;
            shouldPushName = false;
            Debug.Log( $"Nickname set for player: {data.Nickname}");
        }
    }
    
    public void MoveToGameScene()
    {
        if (_runner.ActivePlayers.Count() < minimumPlayers)
        {
            Debug.LogWarning($"Not enough players to start the game. Minimum required: {minimumPlayers}");
            notificationPanelText.text = $"Not enough players to start the game. Minimum required: {minimumPlayers}";
            TogglePanelVisibility(notificationPanel);
            return;
        }

        shouldUpdatePlayerList = false;
        bool isTeam0 = true;
        foreach (var player in _runner.ActivePlayers)
        {
            if (isTeam0)
                PlayerData.Get(_runner, player).Team = 0;
            else
                PlayerData.Get(_runner, player).Team = 1;
            
            isTeam0 = !isTeam0; 
        }
        
        if (_runner != null && _runner.IsRunning && _runner.IsServer)
        {
            _runner.SessionInfo.IsOpen = false;
            _runner.GetComponent<RunnerSimulatePhysics3D>().ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;
            _runner.ProvideInput = true;

            int map = UnityEngine.Random.Range(0, 2);
            switch (map)
            {
                case 0:
                    sceneManager.OnlineMoveToScene(EASY_GAME_SCENE_NAME);
                    break;
                
                case 1:
                    sceneManager.OnlineMoveToScene(HARD_GAME_SCENE_NAME);
                    break;
                
                default:
                    Debug.LogError("Invalid map selection, defaulting to easy game scene.");
                    sceneManager.OnlineMoveToScene(EASY_GAME_SCENE_NAME);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("NetworkRunner is not running or not assigned.");
        }
    }
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        this.sessionList = sessionList;
        
        foreach (var session in existingSessionButtons) 
            Destroy(session.gameObject);
        
        existingSessionButtons.Clear();
        
        foreach (var session in sessionList)
        {
            if (!session.IsVisible)
                continue;
            
            Debug.Log(session.Name);
            GameObject newSessionButton = Instantiate(sessionButtonPrefab, SessionButtonLocations.transform);
            ButtonTextRefHolder newButton = newSessionButton.GetComponent<ButtonTextRefHolder>();

            newButton.SessionName = session.Name;
            newButton.buttonText.text = "Room name: " + session.Name + "\nPlayer Count: " + session.PlayerCount;
            
            newButton.OnSessionButton.AddListener(OnJoinSessionButton);

            existingSessionButtons.Add(newButton.thisButton);
        }
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Joined: " + player);

        if (runner.IsServer && playerDataPrefab != null)
        {
            if (!runner.GetPlayerObject(player))
            {
                var dataObj = runner.Spawn(playerDataPrefab, Vector3.zero, Quaternion.identity, player);
                runner.SetPlayerObject(player, dataObj);
            }
        }

        if (player == runner.LocalPlayer)
        {
            Debug.Log($"if");
            shouldPushName = true;
            TryApplyPendingNickname();
        }
        Debug.Log($"ifn't");

        UpdatePlayersList();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left: " + player);

        if (runner.IsServer)
        {
            var po = runner.GetPlayerObject(player);
            if (po)
                runner.Despawn(po);
        }

        UpdatePlayersList();
    }

    public void UpdatePlayersList()
    {
        if (_runner == null) return;
        //Debug.Log("Updating Player List");

        List<PlayerRef> players = _runner.ActivePlayers.ToList();
        
        shouldUpdatePlayerList = false;
        for (int i = 0; i < playerNamesTexts.Length; ++i)
        {
            if (i >= players.Count)
            {
                playerNamesTexts[i].text = "";
            }
            else
            {
                var playerData = PlayerData.Get(_runner, players[i]);
                playerNamesTexts[i].text = (playerData != null && !playerData.Nickname.Value.IsNullOrEmpty()) ? playerData.Nickname.ToString() : "Joining...";
                
                if(!playerData || playerData.Nickname.Value.IsNullOrEmpty()) 
                    shouldUpdatePlayerList = true;
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Runner Shut Down, Reason: " + shutdownReason);
    }
    
    #region TheShadowRealm
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnSceneLoadDone(NetworkRunner runner) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}
    #endregion
}
