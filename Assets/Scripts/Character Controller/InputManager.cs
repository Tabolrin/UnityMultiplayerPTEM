using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    static public InputManager Instance;

    [SerializeField] PlayerStats playerStats;
    [SerializeField] string playerLayer;

    private Vector3 deltaLookRotation;
    private Quaternion _lookDirection = Quaternion.identity;
    private Vector3 movement;
    private PlayerRef hitTarget;

    private bool shouldInputMove = false;
    private bool shouldInputShoot = false;

    private NetworkRunner runner 
    { 
        get 
        {
            if (!_runner_var)
                _runner_var = NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            return _runner_var;
        } 
    }
    
    private NetworkRunner _runner_var;


    private void Start()
    {
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //translating the input into something that can be sent to the network
    internal void OnInput(NetworkRunner runner, NetworkInput input)
    {
        EnderInputData enderData = new EnderInputData();
        enderData.LookRotation = _lookDirection;
        
        if (shouldInputMove)
        {
            shouldInputMove = false;
            enderData.velocity = movement;
            enderData.buttons.Set(ButtonDefenitions.Move, true);
        }
        
        if (shouldInputShoot)
        {
            shouldInputShoot = false;
            enderData.hitTarget = hitTarget;
            enderData.buttons.Set(ButtonDefenitions.Shoot, true);
        }

        input.Set(enderData);
    }
    

    //proccess the input between ticks
    private void Update()
    {
        Quaternion deltaRotation = Quaternion.Euler(deltaLookRotation);
        _lookDirection *= deltaRotation;
    }

    //get the input with the new input system
    public void OnLook(InputAction.CallbackContext inputContext)
    {
        if (Cursor.visible)
            return;
        Vector2 input = inputContext.ReadValue<Vector2>();
        deltaLookRotation.x = -input.y * playerStats.LookSpeed;
        deltaLookRotation.y = input.x * playerStats.LookSpeed;
    }
    public void OnRoll(InputAction.CallbackContext inputContext)
    {
        if (Cursor.visible)
            return;
        deltaLookRotation.z = -inputContext.ReadValue<float>() * playerStats.LookSpeed;
        
    }
    public void OnMove(InputAction.CallbackContext inputContext) 
    {
        if (Cursor.visible)
            return;
        movement = _lookDirection * Vector3.forward * playerStats.MoveSpeed;
        shouldInputMove = true;
    }

    public void OnMouseToggle(InputAction.CallbackContext inputContext)
    {
        if (!Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public void OnShoot(InputAction.CallbackContext inputContext) 
    {
        if(Cursor.visible)
            return;

        movement = _lookDirection * Vector3.forward * playerStats.MoveSpeed;
        RaycastAShot(out PlayerRef currentHitPlayer);
        //if hitTarget.team == notMine (figure how that whole thing works)
        shouldInputShoot = true;
        hitTarget = currentHitPlayer;
    }

    private bool RaycastAShot(out PlayerRef hitPlayer)
    {
        //null random to not throw an error if we dont get a hit
        hitPlayer = new PlayerRef();

        //find who my player is and then raycast from them
        Transform myPlayer = PlayerData.Get(runner, runner.LocalPlayer).Avatar.transform;
        Ray ray = new Ray(myPlayer.position, myPlayer.forward);
        Physics.Raycast(ray, out RaycastHit hitInfo);

        //Debug.Log("InputManager hit " + hitInfo.collider.gameObject.tag);
        //start filtering the possible hits until you find who was hit if anyone was hit at all
        if (hitInfo.collider && hitInfo.collider.gameObject.CompareTag(playerLayer))
        {
            NetworkObject hitObject = hitInfo.transform.GetComponent<NetworkObject>();
            foreach (PlayerRef playerRef in runner.ActivePlayers)
            {
                if (PlayerData.Get(runner, playerRef).Avatar == hitObject && PlayerData.Get(runner, playerRef).Team != PlayerData.Get(runner, runner.LocalPlayer).Team)
                {
                    hitPlayer = playerRef;
                    return true;
                }
            }
        }
        return false;
    }
}
