using Fusion;
using UnityEngine;


public class LobbyManager : MonoBehaviour
{
    [SerializeField] NetworkRunner networkRunner;

    public async void StartSession()
    {
        StartGameResult resTask = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "Lobby",
            OnGameStarted = OnGameStarted
        });

        if (resTask.Ok)
            OnGameStarted(networkRunner);
        else
        {
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
        }
    }

    private void OnGameStarted(NetworkRunner obj)
    {
        Debug.Log("Game Started");
    }
}
