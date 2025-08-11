using Fusion;
using UnityEngine;

public class MovingObstacle : NetworkBehaviour
{
    [SerializeField] private Transform pos1;
    [SerializeField] private Transform pos2;
    [SerializeField] private float speed = 1f;

    public override void FixedUpdateNetwork()
    {
        float t = Mathf.PingPong((float)Runner.SimulationTime * speed, 1f);

        transform.position = Vector3.Lerp(pos1.position, pos2.position, t);
    }
}
