using Fusion; 
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : NetworkBehaviour, IStateAuthorityChanged  
{
    public const string PLAYER_TAG = "Player";
    
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject jerryProjectilePrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject Model;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera localCamera;
    [SerializeField] private  ParticleSystem particleSystem;

    [Header("Player Settings")]
    [Networked, OnChangedRender(nameof(HpChanged))][field:SerializeField]
    public int HP { get; set; }
    [SerializeField] private float moveSpeed = 5f;

    [Header("Camera Orbit Settings")]
    [SerializeField] private float orbitSpeed = 15;

    private float orbitAngle = 0f;
    private Vector2 moveInput;

    public override void Spawned()
    {
        StateAuthorityChanged();
    }


    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        HandleMovement();
    }

    public void TurnOffAttackBool()
    {
        animator.SetBool("AttackB", false);
    }
    

    private void HpChanged()
    {
        Debug.Log("---- OnChangeRender new hp: " + HP);
    }
    
    
    private void HandleMovement()
    {
        if (rb == null) return;
        
        rb.rotation = Quaternion.Euler(0, orbitAngle, 0);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Vector3 movementForward = cameraTransform.forward * moveInput.y;
        Vector3 movementRight = cameraTransform.right * moveInput.x;
        Vector3 directionVector = movementForward + movementRight;
        directionVector.y = 0f;

        Vector3 movement = directionVector.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        
        if (animator != null)
            animator.SetFloat("MoveSpeed", moveInput.magnitude);
    }


    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        if (animator != null)
            animator.SetBool("AttackB", true);

        if (jerryProjectilePrefab != null && projectileSpawnPoint != null)
        {
            Runner.Spawn(jerryProjectilePrefab, projectileSpawnPoint.position, transform.rotation);
        }
    }
    

    public void OnLook(InputAction.CallbackContext ctx)
    {
        Vector2 deltaMovement = ctx.ReadValue<Vector2>();
        deltaMovement.y = 0;
        orbitAngle += deltaMovement.x * orbitSpeed;
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCTakeDamage(int damage, RpcInfo info = default)
    {
        if (damage > HP || damage < 0)
        {
            Debug.Log($"Unusual damage dealt: {damage}");
            return;
        }
        
        particleSystem.Play();
        
        HP -= damage;
        Debug.Log($"Player {info.Source.PlayerId} took {damage} damage. Remaining health: {HP}");

        if (HasStateAuthority && HP <= 0)
        {
            Debug.Log($"Player {info.Source.PlayerId} has died.");
            Runner.Despawn(Object);
        }
    }
    
    
    public void StateAuthorityChanged()
    {
        Debug.Log("StateAuthorityChanged called for PlayerCharacter");
        bool isMine = HasStateAuthority;

        if (playerInput != null)
            playerInput.enabled = isMine;

        if (cameraTransform != null)
            cameraTransform.gameObject.SetActive(isMine);
    }
}


