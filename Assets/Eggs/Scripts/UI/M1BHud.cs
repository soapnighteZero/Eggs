using UnityEngine;

namespace Eggs.Gameplay
{
    public sealed class M1BHud : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private TextMesh foodValue;
        [SerializeField] private TextMesh populationValue;

        private void Start()
        {
            if (gameState == null || foodValue == null || populationValue == null)
            {
                Debug.LogError("M1BHud needs GameState and both numeric TextMesh references.", this);
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (gameState == null)
                return;
            if (foodValue != null)
                foodValue.text = gameState.Food.ToString();
            if (populationValue != null)
                populationValue.text = gameState.Population.ToString();
        }
    }
}
