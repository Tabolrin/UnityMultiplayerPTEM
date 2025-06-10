using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public NetworkRunner runner;
    public void MoveToScene(string sceneName)
    {
        Debug.Log(sceneName);
        runner.LoadScene(sceneName);
    }
}
