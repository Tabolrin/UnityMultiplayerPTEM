using UnityEngine;

public class Gate : MonoBehaviour
{

    [SerializeField] private Animator animator;

    private void Start()
    {
        GameManager.OnRoundStarted += OpenGate;
    }

    private void OnDisable()
    {
        GameManager.OnRoundStarted -= OpenGate;
    }

    [ContextMenu("Open Gate")]
    private void OpenGate()
    {
        animator.SetBool("Open", true);
    }

    [ContextMenu("Close Gate")]
    private void CloseGate()
    {
        animator.SetBool("Open", false);
    }
}
