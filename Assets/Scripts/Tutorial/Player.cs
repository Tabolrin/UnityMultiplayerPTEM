using System;
using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkCharacterController characterController;
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysXBall _prefabPhysXBall;
    [SerializeField] private Material _material;
    [SerializeField] private Animator _animator;
    
    [Networked]
    public bool spawnedProjectile { get; set; }
    [Networked] private TickTimer _delay { get; set; }
    private Vector3 _forward;
    private bool _youShallChangeCOLOR = false;

    public override void Spawned()
    {
        
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
                    Runner.Spawn(_prefabBall,
                        transform.position+_forward,
                        Quaternion.LookRotation(_forward),
                        Object.InputAuthority,
                        (runner, o) =>
                        {
                            // Initialize the Ball before synchronizing it
                            o.GetComponent<Ball>().Init();
                            _youShallChangeCOLOR = true;
                        });
                }
                else if (data.buttons.IsSet(NetworkInputData.mouseButtonRight))
                {
                    _delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn
                    (
                        _prefabPhysXBall,
                        transform.position+_forward,
                        Quaternion.LookRotation(_forward),
                        Object.InputAuthority,
                        (runner, o) =>
                        {
                            o.GetComponent<PhysXBall>().Init( 10*_forward );
                        }
                    );
                }
            }
        }
    }
    
    public override void Render()
    {
        if (_youShallChangeCOLOR)
        {
            _material.color = Color.white;
            _youShallChangeCOLOR = false;
        }
        _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    }
}
