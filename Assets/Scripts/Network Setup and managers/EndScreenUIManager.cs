using System;
using Fusion;
using TMPro;
using UnityEngine;

public class EndScreenUIManager : MonoBehaviour
{
    private NetworkRunner _runner;
    [SerializeField] private TMP_Text personalScoreText;

    private void Awake()
    {
        _runner = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    void Start()
    {
        personalScoreText.text = "Personal Score: " + SaveWinningTeam.Instance.PlayerScores[_runner.LocalPlayer];
        Debug.Log("Next scene has " + SaveWinningTeam.Instance.WinningTeam);
    }
}
