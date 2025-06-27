using System;
using Fusion;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 10f;
    
    
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        if(Object.HasStateAuthority)
            transform.Translate(Vector3.forward * speed * Runner.DeltaTime);
        
        lifeTime -= Runner.DeltaTime;
        
        if(lifeTime<= 0)
            Runner.Despawn(Object);
        
    }

    
    private void OnTriggerEnter(Collider collider)
    {
        if (HasStateAuthority && collider.gameObject.CompareTag(PlayerCharacter.PLAYER_TAG))
        {
            PlayerCharacter player = collider.gameObject.GetComponent<PlayerCharacter>();
            
            //if (!player.HasInputAuthority)
            //{
                //player.RPCTakeDamage(10);
                //Runner.Despawn(Object);
            //}
        }
    }
}
