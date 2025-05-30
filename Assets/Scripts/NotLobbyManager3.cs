using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Multiplayer;


public class LobbyManager3 : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] NetworkRunner networkRunner;

    [Header("UI References")] [SerializeField]
    private GameObject sessionPanel;

    [SerializeField] private Button startSessionButton;
    [SerializeField] private Button endSessionButton;
    [SerializeField] private TextMeshProUGUI numberOfPlayersText;
    [SerializeField] GameObject GenericReadyManager ;

    private void Start()
    {
        networkRunner.AddCallbacks(this);
        
#if LOBBY_MANAGER_UI
        endSessionButton.interactable = false;
#endif
    }

    public void StartSession()
    {
        networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "OurGameID",
            OnGameStarted = OnGameStarted
            
        });

#if LOBBY_MANAGER_UI
        startSessionButton.interactable = false;
        #endif
    }

    private void OnGameStarted(NetworkRunner obj)
    {
        Debug.Log("Game Started");
        if (networkRunner.IsSharedModeMasterClient)
        {
            networkRunner.Spawn(GenericReadyManager);
            Debug.Log("Game BOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
        }
#if LOBBY_MANAGER_UI
        endSessionButton.interactable = true;
#endif
    }
    
    public void EndSession()
    {
        if (networkRunner.IsRunning)
        {
            networkRunner.Shutdown();
        }
#if LOBBY_MANAGER_UI
        startSessionButton.interactable = true;
        endSessionButton.interactable = false;
#endif
    }

    public async void JoinLobby()
    {
       StartGameResult result = 
           await networkRunner.JoinSessionLobby(SessionLobby.Custom, "MainLobby");
     
       if (result.Ok)
       {
           Debug.Log("Joined Lobby!");
       }
    }

    [ContextMenu( "Join Lobby Test")]
    public void LeaveLobbyTest()
    {
        networkRunner.JoinSessionLobby(SessionLobby.Shared);
    }

    private void RefreshRoomUI()
    {
#if LOBBY_MANAGER_UI
        if (networkRunner.IsRunning && !networkRunner.IsShutdown)
        { 
            sessionPanel.SetActive(true);
            numberOfPlayersText.text = networkRunner.SessionInfo?.PlayerCount.ToString();
        }
        else
        {
            sessionPanel.SetActive(false);
        }
#endif
    }

    #region RunnerCallBacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        bool isLocalPlayer = false;

        if (networkRunner.LocalPlayer == player)
            isLocalPlayer = true;

        Debug.Log($"Player {player.PlayerId} joined, localPlayer: {isLocalPlayer}");

        RefreshRoomUI();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left");
        RefreshRoomUI();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        RefreshRoomUI();

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
        Debug.Log("Connected to server and lobby successfully!");

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"Session list updated. Found {sessionList.Count} sessions.");
        foreach (var session in sessionList)
        {
            Debug.Log($"Session Name: {session.Name}, Player Count: {session.PlayerCount}");
        }
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


    // public async void StartSession()
    // {
    //    StartGameResult resTask = await networkRunner.StartGame(new StartGameArgs()
    //    {
    //       GameMode = GameMode.Shared,
    //       SessionName = "OurGameID",
    //       OnGameStarted = OnGameStarted
    //    });
    //
    //    if (resTask.Ok)
    //    {
    //       OnGameStarted(networkRunner);
    //    }
    //    else
    //    {
    //       Debug.LogError($"Game failed to start because {resTask.ErrorMessage}");
    //    }
    // }
    //
    // private void OnGameStarted(NetworkRunner obj)
    // {
    //    Debug.Log("Game Started");;
    // }
}