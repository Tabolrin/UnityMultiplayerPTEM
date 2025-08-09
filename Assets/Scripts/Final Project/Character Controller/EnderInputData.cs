using Fusion;
using UnityEngine;

enum ButtonDefenitions { Move = 1, Shoot = 2 };
public struct EnderInputData : INetworkInput
{
    public Quaternion LookRotation;
    public Vector3 velocity;
    public NetworkButtons buttons;
}
