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


public struct PlayerData
{
    public int playerId; 
    public string playerNickname;
    public Color playerColor;
}

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
    //[SerializeField] private GameObject playerNamesListPanel;
    //[SerializeField] private GameObject lockedSessionPanel;
    
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
    
    [Header("New session restriction settings")]
    [SerializeField] private int minimumPlayers = 4;
    [SerializeField] private int maximumPlayers = 8;

    [Networked] private NetworkDictionary<PlayerRef, PlayerData> playersDataDict => default;
    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _runner.AddCallbacks(this);
        
        //Instance = this;
    }
    
    
    async Task<StartGameResult> StartGameTutorial(GameMode mode, string SessionNameInput)
    {
        //RunnerNullCheck(); TODO: delete if unneeded
        
        StartGameResult resTask;
        
        if (mode == GameMode.Host)
        {
            resTask = await _runner.StartGame(new StartGameArgs()
           {
               GameMode = mode,
               SessionName = SessionNameInput,
               PlayerCount = int.Parse(numberOfPlayersInput.text),
               CustomLobbyName = _runner.LobbyInfo.Name,
               IsVisible = publicSessionToggle.isOn
           }); 
        }
        else
        {
            resTask = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = SessionNameInput,
                CustomLobbyName = _runner.LobbyInfo.Name,
                IsVisible = publicSessionToggle.isOn
            });
        }

        return resTask;
    }
    
    
    public async void StartSession()
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
    
    public void AddPlayerDataToDictionary()
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
            
            if(players.Key.PlayerId == _runner.LocalPlayer.PlayerId)
            {
                DataValidationError(uiNotificationTexts.IdAlreadyExists);
                return;
            }
        }
        
        PlayerData tempPlayerData = new PlayerData()
        {
            playerId = _runner.LocalPlayer.PlayerId,
            playerNickname = playerNicknameInput.text
        };
        
        TogglePanelVisibility(PlayerRegistrationPanel);

        if (NewSessionCreation)
        {
            StartSession();
            NewSessionCreation = false;
        }

        if (_runner.IsServer)
            playersDataDict.Add(_runner.LocalPlayer, tempPlayerData);
        else 
            RPC_RequestAddPlayerData(tempPlayerData);
        
        UpdatePlayersList();
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestAddPlayerData(PlayerData data)
    {
        if (_runner.IsServer)
            playersDataDict.Add(_runner.LocalPlayer, data);
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
                playerNamesTexts[i].text = playersDataDict[players[i]].playerNickname;
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Runner Shut Down, Reason: " + shutdownReason);
    }
    
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }
    
    
    
    #region UnusedCallbacks
    
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



    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
    #endregion
}
