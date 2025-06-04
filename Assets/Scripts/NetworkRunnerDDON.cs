using System;
using UnityEngine;

public class NetworkRunnerDDON : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
