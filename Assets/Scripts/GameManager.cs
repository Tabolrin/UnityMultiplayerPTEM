using Fusion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    private NetworkRunner runner;
    
    //public Transform playerSpawnPoint;
    public SpawnPoint[] tenPlayerSpawnPoints;
    
    public GameObject[] characterColoredPrefabs = new GameObject[10];
    
    public Button[] CharacterSelectButton = new Button[10];

    [SerializeField] private GameObject CharacterSelectPanel;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        runner = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
 
        if (!runner.IsRunning)
        {
            Debug.LogWarning("NetworkRunner or GameManager not initialized. Cannot call RPC.");
            return;
        }
        else
        {
            Debug.Log("NetworkRunner initialized successfully.");
        }
    }


    
    public void CallRpc(int playerColorIndex)
    {
        Debug.Log("Calling RPCRequestSpawnPointRpc");
        RPCRequestSpawnPointRpc(playerColorIndex);
        RPCToggleButtonRpc(playerColorIndex);
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
        runner.SpawnAsync(characterColoredPrefabs[playerColorIndex], targetSpawnPoint.transform.position, targetSpawnPoint.transform.rotation);
        
        CharacterSelectPanel.SetActive(false);
    }

    [Rpc]
    private void RPCToggleButtonRpc(int playerColorIndex)
    {
        CharacterSelectButton[playerColorIndex].interactable = false;
    }
    
    
}
