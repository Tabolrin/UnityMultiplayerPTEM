using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Multiplayer;


public class LobbyManager : MonoBehaviour
{
    const string catsLobby = "Cats";
    const string puppiesLobby = "Puppies";
    const string capysLobby = "Capybara's";

    private string currentLobby;
    
    [SerializeField] NetworkRunner networkRunner;
    
    [SerializeField] private Button startSessionButton;
    [SerializeField] private Button endSessionButton;
    [SerializeField] private TextMeshProUGUI numberOfPlayersText;

    
    public async void StartSession(int playerLimit)
    {
        StartGameResult resTask = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "Lobby",
            PlayerCount = 4,
            OnGameStarted = OnGameStarted,
            CustomLobbyName = currentLobby,
        });

        if (resTask.Ok)
            OnGameStarted(networkRunner);
        else
        {
            Debug.LogError($"Game start failed: {resTask.ShutdownReason}");
        }
    }
    
    
    public async void JoinLobby(string lobbyName)
    {
        StartGameResult result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyName);
     
        if (result.Ok)
        {
            Debug.Log("Joined Lobby!");
            currentLobby = lobbyName;
        }
        else
        {
            Debug.LogError($"Game join failed: {result.ShutdownReason}");
        }
    }

    private void OnGameStarted(NetworkRunner obj)
    {
        Debug.Log("Game Started");
    }
}
