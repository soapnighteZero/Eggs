using UnityEngine;
using UnityEngine.Rendering;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EggHatch : MonoBehaviour
    {
        [Min(0.05f)] [SerializeField] private float hatchDuration = 4f;
        [SerializeField] private UnitActor unitPrefab;
        [SerializeField] private GameState gameState;
        [SerializeField] private Vector3 unitSpawnOffset = new Vector3(2.6f, -2.6f, 0f);
        [SerializeField] private TextMesh countdownLabel;

        private float elapsed;
        public bool HasHatched { get; private set; }
        public float RemainingSeconds => Mathf.Max(0f, Mathf.Max(0.05f, hatchDuration) - elapsed);
        public bool HasValidUnitPrefab => unitPrefab != null && unitPrefab.enabled
            && unitPrefab.gameObject.activeSelf && unitPrefab.GetComponent<Collider2D>() != null
            && unitPrefab.GetComponent<Collider2D>().enabled
            && unitPrefab.GetComponentInChildren<SpriteRenderer>() != null
            && unitPrefab.GetComponent<SortingGroup>() != null;

        public void Initialize(GameState state)
        {
            gameState = state;
        }

        private void Start()
        {
            if (gameState == null || !HasValidUnitPrefab)
            {
                Debug.LogError("EggHatch needs GameState and an active Unit prefab with UnitActor, "
                    + "Collider2D, SpriteRenderer and SortingGroup.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (HasHatched || gameState == null || !gameState.isActiveAndEnabled)
                return;

            elapsed += Time.deltaTime;
            if (RemainingSeconds <= 0f)
                Hatch();
        }

        private void Hatch()
        {
            if (HasHatched)
                return;

            // Set before spawning: repeated calls cannot spawn twice or add Population twice.
            HasHatched = true;
            UnitActor unit = Instantiate(unitPrefab, transform.position + unitSpawnOffset, Quaternion.identity);
            unit.SetInteractionLocked(false);
            unit.AssignTo(null);
            gameState.AddPopulation(1);
            unit.name = $"Unit {gameState.Population}";
            Destroy(gameObject);
        }

        private void LateUpdate()
        {
            if (countdownLabel != null)
                countdownLabel.text = $"EGG {RemainingSeconds:0.0}s";
        }
        // Disabling pauses incubation. Re-enabling keeps elapsed time and the one-shot guard.
    }
}
