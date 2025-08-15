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
    public UnityEvent<string> OnSessionButton;

    private void Awake()
    {
        lobbyManager = FindFirstObjectByType<LobbyManager>();
    }

    public void InvokeOnSessionButton()
    {
        OnSessionButton.Invoke(SessionName);
    }
}
