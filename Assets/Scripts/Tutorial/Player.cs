using System;
using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkCharacterController characterController;
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private Animator _animator;
    [Networked] private TickTimer _delay { get; set; }
    private Vector3 _forward;

    
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && _delay.ExpiredOrNotRunning(Runner))
        {
           if(GetInput(out NetworkInputData data))
           {
              data.direction.Normalize();
              characterController.Move(data.direction * 10 * Runner.DeltaTime );
              _animator.SetFloat("Speed", 1 * data.direction.magnitude);

              if (data.direction.sqrMagnitude > 0)
                  _forward = data.direction;
              
              if (data.buttons.IsSet(NetworkInputData.mouseButton0)) 
              {
                  _delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                  
                  Runner.Spawn
                  (
                      _prefabBall,
                      transform.position+_forward, Quaternion.LookRotation(_forward),
                      Object.InputAuthority, 
                      (runner, o) =>
                      {
                          o.GetComponent<Ball>().Init();
                      });
              }
           }  
        }
    }
}
