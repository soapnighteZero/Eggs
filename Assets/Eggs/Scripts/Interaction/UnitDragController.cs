using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class UnitDragController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private WorkZone[] workZones;
        [SerializeField] private LayerMask unitLayers = Physics2D.DefaultRaycastLayers;
        [SerializeField] private float dragPlaneZ;
        [Min(0f)] [SerializeField] private float screenEdgePadding = 1.3f;
        [SerializeField] private int draggedSortingOrder = 100;

        private readonly List<Collider2D> hitColliders = new List<Collider2D>();
        private UnitActor draggedUnit;
        private Vector3 grabOffset;
        private SortingGroup draggedSortingGroup;
        private int previousSortingOrder;

        private void Awake()
        {
            if (inputCamera == null || !inputCamera.orthographic)
            {
                Debug.LogError("UnitDragController needs an assigned orthographic camera.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || inputCamera == null || !Application.isFocused)
            {
                FinishDrag(false);
                return;
            }

            Vector2 screenPoint = mouse.position.ReadValue();
            if (!TryGetWorldPoint(screenPoint, out Vector3 worldPoint))
            {
                FinishDrag(false);
                return;
            }

            if (draggedUnit == null && mouse.leftButton.wasPressedThisFrame
                && inputCamera.pixelRect.Contains(screenPoint))
                BeginDrag(worldPoint);

            if (draggedUnit == null || !draggedUnit.isActiveAndEnabled)
            {
                FinishDrag(false);
                return;
            }

            draggedUnit.transform.position = ClampToCamera(worldPoint + grabOffset);

            if (mouse.leftButton.wasReleasedThisFrame)
                FinishDrag(inputCamera.pixelRect.Contains(screenPoint));
            else if (!mouse.leftButton.isPressed)
                FinishDrag(false);
        }

        private void BeginDrag(Vector3 worldPoint)
        {
            // The project disables auto-sync. Queries must see the last drag position.
            Physics2D.SyncTransforms();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(unitLayers);
            filter.useTriggers = true;
            hitColliders.Clear();
            Physics2D.OverlapPoint(worldPoint, filter, hitColliders);

            int bestOrder = int.MinValue;
            float bestDistance = float.PositiveInfinity;
            foreach (Collider2D hit in hitColliders)
            {
                UnitActor unit = hit.GetComponentInParent<UnitActor>();
                if (unit == null || !unit.isActiveAndEnabled)
                    continue;

                SortingGroup group = unit.GetComponent<SortingGroup>();
                int order = group != null ? group.sortingOrder : 0;
                float distance = (unit.transform.position - worldPoint).sqrMagnitude;
                // Select one visible candidate even when units overlap.
                if (order < bestOrder || (order == bestOrder && distance >= bestDistance))
                    continue;

                draggedUnit = unit;
                bestOrder = order;
                bestDistance = distance;
            }

            if (draggedUnit == null)
                return;

            grabOffset = draggedUnit.transform.position - worldPoint;
            draggedUnit.AssignTo(null);
            draggedSortingGroup = draggedUnit.GetComponent<SortingGroup>();
            if (draggedSortingGroup != null)
            {
                previousSortingOrder = draggedSortingGroup.sortingOrder;
                draggedSortingGroup.sortingOrder = draggedSortingOrder;
            }
        }

        private void FinishDrag(bool assignDrop)
        {
            if (draggedUnit != null)
            {
                WorkZone zone = null;
                if (assignDrop)
                {
                    Physics2D.SyncTransforms();
                    zone = FindUniqueZone(draggedUnit.transform.position);
                }
                draggedUnit.AssignTo(zone);
                if (draggedUnit.CurrentZone != null)
                    draggedUnit.transform.position = zone.GetDropPosition(draggedUnit.transform.position);
            }

            if (draggedSortingGroup != null)
                draggedSortingGroup.sortingOrder = previousSortingOrder;

            draggedUnit = null;
            draggedSortingGroup = null;
        }

        private WorkZone FindUniqueZone(Vector2 point)
        {
            WorkZone result = null;
            if (workZones == null)
                return null;

            foreach (WorkZone zone in workZones)
            {
                if (zone == null || zone == result || !zone.Contains(point))
                    continue;
                if (result != null)
                {
                    Debug.LogWarning($"Drop overlaps '{result.name}' and '{zone.name}'. "
                        + "Assignment rejected; unit stays Idle. Separate the work zones.", this);
                    return null;
                }
                result = zone;
            }
            return result;
        }

        private bool TryGetWorldPoint(Vector2 screenPoint, out Vector3 point)
        {
            Ray ray = inputCamera.ScreenPointToRay(screenPoint);
            Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, dragPlaneZ));
            if (plane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = default;
            return false;
        }

        private Vector3 ClampToCamera(Vector3 point)
        {
            Rect viewport = inputCamera.pixelRect;
            if (TryGetWorldPoint(viewport.min, out Vector3 min)
                && TryGetWorldPoint(viewport.max, out Vector3 max))
            {
                float paddingX = Mathf.Min(screenEdgePadding, Mathf.Max(0f, (max.x - min.x) * 0.5f));
                float paddingY = Mathf.Min(screenEdgePadding, Mathf.Max(0f, (max.y - min.y) * 0.5f));
                point.x = Mathf.Clamp(point.x, min.x + paddingX, max.x - paddingX);
                point.y = Mathf.Clamp(point.y, min.y + paddingY, max.y - paddingY);
            }
            point.z = dragPlaneZ;
            return point;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                FinishDrag(false);
        }

        private void OnDisable()
        {
            FinishDrag(false);
        }
    }
}
