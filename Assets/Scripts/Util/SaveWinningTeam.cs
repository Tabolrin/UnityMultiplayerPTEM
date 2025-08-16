using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class SaveWinningTeam : MonoBehaviour
{
    static public SaveWinningTeam Instance;
    public byte WinningTeam;
    public Dictionary<PlayerRef, int> PlayerScores = new Dictionary<PlayerRef, int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }
}
