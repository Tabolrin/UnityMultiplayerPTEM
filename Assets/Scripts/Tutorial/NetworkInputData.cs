using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public const byte mouseButtonLeft = 1;
    public const byte mouseButtonRight = 2;
    public NetworkButtons buttons;
}