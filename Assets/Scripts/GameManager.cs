using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private NetworkRunner nr;
    public GameObject playerPrefab;
    public Transform spawnPoint;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nr = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        nr.Spawn(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
