using Fusion;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    [Networked] public NetworkString<_32> Nickname { get; set; }
    [Networked] public byte Team { get; set; }
    [Networked] public AstronautColor TeamColor { get; set; }
    [Networked] public NetworkObject Avatar { get; set; }
    public EnderCharacterController enderCharacterController;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public static PlayerData Get(NetworkRunner runner, PlayerRef player)
    {
        var playerObject = runner.GetPlayerObject(player);
        return playerObject ? playerObject.GetComponent<PlayerData>() : null;
    }

    
    public static PlayerData Local(NetworkRunner runner) => Get(runner, runner.LocalPlayer);

    
    public static void SetLocal(NetworkRunner runner, string nickname) 
    {
        var data = Local(runner);
        
        if (data != null)
            data.RPC_Set(nickname);
        else 
            Debug.LogWarning("PlayerData not spawned yet. Try again shortly.");
    }

    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Set(NetworkString<_32> nickname, RpcInfo _ = default) 
    {
        Nickname   = nickname;
    }
}