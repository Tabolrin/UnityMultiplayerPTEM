using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    static public InputManager Instance;

    [SerializeField] PlayerStats playerStats;

    private Vector3 deltaLookRotation;
    private Quaternion _lookDirection = Quaternion.identity;
    private Vector3 movement;

    private bool shouldInputMove = false;

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
        Vector2 input = inputContext.ReadValue<Vector2>() * playerStats.LookSpeed;
        deltaLookRotation.x = -input.y * playerStats.LookSpeed;
        deltaLookRotation.y = input.x * playerStats.LookSpeed;
    }
    public void OnRoll(InputAction.CallbackContext inputContext)
    {
        deltaLookRotation.z = -inputContext.ReadValue<float>() * playerStats.LookSpeed;
        
    }
    public void OnMove(InputAction.CallbackContext inputContext) 
    {
        movement = _lookDirection * Vector3.forward * playerStats.MoveSpeed;
        shouldInputMove = true;
    }
}
