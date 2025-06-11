using System;
using TMPro;
using UnityEngine;

public class ChatMessageHandler : MonoBehaviour
{
    [SerializeField] public TMP_Text text;

    private void Awake()
    {
        Destroy(this, 5);
    }
}
