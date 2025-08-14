using Fusion;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    [Networked] public NetworkString<_32> Nickname { get; set; }
    [Networked] public byte Team { get; set; }
    [Networked] public byte ColorIndex { get; set; }
    [Networked] public NetworkObject Avatar { get; set; }

    public static PlayerData Get(NetworkRunner runner, PlayerRef player)
    {
        var playerObject = runner.GetPlayerObject(player);
        return playerObject ? playerObject.GetComponent<PlayerData>() : null;
    }

    public static PlayerData Local(NetworkRunner runner) => Get(runner, runner.LocalPlayer);

    public static void SetLocal(NetworkRunner runner, string nickname, int team, int colorIndex = 0) 
    {
        var data = Local(runner);
        if (data != null) data.RPC_Set(nickname, (byte)team, (byte)colorIndex);
        else Debug.LogWarning("PlayerData not spawned yet. Try again shortly.");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Set(NetworkString<_32> nickname, byte team, byte colorIndex, RpcInfo _ = default) 
    {
        Nickname   = nickname;
        Team       = team;
        ColorIndex = colorIndex;
    }
}