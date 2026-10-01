using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class FoodProductionSystem : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private WorkZone[] foodZones;
        [Min(0.05f)] [SerializeField] private float gatherInterval = 1f;
        [Min(0)] [SerializeField] private int foodPerUnitPerTick = 1;

        private float elapsed;
        private readonly HashSet<WorkZone> countedZones = new HashSet<WorkZone>();

        private void Start()
        {
            if (gameState == null || foodZones == null || foodZones.Length == 0)
            {
                Debug.LogError("FoodProductionSystem needs GameState and a Food WorkZone array.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (gameState == null || !gameState.isActiveAndEnabled)
            {
                elapsed = 0f;
                return;
            }

            elapsed += Time.deltaTime;
            float interval = Mathf.Max(0.05f, gatherInterval);
            while (elapsed >= interval)
            {
                elapsed -= interval;
                // Read membership at the tick, never cache Units that have left.
                long gained = CountGatherers() * Math.Max(0, foodPerUnitPerTick);
                gameState.AddFood((int)Math.Min(int.MaxValue, gained));
            }
        }

        private long CountGatherers()
        {
            countedZones.Clear();
            long count = 0;
            if (foodZones == null)
                return count;
            foreach (WorkZone zone in foodZones)
            {
                if (zone == null || !zone.CanAcceptUnits || !zone.ResourceAvailable
                    || zone.TargetState != UnitWorkState.Gathering || !countedZones.Add(zone))
                    continue;
                // UnitActor owns exactly one membership; duplicate zone references count only once.
                count += zone.MemberCount;
            }
            return count;
        }

        private void OnDisable()
        {
            elapsed = 0f;
        }
    }
}
