using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public enum AstronautColor { Black, Blue, Green, Orange, Pink, Red, White, Yellow, Gray}
public class EnderCharacterController : NetworkBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject model;
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    [SerializeField] PlayerMaterialsContainer matContainer;
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
            if(enderData.buttons.IsSet(ButtonDefenitions.Move))
            {
                rb.linearVelocity = enderData.velocity;
            }
        }
    }
    
    //make sure this is called for every pc after the player is spawned the color isnt getting synchronized just by changing it on the host
    public void SetColor(AstronautColor color)
    {
        switch (color)
        {
            case AstronautColor.Black:
                meshRenderer.material = matContainer.blackMaterial;
                break;
            case AstronautColor.Blue:
                meshRenderer.material = matContainer.blueMaterial;
                break;
            case AstronautColor.Green:
                meshRenderer.material = matContainer.greenMaterial;
                break;
            case AstronautColor.Orange:
                meshRenderer.material = matContainer.orangeMaterial;
                break;
            case AstronautColor.Pink:
                meshRenderer.material = matContainer.pinkMaterial;
                break;
            case AstronautColor.Red:
                meshRenderer.material = matContainer.redMaterial;
                break;
            case AstronautColor.White:
                meshRenderer.material = matContainer.whiteMaterial;
                break;
            case AstronautColor.Yellow:
                meshRenderer.material = matContainer.yellowMaterial;
                break;
            case AstronautColor.Gray:
                meshRenderer.material = matContainer.grayMaterial;
                break;
        }
    }
}
