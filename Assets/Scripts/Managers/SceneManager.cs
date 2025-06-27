using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public NetworkRunner runner;

    void Start()
    {
        if(runner == null)
            runner = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        if (!runner.IsRunning)
        {
            Debug.LogWarning("NetworkRunner or GameManager not initialized. Cannot call RPC.");
            return;
        }
        else
            Debug.Log("NetworkRunner initialized successfully.");
    }

    public void OnlineMoveToScene(string sceneName)
    {
        runner.LoadScene(sceneName);
    }
    
    public void OfflineMoveToScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
