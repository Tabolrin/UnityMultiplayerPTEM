using Fusion;
using TMPro;
using UnityEngine;

public class EndScreenUIManager : MonoBehaviour
{
    private NetworkRunner _runner;
    [SerializeField] private TMP_Text personalScoreText;
    [SerializeField] private TMP_Text winLoseText;

    private void Awake()
    {
        _runner = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    void Start()
    {
        if(SaveWinningTeam.Instance.WinningTeam == PlayerData.Get(_runner, _runner.LocalPlayer).Team)
            winLoseText.text = "Your Team Won!";
        else
            winLoseText.text = "Your Team Lost!";
        
        personalScoreText.text = "Personal Score: " + SaveWinningTeam.Instance.PlayerScores[_runner.LocalPlayer];
    }
}
