using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private int maxChatMessages = 8; // Maximum number of chat messages to keep in the queue
    
    Queue<TMP_Text> chatQueue = new Queue<TMP_Text>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
