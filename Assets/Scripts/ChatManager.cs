using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class ChatManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    private const string ALL_STR = "All";

    [SerializeField] public int maxChatMessages = 7;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Dropdown targetPlayersDropdown;
    [SerializeField] private TMP_Text[] chatTxtArr;

    private NetworkRunner runner;
    private Queue<string> chatQueue = new Queue<string>();


    void Start()
    {
        runner = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        if (!runner.IsRunning)
        {
            Debug.LogWarning("NetworkRunner not initialized.");
            return;
        }

        Debug.Log("NetworkRunner initialized.");
        FillTargetPlayerList();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!string.IsNullOrWhiteSpace(inputField.text))
            {
                ActuallySendChat(inputField.text.Trim());
                inputField.text = "";
            }
        }
    }

    [Rpc]
    private void RPCWhisper([RpcTarget] PlayerRef recipient, string senderName, string message, RpcInfo info = default)
    {
        if (runner.LocalPlayer == recipient || runner.LocalPlayer == info.Source)
            AddChatMessage($"[Whisper] {senderName}: {message}");
    }

    [Rpc]
    private void RPCMessageAll(string senderName, string message, RpcInfo info = default)
    {
        AddChatMessage($"{senderName}: {message}");
    }

    public void ActuallySendChat(string text)
    {
        string selectedOption = targetPlayersDropdown.options[targetPlayersDropdown.value].text;
        string senderName = $"Player {runner.LocalPlayer.PlayerId}";

        if (selectedOption == ALL_STR)
        {
            RPCMessageAll(senderName, text);
        }
        else if (int.TryParse(selectedOption.Replace("Player ", ""), out int targetId))
        {
            PlayerRef target = runner.ActivePlayers.FirstOrDefault(p => p.PlayerId == targetId);
            
            if (target != PlayerRef.None)
                RPCWhisper(target, senderName, text);
        }
    }

    private void AddChatMessage(string messageText)
    {
        chatQueue.Enqueue(messageText);

        if (chatQueue.Count > maxChatMessages)
        {
            chatQueue.Dequeue();
        }

        UpdateMessages();
    }

    private void UpdateMessages()
    {
        for (int i = 0; i < chatTxtArr.Length; i++)
            chatTxtArr[i].text = (i < chatQueue.Count) ? chatQueue.ElementAt(i) : "";
    }

    private void FillTargetPlayerList()
    {
        if (!targetPlayersDropdown) 
            return;

        targetPlayersDropdown.ClearOptions();

        List<string> playerOptions = new List<string> { ALL_STR };

        foreach (PlayerRef player in runner.ActivePlayers.ToList())
            if (player != runner.LocalPlayer)
                playerOptions.Add($"Player {player.PlayerId}");

        targetPlayersDropdown.AddOptions(playerOptions);
        targetPlayersDropdown.value = 0;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        FillTargetPlayerList();

        foreach (var VARIABLE in runner.ActivePlayers)
        {
            Debug.Log(VARIABLE.PlayerId);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        FillTargetPlayerList();
    }

    #region UnusedCallbacks
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    #endregion
}
