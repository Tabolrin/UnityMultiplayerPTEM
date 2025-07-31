using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }
    
    public void Init()//todo: shall we call him?
    {
        life = TickTimer.CreateFromSeconds(Runner, 5f);
    }
    
    public override void FixedUpdateNetwork()
    {
        transform.position += 5 * transform.forward * Runner.DeltaTime;
        
        if(life.Expired(Runner))
        {
            Runner.Despawn(Object);
            Debug.Log("Ball despawned");
        }
    }
}
