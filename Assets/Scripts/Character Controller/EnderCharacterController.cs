using Fusion;
using UnityEngine;



public enum AstronautColor { Black, Blue, Green, Orange, Pink, Red, White, Yellow, Gray}

public class EnderCharacterController : NetworkBehaviour
{
    private const string GATE_TAG = "Gate";

    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject model;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private PlayerMaterialsContainer matContainer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LaserPulse laser;

    [Networked] AstronautColor myColor { get; set; } = AstronautColor.Gray;

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
        
        if (!laser) 
            laser = GetComponentInChildren<LaserPulse>(true);
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
                if (laser)
                    laser.Pulse();

                if (RaycastAShot(out PlayerRef myHitPlayer, out EnderCharacterController hitController))
                {
                    if (myHitPlayer == enderData.hitTarget && !hitController.Frozen)
                    {
                        hitController.Freeze();
                        score+= 1000;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GATE_TAG))
        {
            Gate gate = other.GetComponent<Gate>();
            PlayerRef me = new PlayerRef();
            foreach(PlayerRef player in Runner.ActivePlayers)
                if(PlayerData.Get(Runner, player).Avatar == Object)
                    me = player;

            if ( gate.Team != PlayerData.Get(Runner, me).Team)
            {
                GameManager.Instance.RPC_Goal(PlayerData.Get(Runner, me).Team);
            }
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

        Debug.Log("EnderController hit " + hitInfo.collider.gameObject.tag);
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
