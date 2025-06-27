using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;


public class ChatManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    private const string ALL_STR = "All";
    
    [SerializeField] public int maxChatMessages = 7; 
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
        AddChatMessage(messageInfo);
    }
    


    [Rpc]
    private void RPCMessageAll(string messageInfo, RpcInfo info = default)//, int playerColorIndex)
    {
        AddChatMessage(info.Source.PlayerId + ": " + messageInfo);
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
            RPCWhisper(target, $"/W {runner.LocalPlayer}: {text}");
            AddChatMessage($"/W to {selected}: {text}");
        }
    }

    
    private void AddChatMessage(string messageText)
    {
        chatQueue.Enqueue(messageText);
        
        if (chatQueue.Count > maxChatMessages)
            chatQueue.Dequeue();
        
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
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
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

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
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
