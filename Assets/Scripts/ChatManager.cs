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
    
    [SerializeField] public int maxChatMessages = 7; // Maximum number of chat messages to keep in the queue
    //[SerializeField] private GameObject chatMessagePrefab;
    //[SerializeField] private GameObject chatGrid;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Dropdown targetPlayersDropdown;

    [SerializeField] private TMP_Text[] chatTxtArr;
    
    private NetworkRunner runner;
    
    Queue<string> chatQueue = new Queue<string>();
    
    List<PlayerData> playerDataList = new List<PlayerData>();//---------------------------------TODO
    

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
            Debug.Log("NetworkRunner initialized successfully.");
        
        runner.AddCallbacks(this);

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
    private void RPCWhisper([RpcTarget] PlayerRef targetPlayer, string messageInfo, RpcInfo info = default)//, int playerColorIndex)
    {
        if (runner.LocalPlayer.PlayerId == targetPlayer.PlayerId || runner.LocalPlayer.PlayerId == info.Source.PlayerId)
            AddChatMessage(messageInfo);
    }
    
    // [Rpc]
    // private void RPCWhisper([RpcTarget] PlayerRef recipient, string senderName, string message, RpcInfo info = default)
    // {
    //     if (runner.LocalPlayer == recipient || runner.LocalPlayer == info.Source)
    //     {
    //         AddChatMessage($"[Whisper] {senderName}: {message}");
    //     }
    // }


    [Rpc]
    private void RPCMessageAll(string messageInfo, RpcInfo info = default)//, int playerColorIndex)
    {
        Debug.Log("Inside RPCMessageAll");
        AddChatMessage(messageInfo);
    }
    
    
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        FillTargetPlayerList();
        Debug.Log("Special Player Joined: " + player + "aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }

    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        FillTargetPlayerList();
        Debug.Log(" Special Player Left: " + player+ "aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
    
    
    private void UpdateMessages()
    {
        for(int i = chatTxtArr.Length - 1; i >= 0; --i)
        {
            if(chatQueue.Count > i)
            {
                Debug.Log(i + chatQueue.ElementAt(i));
                chatTxtArr[i].text =  chatQueue.ElementAt(chatQueue.Count - 1 - i);
            }
        }
    }

    public void ActuallySendChat(string text)
    {
        string selected = targetPlayersDropdown.options[targetPlayersDropdown.value].text;

        if (selected == ALL_STR)
        {
            RPCMessageAll(text);
        }
        else
        {
            PlayerRef target = runner.ActivePlayers.FirstOrDefault(p => p.ToString() == selected);
            RPCWhisper(target, $"[Whisper] {runner.LocalPlayer}: {text}");
        }
    }
    
    private void AddChatMessage(string messageText)
    {
        Debug.Log("Inside AddChatMessage");

        chatQueue.Enqueue(messageText);
        
        if (chatQueue.Count > maxChatMessages)
        {
            chatQueue.Dequeue();
        }
        
        UpdateMessages();
    }
    

    private void FillTargetPlayerList()
    {
        targetPlayersDropdown.ClearOptions();//Empty The Dropdown
        
        List<PlayerRef> players = runner.ActivePlayers.ToList();
        List<string> playerNames = new List<string>();
        
        playerNames.Add(ALL_STR);
        
        foreach (PlayerRef player in players)
            if(player.PlayerId != runner.LocalPlayer.PlayerId)
                playerNames.Add(player.ToString());

        targetPlayersDropdown.AddOptions(playerNames);
    }
    

    #region UnusedCallbacks
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
    
    #endregion
}
