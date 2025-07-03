 using Fusion; // Uncomment when testing in a networked scene

using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

 public class PlayerCharacter : NetworkBehaviour, IStateAuthorityChanged  // ← Uncomment for Fusion
//public class PlayerCharacter : MonoBehaviour
{
    public const string PLAYER_TAG = "Player";

    [Networked, OnChangedRender(nameof(HpChanged))][field:SerializeField]
    public int HP { get; set; }
    
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject jerryProjectilePrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject Model;

    [Header("Player Settings")]
    [SerializeField] private int health = 100;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Camera Orbit Settings")]
    [SerializeField] private float orbitDistance = 4f;
    [SerializeField] private float orbitHeight = 1.5f;
    [SerializeField] private float orbitSpeed = 15;

    private float orbitAngle = 0f;
    private Vector2 moveInput;
    private PlayerInput playerInput;
    private Camera localCamera;

    public override void Spawned()
    {
        playerInput = GetComponent<PlayerInput>();
        localCamera = cameraTransform.GetComponent<Camera>();

        StateAuthorityChanged();
    }

    
    
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        HandleMovement();
    }
    

    private void HpChanged()
    {
        Debug.Log("new hp: " + HP);
    }
    
    /*private void HandleMovement()
    {
        // Uncomment this check in networked context
        if (!HasInputAuthority) return;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        //position movement
        Vector3 movementForward = cameraTransform.forward * moveInput.y;
        Vector3 movementRight = cameraTransform.right * moveInput.x;
        Vector3 directionVector = movementForward + movementRight;
        directionVector.y = 0;
        Vector3 movement = directionVector.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        
        Model.transform.rotation = Quaternion.Euler(new Vector3(0, orbitAngle, 0));
        
        if (animator != null)
            animator.SetFloat("MoveSpeed", moveInput.magnitude);
    }*/
    
    
    private void HandleMovement()
    {
        if (!HasInputAuthority || rb == null) return;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Vector3 movementForward = cameraTransform.forward * moveInput.y;
        Vector3 movementRight = cameraTransform.right * moveInput.x;
        Vector3 directionVector = movementForward + movementRight;
        directionVector.y = 0f;

        Vector3 movement = directionVector.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
    
        if (moveInput.magnitude > 0.1f)
            Model.transform.rotation = Quaternion.LookRotation(directionVector);

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
            animator.SetTrigger("Attack");

        if (jerryProjectilePrefab != null && projectileSpawnPoint != null)
        {
            // Fusion version:
             Runner.Spawn(jerryProjectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

            // Local version:
            //Instantiate(jerryProjectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        }
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
            Vector2 deltaMovement = ctx.ReadValue<Vector2>();
            deltaMovement.y = 0;
            orbitAngle += deltaMovement.x * orbitSpeed;
            
            cameraTransform.rotation = Quaternion.Euler(0, orbitAngle , 0);
    }

    // Uncomment for Fusion networked damage
    //*
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
    //
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


