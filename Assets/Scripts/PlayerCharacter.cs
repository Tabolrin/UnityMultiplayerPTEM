// using Fusion; // Uncomment when testing in a networked scene
using UnityEngine;
using UnityEngine.InputSystem;

// public class PlayerCharacter : NetworkBehaviour // ← Uncomment for Fusion
public class PlayerCharacter : MonoBehaviour
{
    public const string PLAYER_TAG = "Player";

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

    // Uncomment for Fusion
    // public override void FixedUpdateNetwork()
    // {
    //     base.FixedUpdateNetwork();
    //     HandleMovement();
    // }

    // Local testing
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Uncomment this check in networked context
        // if (!HasInputAuthority) return;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        //position movement
        Vector3 movementForward = cameraTransform.forward * moveInput.y;
        Vector3 movementRight = cameraTransform.right * moveInput.x;
        Vector3 directionVector = movementForward + movementRight;
        directionVector.y = 0;
        Vector3 movement = directionVector.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        //rotation
        //Vector3 look = new Vector3(lookDirection.x, 0, lookDirection.y);
        Model.transform.rotation = Quaternion.Euler(new Vector3(0, orbitAngle, 0));
        
        //rb.angularVelocity = Vector3.zero;
        //Model.transform.rotation = Quaternion.Euler(new Vector3(0, orbitAngle, 0));
        
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
            // Runner.Spawn(jerryProjectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

            // Local version:
            Instantiate(jerryProjectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
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
    /*
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
    */
}
