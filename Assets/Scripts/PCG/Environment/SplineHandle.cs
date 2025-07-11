using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SplineHandle : MonoBehaviour
{
    SplineContainer container;
    int knotIndex;
    SplineManagerNew manager;

    public void Init(SplineContainer c, int idx, SplineManagerNew m)
    {
        container = c;
        knotIndex = idx;
        manager = m;
    }

    void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit))
        {
            Vector3 local = container.transform.InverseTransformPoint(hit.point);
            var knot = container.Spline[knotIndex];
            knot.Position = new float3(local.x, local.y, local.z);
            container.Spline[knotIndex] = knot;

            // Visuals aktualisieren
            manager.UpdateLines();
            manager.SpawnHandles(container);
        }
    }
}
