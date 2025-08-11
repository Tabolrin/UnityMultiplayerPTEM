using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonTextRefHolder : MonoBehaviour
{
    public string SessionName;
    public TMP_Text buttonText;
    public Button thisButton;
    
    private LobbyManager lobbyManager;
    public UnityEvent AddPlayerToDictionary;
    public UnityEvent<string> JoinGame;

    private void Awake()
    {
        lobbyManager = FindFirstObjectByType<LobbyManager>();
        lobbyManager.JoinLobbyEvent.AddListener(InvokeJoinGame);
    }

    public void InvokeAddPlayerToDictionary()
    {
        AddPlayerToDictionary.Invoke();
    }

    public void InvokeJoinGame()
    {
        JoinGame.Invoke(SessionName);
    }
}
