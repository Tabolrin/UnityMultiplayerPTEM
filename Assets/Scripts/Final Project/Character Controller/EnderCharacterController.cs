using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnderCharacterController : NetworkBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject model;
    [SerializeField] Rigidbody rb;
    //[SerializeField] PlayerStats playerStats;

    public override void Spawned()
    {
        if(HasInputAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false; 

            model.SetActive(false);
            playerCamera.SetActive(true);
        }
        else
        {
            model.SetActive(true);
            playerCamera.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out EnderInputData enderData))
        {
            rb.rotation = enderData.LookRotation;
        }
    }
}
