using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : NetworkBehaviour
{
    private NetworkRunner nr;
    
    //public Transform playerSpawnPoint;
    public SpawnPoint[] tenPlayerSpawnPoints;
    
    public GameObject[] characterColoredPrefabs = new GameObject[10];
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nr = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        //rn = FindFirstObjectByType<NetworkRunner>();
 
        if (!nr.IsRunning)
        {
            Debug.LogWarning("NetworkRunner or GameManager not initialized. Cannot call RPC.");
            return;
        }
        else
        {
            Debug.Log("NetworkRunner initialized successfully.");
        }
    }

    // public void SpawnPlayer(GameObject coloredPlayerPrefab)
    // {
    //     SpawnPoint targetSpawnPoint;
    //     do
    //     {
    //         targetSpawnPoint = tenPlayerSpawnPoints[Random.Range(0, tenPlayerSpawnPoints.Length)];
    //     } while (targetSpawnPoint.isTaken);
    //     
    //     targetSpawnPoint.isTaken = true;
    //     _runner.SpawnAsync(coloredPlayerPrefab, targetSpawnPoint.transform.position, targetSpawnPoint.transform.rotation);
    // }
    
    public void CallRpc(int playerColorIndex)
    {
        Debug.Log("Calling RPCRequestSpawnPointRpc");
        RPCRequestSpawnPointRpc(playerColorIndex);
    }

    
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCRequestSpawnPointRpc(int playerColorIndex, RpcInfo info = default)
    {
        Debug.Log("helloooo");
        
        int spawnSpawnIndex = 0;
        SpawnPoint targetSpawnPoint;
        
        do
        {
            spawnSpawnIndex = Random.Range(0, tenPlayerSpawnPoints.Length);
            targetSpawnPoint = tenPlayerSpawnPoints[spawnSpawnIndex];
        } while (targetSpawnPoint.isTaken);
    
        targetSpawnPoint.isTaken = true;
        Debug.Log("byeee");
        RPCSetSpawnPoint(info.Source, spawnSpawnIndex, playerColorIndex);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCSetSpawnPoint([RpcTarget] PlayerRef targetPlayer, int spawnPointIndex, int playerColorIndex)
    {
        Debug.Log("RPCSetSpawnPoint");
        SpawnPoint targetSpawnPoint = tenPlayerSpawnPoints[spawnPointIndex];
        
        targetSpawnPoint.isTaken = true;
        nr.SpawnAsync(characterColoredPrefabs[playerColorIndex], targetSpawnPoint.transform.position, targetSpawnPoint.transform.rotation);
    }
}
