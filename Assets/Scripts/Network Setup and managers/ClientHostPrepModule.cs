using System;
using System.Collections;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class ClientHostPrepModule : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Snapshot Settings")]
    [Tooltip("Interval between Host Migration snapshots (minutes).")]
    [SerializeField] private float snapshotIntervalMinutes = 1f;

    private NetworkRunner _runner;
    private Coroutine _snapshotRoutine;

    private static ClientHostPrepModule _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ConfigureStartGameArgs(ref StartGameArgs args)
    {
        args.ConnectionToken = SessionTokenUtil.GetConnectionTokenBytes();
    }

    public void AttachToRunner(NetworkRunner runner)
    {
        if (_runner == runner)
        {
            return;
        }

        if (_runner != null)
        {
            _runner.RemoveCallbacks(this);
        }

        _runner = runner;

        if (_runner != null)
        {
            _runner.AddCallbacks(this);
            RestartSnapshotRoutine();
        }
    }

    private void RestartSnapshotRoutine()
    {
        if (_snapshotRoutine != null)
        {
            StopCoroutine(_snapshotRoutine);
            _snapshotRoutine = null;
        }

        if (snapshotIntervalMinutes > 0f && isActiveAndEnabled)
        {
            _snapshotRoutine = StartCoroutine(SnapshotLoop());
        }
    }

    private IEnumerator SnapshotLoop()
    {
        var wait = new WaitForSeconds(snapshotIntervalMinutes * 60f);

        while (true)
        {
            yield return wait;

            if (_runner != null && _runner.IsRunning && _runner.IsServer)
            {
                var task = _runner.PushHostMigrationSnapshot();

                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (!task.Result)
                {
                    Debug.LogWarning("PushHostMigrationSnapshot failed");
                }
            }
        }
    }
    

    #region TheShadowRealm4.0

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    #endregion

}
