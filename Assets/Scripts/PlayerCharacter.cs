using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : NetworkBehaviour
{
    public const string PLAYER_TAG = "Player";
    
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject jerryProjectilePrefab;
    
    [SerializeField] private int health = 100;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSpeed = 2f;
    
    
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
    }

    
    void OnMove(InputAction.CallbackContext ctx)
    {
        
    }
    
    
    void OnAttack(InputAction.CallbackContext ctx)
    {
        Runner.SpawnAsync(jerryProjectilePrefab, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);
    }
    
    
    void OnLook(InputAction.CallbackContext ctx)
    {
        
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCTakeDamage(int damage, RpcInfo info = default)
    {
        if (HasStateAuthority)
        {
            health -= damage;
            Debug.Log($"Player {info.Source.PlayerId} took {damage} damage. Remaining health: {health}");
            
            if (health <= 0)
            {
                Debug.Log($"Player {info.Source.PlayerId} has died.");
                Runner.Despawn(Object);
            }
        }
    }
}
