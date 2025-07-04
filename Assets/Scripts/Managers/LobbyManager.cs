using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Multiplayer;
using Unity.VisualScripting;


struct PlayerData
{
    public string playerId; 
    public string playerName;
    public Color playerColor;
}

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public const string GAME_SCENE_NAME = "Game";
    
    //Relevant Instances
    public static LobbyManager Instance;
    public static ReadyManager readyManagerInstance;
    
    const string catsLobby = "Cats";
    const string puppiesLobby = "Puppies";
    const string capysLobby = "Capybara's";

    private string currentLobby;
    
    List<GameObject> playersTextBoxes = new List<GameObject>();


    [Header("Critical Dependencies")] 
    [SerializeField] private GameObject networkRunnerPrefab;
    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] private GameObject sessionButtonPrefab;
    [SerializeField] private GameObject playerNamePrefab;
    [SerializeField] private GameObject playerNameTextPrefab;
    [SerializeField] private SceneManager sceneManager;
    
    [Header("Panels")]
    [SerializeField] private GameObject sessionListPanel;
    [SerializeField] private GameObject Lobbies;
    [SerializeField] private GameObject MidSessionPanel;
    [SerializeField] private GameObject SessionButtonLocations;
    [SerializeField] private GameObject newSessionPanel;
    [SerializeField] private GameObject lockedSessionPanel;
    //[SerializeField] private GameObject playerNamesListPanel;

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
    


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        networkRunner.AddCallbacks(this);
        Instance = this;
    }
    
    public async void StartSession()
    {
        if (string.IsNullOrEmpty(newSessionNameInput.text) || string.IsNullOrEmpty(numberOfPlayersInput.text))
            return;

        ToggleButtonInteractivity(startSessionButton);
        
        StartGameResult resTask = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = newSessionNameInput.text,
            PlayerCount = int.Parse(numberOfPlayersInput.text),
            CustomLobbyName = networkRunner.LobbyInfo.Name,
            IsVisible = publicSessionToggle.isOn
        });

        if (resTask.Ok)
        {
            TogglePanelVisibility(newSessionPanel);
            OnGameStarted(networkRunner);
        }
        else
        {
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
            ToggleButtonInteractivity(startSessionButton);
        }
    }
    
    public async void JoinSession(TMP_Text sessionName)
    {
        ToggleButtonInteractivity(existingSessionButtons);
        
        StartGameResult resTask = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName.text,
            CustomLobbyName = networkRunner.LobbyInfo.Name,
        });

        if (resTask.Ok)
        {
            OnGameStarted(networkRunner);
        }
        else
        {
            if (resTask.ShutdownReason == ShutdownReason.GameClosed)
                lockedSessionPanel.SetActive(true);

            ResetNetworkRunner();
            
            ToggleButtonInteractivity(existingSessionButtons);
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
        }
    }

    private void ResetNetworkRunner()
    {
        networkRunner = Instantiate(networkRunnerPrefab).GetComponent<NetworkRunner>();
    }
    
    public async void JoinLobby(string lobbyName)
    {
        StartGameResult result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyName);
        
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
        
        if(networkRunner.IsSharedModeMasterClient)
            startGameButton.gameObject.SetActive(true);
    }
    
    
    public void MoveToGameScene()
    {
        if (networkRunner != null && networkRunner.IsRunning)
        {
            networkRunner.SessionInfo.IsOpen = false;
            sceneManager.OnlineMoveToScene(GAME_SCENE_NAME);
        }
        else
        {
            Debug.LogWarning("NetworkRunner is not running or not assigned.");
        }
    }
    
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
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
            
            newButton.buttonText.text = session.Name;
            newButton.onButtonClick.AddListener(JoinSession);

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
        List<PlayerRef> players = networkRunner.ActivePlayers.ToList();
        
        for(int i = 0; i < playerNamesTexts.Length; ++i)
        {
            if(i >= players.Count())
            {
                playerNamesTexts[i].text = "";
            }
            else
            {
                playerNamesTexts[i].text = players[i].PlayerId.ToString();
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Runner Shut Down, Reason: " + shutdownReason);
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
