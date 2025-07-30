using System;
using TMPro;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    [SerializeField] public TMP_Text text;

    private void Awake()
    {
        Destroy(this, 5);
    }
}
