using System;
using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkCharacterController characterController;
    [SerializeField] private Animator _animator;
    
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            characterController.Move(data.direction * 5 * Runner.DeltaTime );
            
            _animator.SetFloat("Speed", 5 * data.direction.magnitude);
        }
    }
}
