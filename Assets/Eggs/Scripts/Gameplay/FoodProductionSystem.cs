using System;
using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class FoodProductionSystem : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private WorkZone foodZone;
        [Min(0.05f)] [SerializeField] private float gatherInterval = 1f;
        [Min(0)] [SerializeField] private int foodPerUnitPerTick = 1;

        private float elapsed;

        private void Start()
        {
            if (gameState == null || foodZone == null || foodZone.TargetState != UnitWorkState.Gathering)
            {
                Debug.LogError("FoodProductionSystem needs GameState and a Gathering WorkZone.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (gameState == null || !gameState.isActiveAndEnabled || foodZone == null
                || !foodZone.CanAcceptUnits || foodZone.TargetState != UnitWorkState.Gathering)
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
                long gained = (long)foodZone.MemberCount * Math.Max(0, foodPerUnitPerTick);
                gameState.AddFood((int)Math.Min(int.MaxValue, gained));
            }
        }

        private void OnDisable()
        {
            elapsed = 0f;
        }
    }
}
