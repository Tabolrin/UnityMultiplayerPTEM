using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameCallBackMaster : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] InputManager inputManager;
    [SerializeField] SpawnerDebug spawnerDebug;
    [SerializeField] SceneManager sceneManager;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        spawnerDebug.SetRunner(runner);
        spawnerDebug.OnPlayerJoined(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        spawnerDebug.OnPlayerLeft(runner, player);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        inputManager.OnInput(runner, input);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Game ended - shutdown reason: {shutdownReason}");
        sceneManager.OfflineMoveToScene("Lobby");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected: {reason}");
        sceneManager.OfflineMoveToScene("Lobby");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"Connection failed: {reason}");
        sceneManager.OfflineMoveToScene("Lobby");
    }

    #region TheShdowRealm3.0

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }


    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    #endregion
}
