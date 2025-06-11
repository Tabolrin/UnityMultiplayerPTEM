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
    
    [SerializeField] public int maxChatMessages = 8; // Maximum number of chat messages to keep in the queue
    //[SerializeField] private GameObject chatMessagePrefab;
    //[SerializeField] private GameObject chatGrid;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Dropdown targetPlayersDropdown;

    [SerializeField] private TMP_Text[] chatMessages;
    
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

        FillTargetPlayerList();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ActuallySendChat(inputField.text);
            inputField.text = "";
        }
    }

    
    [Rpc]
    private void RPCWhisper([RpcTarget] PlayerRef targetPlayer, string messageInfo, RpcInfo info = default)//, int playerColorIndex)
    {
        if (runner.LocalPlayer.PlayerId == targetPlayer.PlayerId)
            AddChatMessage(messageInfo);
    }


    [Rpc]
    private void RPCMessageAll(string messageInfo, RpcInfo info = default)//, int playerColorIndex)
    {
        Debug.Log("Inside RPCMessageAll");
        AddChatMessage(messageInfo);
    }
    
    
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        FillTargetPlayerList();
    }

    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        FillTargetPlayerList();
    }
    
    
    private void UpdateMessages()
    {
        for(int i = chatMessages.Length - 1; i >= 0; --i)
        {
            if(chatQueue.Count > i)
            {
                chatMessages[i].text =  chatQueue.ElementAt(i);
            }
        }
    }

    public void ActuallySendChat(string text)
    {
        //if (targetPlayersDropdown.value.ToString() == ALL_STR)
            RPCMessageAll(text);
        //else
           // RPCWhisper(targetPlayersDropdown.value.ToString())
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
