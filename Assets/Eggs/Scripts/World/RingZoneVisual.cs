using UnityEngine;

namespace Eggs.Gameplay
{
    // Optional presentation: removing this component never changes zone membership or geometry.
    [ExecuteAlways]
    public sealed class RingZoneVisual : MonoBehaviour
    {
        [SerializeField] private WorkZone zone;
        [SerializeField] private LineRenderer outline;
        [Min(12)] [SerializeField] private int segments = 96;

        private Vector3[] points;
        private Vector3 lastCenter;
        private float lastRadius = -1f;

        private void LateUpdate()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (outline == null)
                return;
            outline.enabled = zone != null && zone.Shape == WorkZoneShape.Ring && zone.HasValidGeometry;
            if (!outline.enabled)
                return;

            Vector3 center = zone.RingCenter.position;
            int count = Mathf.Clamp(segments, 12, 512);
            if (points != null && points.Length == count && lastCenter == center && lastRadius == zone.OuterRadius)
                return;

            points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float angle = i * (Mathf.PI * 2f / count);
                points[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * zone.OuterRadius;
            }
            outline.useWorldSpace = true;
            outline.loop = true;
            outline.positionCount = count;
            outline.SetPositions(points);
            lastCenter = center;
            lastRadius = zone.OuterRadius;
        }

        private void OnDisable()
        {
            if (outline != null)
                outline.enabled = false;
        }
    }
}
