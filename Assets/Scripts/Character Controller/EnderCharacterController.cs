using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Collections.Unicode;


public enum AstronautColor { Black, Blue, Green, Orange, Pink, Red, White, Yellow, Gray}

public class EnderCharacterController : NetworkBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject model;
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    [SerializeField] PlayerMaterialsContainer matContainer;
    [SerializeField] Rigidbody rb;

    AstronautColor myColor = AstronautColor.Gray;

    //[SerializeField] PlayerStats playerStats;

    [Networked][OnChangedRender(nameof(FreezeColor))] public bool Frozen { get; private set; }
    [Networked] public int score { get; private set; }

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
            transform.rotation = enderData.LookRotation;
            if (Frozen) return;

            if (enderData.buttons.IsSet(ButtonDefenitions.Move))
            {
                rb.linearVelocity = enderData.velocity;
            }
            //rb.rotation = enderData.LookRotation;
            if(enderData.buttons.IsSet(ButtonDefenitions.Shoot))
            {
                if (RaycastAShot(out PlayerRef myHitPlayer, out EnderCharacterController hitController))
                {
                    if (myHitPlayer == enderData.hitTarget && !hitController.Frozen)
                    {
                        hitController.Freeze();
                        score++;
                        PlayerData.Get(Runner, Runner.LocalPlayer).scoreData.score = score;
                    }
                }
            }
        }
    }

    public void Freeze()
    {
        Frozen = true;
    }

    private void FreezeColor()
    {
        switch (myColor)
        {
            case AstronautColor.Black:
                meshRenderer.material = matContainer.freezeBlackMaterial;
                break;                               
            case AstronautColor.Blue:                
                meshRenderer.material = matContainer.freezeBlueMaterial;
                break;                               
            case AstronautColor.Green:               
                meshRenderer.material = matContainer.freezeGreenMaterial;
                break;                               
            case AstronautColor.Orange:              
                meshRenderer.material = matContainer.freezeOrangeMaterial;
                break;                               
            case AstronautColor.Pink:                
                meshRenderer.material = matContainer.freezePinkMaterial;
                break;                               
            case AstronautColor.Red:                 
                meshRenderer.material = matContainer.freezeRedMaterial;
                break;                               
            case AstronautColor.White:               
                meshRenderer.material = matContainer.freezeWhiteMaterial;
                break;                               
            case AstronautColor.Yellow:              
                meshRenderer.material = matContainer.freezeYellowMaterial;
                break;                               
            case AstronautColor.Gray:                
                meshRenderer.material = matContainer.freezeGrayMaterial;
                break;
        }
    }
    
    //make sure this is called for every pc after the player is spawned the color isnt getting synchronized just by changing it on the host
    public void SetColor(AstronautColor color)
    {
        myColor = color;
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

    //double checking the raycast hit from the host too
    private bool RaycastAShot(out PlayerRef hitPlayer, out EnderCharacterController playerController)
    {
        //null random to not throw an error if we dont get a hit
        hitPlayer = new PlayerRef();
        playerController = null;

        //find who my player is and then raycast from them
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out RaycastHit hitInfo);

        //start filtering the possible hits until you find who was hit if anyone was hit at all
        if (hitInfo.collider.gameObject.tag == gameObject.tag)
        {
            NetworkObject hitObject = hitInfo.transform.GetComponent<NetworkObject>();
            foreach (PlayerRef playerRef in Runner.ActivePlayers)
            {
                if (PlayerData.Get(Runner, playerRef).Avatar == hitObject)
                {
                    playerController = hitObject.GetComponent<EnderCharacterController>();
                    hitPlayer = playerRef;
                    return true;
                }
            }
        }
        return false;
    }
}
