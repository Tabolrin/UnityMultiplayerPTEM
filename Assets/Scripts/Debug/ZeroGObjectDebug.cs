using UnityEngine;

public class ZeroGObjectDebug : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.D))
        {
            rb.AddForce(Vector3.forward, ForceMode.Force);
        }
    }
}
