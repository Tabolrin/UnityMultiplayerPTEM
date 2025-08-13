using Fusion;
using Fusion.Addons.Physics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnerDebug : MonoBehaviour
{
    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private SceneManager _sceneManager;
    [SerializeField] private string sessionName;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void SetRunner(NetworkRunner runner)
    {
        _runner = runner;
    }

    async void StartGame(GameMode mode)
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            var runnerSimulatePhysics3D = gameObject.AddComponent<RunnerSimulatePhysics3D>();
            runnerSimulatePhysics3D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;
        }

        _runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
            NetworkObject networkPlayerObj = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkPlayerObj);
            Debug.Log($"Player {player} joined and spawned at {spawnPosition}");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    [ContextMenu("End Game")]
    public void EndGame()
    {
        if (_sceneManager == null)
        {
            Debug.LogWarning("SceneManager reference not set on SpawnerDebug!");
            return;
        }

        _sceneManager.runner = _runner;

        if (_runner != null && _runner.IsServer)
        {
            _sceneManager.OnlineMoveToScene("Lobby");
        }
        else
        {
            _sceneManager.OfflineMoveToScene("Lobby");
        }
    }
}
