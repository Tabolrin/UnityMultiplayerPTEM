/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public const string GAME_SCENE_NAME = "EnderArena";
    
    const string EndersLobby = "Ender's";
    const string MoxieLobby = "Moxie's";
    const string BillsLobby = "Bill's";

    private string currentLobby;
    private bool NewSessionCreation = false;
    private List<SessionInfo> sessionList = new List<SessionInfo>();
    
    public List<GameObject> playersTextBoxes = new List<GameObject>();
    
    public UnityEvent JoinLobbyEvent;

    [Header("Critical Dependencies")] 
    [SerializeField] private GameObject networkRunnerPrefab;
    [SerializeField] NetworkRunner _runner;
    [SerializeField] private GameObject sessionButtonPrefab;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private UiNotificationTexts uiNotificationTexts;
    [SerializeField] private ClientHostPrepModule hostPrep;
    //[SerializeField] private PlayerDataHandler playerDataHandler;
    
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
    List<Button> existingSessionButtons = new List<Button>();
    
    [Header("Player Id's")]
    [SerializeField] private TMP_Text[] playerNamesTexts;
    
    [Header("New Session Input")]
    [SerializeField] private TMP_InputField newSessionNameInput;
    [SerializeField] private TMP_InputField numberOfPlayersInput;
    [SerializeField] private Toggle publicSessionToggle;
    
    [Header("New Player Input")]
    [SerializeField] private TMP_InputField playerNicknameInput;
    
    //todo: remove?
    [Header("New session restriction settings")]
    [SerializeField] private int minimumPlayers = 4;
    [SerializeField] private int maximumPlayers = 8;

    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _runner.AddCallbacks(this);
    }
    
    
    async Task<StartGameResult> StartGameTutorial(GameMode mode, string SessionNameInput)
    {
        StartGameArgs args = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = SessionNameInput,
            CustomLobbyName = _runner.LobbyInfo != null ? _runner.LobbyInfo.Name : null,
            IsVisible = publicSessionToggle != null ? publicSessionToggle.isOn : true
        };

        if (mode == GameMode.Host)
            args.PlayerCount = int.Parse(numberOfPlayersInput.text);

        if (hostPrep != null)
            hostPrep.ConfigureStartGameArgs(ref args);

        StartGameResult resTask = await _runner.StartGame(args);

        if (resTask.Ok && hostPrep != null)
            hostPrep.AttachToRunner(_runner);

        return resTask;
    }

    
    
    public async Task StartSession()
    {
        ToggleButtonInteractivity(startSessionButton);
        
        StartGameResult resTask = await StartGameTutorial(GameMode.Host, newSessionNameInput.text);

        if (resTask.Ok)
        {
            TogglePanelVisibility(newSessionPanel);
            OnGameStarted(_runner);
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
        
        if(int.Parse(numberOfPlayersInput.text) > maximumPlayers)
        {
            DataValidationError(uiNotificationTexts.MaximalPlayerCountExceeded);
            return false;
        }
        
        if(int.Parse(numberOfPlayersInput.text) < minimumPlayers)
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
    
    
    public async void JoinSession(string sessionName)
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

        if (hostPrep != null)
        {
            hostPrep.AttachToRunner(_runner);
        }
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
    
    
    public void TogglePanelVisibility(GameObject panel)
    {
        if (panel)
        {
            if(panel == notificationPanel || panel == PlayerRegistrationPanel)
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
        
        if(_runner.IsServer)
            startGameButton.gameObject.SetActive(true);
    }

    public void AddPlayerToDictionaryOnNewSession()
    {
        if(!NewSessionDataValidation())
            return;
        
        NewSessionCreation = true;
        TogglePanelVisibility(PlayerRegistrationPanel);
    }
    
    public void AddPlayerToDictionaryOnExistingSession()
    {
        if(!NewSessionDataValidation())
            return;
        
        NewSessionCreation = false;
        TogglePanelVisibility(PlayerRegistrationPanel);
    }
    
    
    public async void AddPlayerDataToDictionary()
    {
        
        foreach (var players in playersDataDict)
        {
            if (players.Value.playerNickname == playerNicknameInput.text)
            {
                DataValidationError(uiNotificationTexts.NicknameAlreadyExists);
                return;
            }
            
            if(playerNicknameInput.text.IsNullOrEmpty())
            {
                DataValidationError(uiNotificationTexts.NicknameEmptyOrNull);
                return;
            }
        }
        
        PlayerData tempPlayerData = new PlayerData()
        {
            playerNickname = playerNicknameInput.text
        };
    
        TogglePanelVisibility(PlayerRegistrationPanel);

        if (NewSessionCreation)
        {
            await StartSession();  // Await to ensure runner is started
            NewSessionCreation = false;
        }

        if (_runner.IsServer)
            playersDataDict.Add(_runner.LocalPlayer, tempPlayerData);
        else 
            RPC_RequestAddPlayerData(tempPlayerData);
    
        UpdatePlayersList();
    }
    
        
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestAddPlayerData(PlayerData data, RpcInfo info = default)
    {
        if (_runner.IsServer)
            playersDataDict.Add(info.Source, data);
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
        
        if (_runner != null && _runner.IsRunning)
        {
            _runner.SessionInfo.IsOpen = false;
            sceneManager.OnlineMoveToScene(GAME_SCENE_NAME);
        }
        else
        {
            Debug.LogWarning("NetworkRunner is not running or not assigned.");
        }
    }
    
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        this.sessionList = sessionList;
        
        foreach (var textBox in playersTextBoxes)
            Destroy(textBox);

        playersTextBoxes.Clear();
        
        foreach (var session in existingSessionButtons) 
            Destroy(session.gameObject);
        
        existingSessionButtons.Clear();
        
        foreach (var session in sessionList)
        {
            if(!session.IsVisible)
                continue;
            
            Debug.Log(session.Name);
            GameObject newSessionButton = Instantiate(sessionButtonPrefab, SessionButtonLocations.transform);
            ButtonTextRefHolder newButton = newSessionButton.GetComponent<ButtonTextRefHolder>();

            newButton.SessionName = session.Name;
            newButton.buttonText.text = "Room name: " + session.Name + "\nPlayer Count: " + session.PlayerCount;
            
            newButton.AddPlayerToDictionary.AddListener(AddPlayerToDictionaryOnExistingSession);
            newButton.JoinGame.AddListener(JoinSession);

            existingSessionButtons.Add(newButton.thisButton);
        }
    }
    
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Joined: " + player);
        UpdatePlayersList();
    }

    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left: " + player);
        UpdatePlayersList();
    }

    
    public void UpdatePlayersList()
    {
        List<PlayerRef> players = _runner.ActivePlayers.ToList();
        
        for(int i = 0; i < playerNamesTexts.Length; ++i)
        {
            if(i >= players.Count())
            {
                playerNamesTexts[i].text = "";
            }
            else
            {
                playerNamesTexts[i].text = playersDataDict[players[i]].playerNickname.ToString();
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Runner Shut Down, Reason: " + shutdownReason);
    }
    
    
    #region TheShadowRealm
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
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
*///todo: delete!!!!!!!!!!!!!!!!!!!!!!!!!! ------------------------------
//-----------------------$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$--------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public const string GAME_SCENE_NAME = "EnderArena";
    
    const string EndersLobby = "Ender's";
    const string MoxieLobby  = "Moxie's";
    const string BillsLobby  = "Bill's";

    private string currentLobby;
    private bool NewSessionCreation = false;
    private List<SessionInfo> sessionList = new List<SessionInfo>();
    
    //public List<GameObject> playersTextBoxes = new List<GameObject>();
    
    public UnityEvent JoinLobbyEvent;

    [Header("Critical Dependencies")] 
    [SerializeField] private GameObject networkRunnerPrefab;
    [SerializeField] private NetworkRunner _runner;
    [SerializeField] private GameObject sessionButtonPrefab;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private UiNotificationTexts uiNotificationTexts;
    [SerializeField] private ClientHostPrepModule hostPrep;

    // NEW: prefab for PlayerData (NetworkObject + PlayerData)
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

    // buffer the name until we're actually in-session & PlayerData exists
    private string pendingNickname;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if (_runner != null)
            _runner.AddCallbacks(this);
    }
    
    async Task<StartGameResult> StartGameTutorial(GameMode mode, string SessionNameInput)
    {
        StartGameArgs args = new StartGameArgs()
        {
            GameMode       = mode,
            SessionName    = SessionNameInput,
            CustomLobbyName= _runner.LobbyInfo != null ? _runner.LobbyInfo.Name : null,
            IsVisible      = publicSessionToggle != null ? publicSessionToggle.isOn : true
        };

        if (mode == GameMode.Host)
            args.PlayerCount = int.Parse(numberOfPlayersInput.text);

        if (hostPrep != null)
            hostPrep.ConfigureStartGameArgs(ref args);

        StartGameResult resTask = await _runner.StartGame(args);

        if (resTask.Ok && hostPrep != null)
            hostPrep.AttachToRunner(_runner);

        return resTask;
    }

    public async Task StartSession()
    {
        ToggleButtonInteractivity(startSessionButton);
        
        StartGameResult resTask = await StartGameTutorial(GameMode.Host, newSessionNameInput.text);

        if (resTask.Ok)
        {
            TogglePanelVisibility(newSessionPanel);
            OnGameStarted(_runner);

            // try apply nickname right after host session starts
            TryApplyPendingNickname();
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
    
    public async void JoinSession(string sessionName)
    {
        ToggleButtonInteractivity(existingSessionButtons);
        
        StartGameResult resTask = await StartGameTutorial(GameMode.Client, sessionName);
        
        if (resTask.Ok)
        {
            OnGameStarted(_runner);
            // as a client, apply nickname after join succeeds
            TryApplyPendingNickname();
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

        if (hostPrep != null)
        {
            hostPrep.AttachToRunner(_runner);
        }
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
        
        if (_runner.IsServer)
            startGameButton.gameObject.SetActive(true);
    }

    public void AddPlayerToDictionaryOnNewSession()
    {
        if (!NewSessionDataValidation())
            return;
        
        NewSessionCreation = true;
        TogglePanelVisibility(PlayerRegistrationPanel);
    }
    
    public void AddPlayerToDictionaryOnExistingSession()
    {
        // keep same UX: open the name panel before joining
        NewSessionCreation = false;
        TogglePanelVisibility(PlayerRegistrationPanel);
    }
    
    // renamed logic, same UX: store name now; apply it once we're in-session
    public async void AddPlayerDataToDictionary()
    {
        string nick = playerNicknameInput != null ? playerNicknameInput.text?.Trim() : null;

        if (string.IsNullOrEmpty(nick))
        {
            DataValidationError(uiNotificationTexts.NicknameEmptyOrNull);
            return;
        }

        pendingNickname = nick;

        TogglePanelVisibility(PlayerRegistrationPanel);

        if (NewSessionCreation)
        {
            await StartSession();  // ensures runner is started; host joins first
            NewSessionCreation = false;
        }

        // If we're already in-session (e.g., host path), try immediately
        TryApplyPendingNickname();
        UpdatePlayersList();
    }
    
    private void TryApplyPendingNickname()
    {
        if (_runner == null || string.IsNullOrEmpty(pendingNickname))
            return;

        var data = PlayerData.Local(_runner);
        if (data != null)
        {
            // Default team/colorIndex if you don't have pickers here; adjust later if needed
            PlayerData.SetLocal(_runner, pendingNickname, 0, 0);
            pendingNickname = null;
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
        
        if (_runner != null && _runner.IsRunning)
        {
            _runner.SessionInfo.IsOpen = false;
            _runner.GetComponent<RunnerSimulatePhysics3D>().ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;
            _runner.ProvideInput = true;
            sceneManager.OnlineMoveToScene(GAME_SCENE_NAME);
        }
        else
        {
            Debug.LogWarning("NetworkRunner is not running or not assigned.");
        }
    }
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        this.sessionList = sessionList;
        
        //todo: remove?
        /*foreach (var textBox in playersTextBoxes)
            Destroy(textBox);

        playersTextBoxes.Clear();*/
        
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
            
            newButton.AddPlayerToDictionary.AddListener(AddPlayerToDictionaryOnExistingSession);
            newButton.JoinGame.AddListener(JoinSession);

            existingSessionButtons.Add(newButton.thisButton);
        }
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Joined: " + player);

        // Host spawns & binds PlayerData for everyone (including host self on first join)
        if (runner.IsServer && playerDataPrefab != null)
        {
            if (!runner.GetPlayerObject(player))
            {
                var dataObj = runner.Spawn(playerDataPrefab, Vector3.zero, Quaternion.identity, player);
                runner.SetPlayerObject(player, dataObj);
            }
        }

        // If this is us and we had a name pending, apply it now
        if (player == runner.LocalPlayer)
            TryApplyPendingNickname();

        UpdatePlayersList();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left: " + player);

        // Host cleans up that player's PlayerData (and anything else if you want)
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

        List<PlayerRef> players = _runner.ActivePlayers.ToList();
        
        for (int i = 0; i < playerNamesTexts.Length; ++i)
        {
            if (i >= players.Count)
            {
                playerNamesTexts[i].text = "";
            }
            else
            {
                var pd = PlayerData.Get(_runner, players[i]);
                playerNamesTexts[i].text = (pd != null && !pd.Nickname.Value.IsNullOrEmpty()) ? pd.Nickname.ToString() : "Joining...";
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
