using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class BreedingSystem : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private WorkZone loveNestZone;
        [SerializeField] private EggHatch eggPrefab;
        [SerializeField] private Transform eggSpawnPoint;
        [Min(0)] [SerializeField] private int breedingFoodCost = 5;
        [Min(0.05f)] [SerializeField] private float breedingDuration = 5f;
        [SerializeField] private TextMesh statusLabel;

        private UnitActor firstUnit;
        private UnitActor secondUnit;
        private EggHatch activeEgg;
        private float remainingSeconds;

        public bool IsBreeding { get; private set; }
        public float RemainingSeconds => remainingSeconds;
        public EggHatch ActiveEgg => activeEgg;

        private void Start()
        {
            if (gameState == null || loveNestZone == null
                || loveNestZone.TargetState != UnitWorkState.Breeding || eggSpawnPoint == null
                || eggPrefab == null || !eggPrefab.HasValidUnitPrefab
                || !eggPrefab.enabled || !eggPrefab.gameObject.activeSelf)
            {
                Debug.LogError("BreedingSystem needs GameState, a Breeding WorkZone, a spawn point "
                    + "and an active Egg prefab with a valid Unit prefab reference.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (gameState == null || !gameState.isActiveAndEnabled || loveNestZone == null
                || !loveNestZone.CanAcceptUnits || loveNestZone.TargetState != UnitWorkState.Breeding)
            {
                EndCycle();
                return;
            }

            if (IsBreeding)
            {
                if (!IsParticipantValid(firstUnit) || !IsParticipantValid(secondUnit))
                {
                    EndCycle();
                    return;
                }

                remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
                if (remainingSeconds <= 0f)
                    CompleteCycle();
                return;
            }

            // Even a disabled, unhatched Egg occupies the single slot until resumed/destroyed.
            if (activeEgg != null || eggPrefab == null || eggSpawnPoint == null)
                return;

            UnitActor first = null;
            UnitActor second = null;
            foreach (UnitActor unit in loveNestZone.Members)
            {
                if (unit == null || !unit.isActiveAndEnabled || unit.IsInteractionLocked
                    || unit.CurrentZone != loveNestZone || unit.CurrentState != UnitWorkState.Breeding)
                    continue;
                if (first == null)
                    first = unit;
                else
                {
                    second = unit;
                    break;
                }
            }

            if (second == null || !gameState.TrySpendFood(Mathf.Max(0, breedingFoodCost)))
                return;

            firstUnit = first;
            secondUnit = second;
            firstUnit.SetInteractionLocked(true);
            secondUnit.SetInteractionLocked(true);
            remainingSeconds = Mathf.Max(0.05f, breedingDuration);
            IsBreeding = true;
        }

        private bool IsParticipantValid(UnitActor unit)
        {
            return unit != null && unit.isActiveAndEnabled && unit.IsInteractionLocked
                && unit.CurrentZone == loveNestZone && unit.CurrentState == UnitWorkState.Breeding;
        }

        private void CompleteCycle()
        {
            try
            {
                if (eggPrefab == null || eggSpawnPoint == null)
                {
                    Debug.LogError("Breeding lost its Egg prefab or spawn point. Cycle cancelled.", this);
                    return;
                }

                activeEgg = Instantiate(eggPrefab, eggSpawnPoint.position, Quaternion.identity);
                // Scene state is injected before the spawned Egg's Start/Update.
                activeEgg.Initialize(gameState);
            }
            finally
            {
                EndCycle();
            }
        }

        private void EndCycle()
        {
            IsBreeding = false;
            remainingSeconds = 0f;
            ReleaseUnit(firstUnit);
            ReleaseUnit(secondUnit);
            firstUnit = null;
            secondUnit = null;
        }

        private static void ReleaseUnit(UnitActor unit)
        {
            if (unit == null)
                return;
            unit.SetInteractionLocked(false);
            unit.AssignTo(null);
        }

        private void LateUpdate()
        {
            if (statusLabel == null)
                return;
            if (IsBreeding)
                statusLabel.text = $"BREEDING {remainingSeconds:0.0}s";
            else if (activeEgg != null)
                statusLabel.text = "EGG INCUBATING";
            else if (loveNestZone != null && loveNestZone.MemberCount >= 2
                && gameState != null && gameState.Food < Mathf.Max(0, breedingFoodCost))
                statusLabel.text = $"NEED {Mathf.Max(0, breedingFoodCost)} FOOD";
            else
                statusLabel.text = "DROP 2 UNITS TO BREED";
        }

        private void OnDisable()
        {
            // Cancellation consumes no further Food; an already-paid cost is not refunded.
            EndCycle();
            if (statusLabel != null)
                statusLabel.text = string.Empty;
            // Keep the existing Egg reference so re-enabling cannot open a second slot.
        }
    }
}
