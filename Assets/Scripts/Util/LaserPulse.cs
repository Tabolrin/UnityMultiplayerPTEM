using System.Collections;
using UnityEngine;

public class LaserPulse : MonoBehaviour
{
    [Header("Beam Setup")]
    [SerializeField] private Transform muzzle;            
    [SerializeField] private Material beamMaterial;       // Unlit/Additive recommended
    [SerializeField] private float width = 0.03f;
    [SerializeField] private float life = 0.75f;
    [SerializeField] private float range = 150f;
    [SerializeField] private LayerMask hitMask = ~0;      // exclude your player layer if needed
    [SerializeField] private LineRenderer lineRenderer;
    
    private Coroutine pulseCoro;

    void Awake()
    {
        if (!lineRenderer) lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineRenderer.endWidth = width;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.alignment = LineAlignment.View;
        if (beamMaterial) lineRenderer.material = beamMaterial;
    }

    
    public void Pulse()
    {
        Vector3 start = muzzle ? muzzle.position : transform.position;
        Vector3 dir   = muzzle ? muzzle.forward  : transform.forward;

        Vector3 end = start + dir * range;
        if (Physics.Raycast(start, dir, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
            end = hit.point;

        if (pulseCoro != null) StopCoroutine(pulseCoro);
        pulseCoro = StartCoroutine(PulseRoutine(start, end));
    }

    private IEnumerator PulseRoutine(Vector3 start, Vector3 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        yield return new WaitForSeconds(life);
        lineRenderer.enabled = false;
    }
}