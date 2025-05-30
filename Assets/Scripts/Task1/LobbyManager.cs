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


public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    const string catsLobby = "Cats";
    const string puppiesLobby = "Puppies";
    const string capysLobby = "Capybara's";

    private string currentLobby;
    
    List<GameObject> newSessionButtons = new List<GameObject>();
    List<GameObject> playersTextBoxes = new List<GameObject>();
   
    
    [Header("Critical Dependencies")]
    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] private GameObject sessionButtonPrefab;
    [SerializeField] private GameObject playerNamePrefab;
    [SerializeField] private GameObject playerNameTextPrefab;
    
    [Header("Panels")]
    [SerializeField] private GameObject sessionListPanel;
    [SerializeField] private GameObject Lobbies;
    [SerializeField] private GameObject MidSessionPanel;
    [SerializeField] private GameObject NewSessionButtonLocations;
    [SerializeField] private GameObject playerNamesListPanel;
    
    [Header("Player Id's")]
    [SerializeField] private TMP_Text[] playerNamesTexts;
    
    [Header("Player Id's")]
    [SerializeField] private Button startSessionButton;
    [SerializeField] private Button endSessionButton;
    
    [Header("Player Id's")]
    [SerializeField] private TMP_InputField newSessionNameInput;
    [SerializeField] private TMP_InputField numberOfPlayersInput;
    


    private void Awake()
    {
        networkRunner.AddCallbacks(this);
    }
    
    public async void StartSession()
    {
        if (string.IsNullOrEmpty(newSessionNameInput.text) || string.IsNullOrEmpty(numberOfPlayersInput.text))
            return;
        
        StartGameResult resTask = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = newSessionNameInput.text,
            PlayerCount = int.Parse(numberOfPlayersInput.text),
            CustomLobbyName = networkRunner.LobbyInfo.Name,
        });

        if (resTask.Ok)
            OnGameStarted(networkRunner);
        else
        {
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
        }
    }
    
    public async void JoinSession(TMP_Text sessionName)
    {
        StartGameResult resTask = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName.text,
            CustomLobbyName = networkRunner.LobbyInfo.Name,
        });

        if (resTask.Ok)
            OnGameStarted(networkRunner);
        else
        {
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
        }
    }
    
    public async void JoinLobby(string lobbyName)
    {
        StartGameResult result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyName);
        
        if (result.Ok)
        {
            Debug.Log("Joined Lobby!" + lobbyName);
            currentLobby = lobbyName;
        }
        else
        {
            Debug.LogError($"Game join failed: {result.ShutdownReason}");
        }
    }
    
    public void CheckCurrentLobby()
    {
        if (networkRunner != null && networkRunner.IsRunning)
        {
            string currentLobby = networkRunner.LobbyInfo.Name;
            
            if (!string.IsNullOrEmpty(currentLobby))
            {
                Debug.Log($"Currently in lobby: {currentLobby}");
            }
            else
            {
                Debug.Log("Not in any lobby.");
            }
        }
        else
        {
            Debug.LogWarning("NetworkRunner is not running or not assigned.");
        }
    }
    
    public void TogglePanelVisibility(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
        else
        {
            Debug.LogWarning("Panel is not assigned.");
        }
    }

    private void OnGameStarted(NetworkRunner obj)
    {
        Debug.Log("Game Started");
        sessionListPanel.SetActive(false);
        MidSessionPanel.SetActive(true);
    }
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("OnSessionListUpdated");
        
        foreach (var textBox in playersTextBoxes)
            Destroy(textBox);
        
        newSessionButtons.Clear();
        
        foreach (var session in sessionList)
        {
            GameObject newSessionButton = Instantiate(sessionButtonPrefab, NewSessionButtonLocations.transform);
            ButtonTextRefHolder newButton = newSessionButton.GetComponent<ButtonTextRefHolder>();
            
            newButton.buttonText.text = session.Name;
            newButton.onButtonClick.AddListener(JoinSession);
            
            newSessionButtons.Add(newSessionButton);
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
