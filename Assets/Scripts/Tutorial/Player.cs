using System;
using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkCharacterController characterController;
    [SerializeField] private SkinnedMeshRenderer _playerMeshRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerMaterialsContainer _materialsContainer;
    

    [Networked] private TickTimer _delay { get; set; }
    private Vector3 _forward;

    public override void Spawned()
    {
    }
    
    public void SetMaterial(Material material)
    {
        if (!Runner.IsServer) return;

        _playerMeshRenderer.material = material;
    }

    
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            characterController.Move(5*data.direction*Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && _delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.mouseButtonLeft))
                {
                    _delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                   
                }
                else if (data.buttons.IsSet(NetworkInputData.mouseButtonRight))
                {
                    _delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                }
            }
        }
    }

}
