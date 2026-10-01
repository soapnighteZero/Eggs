using System;
using UnityEngine;

namespace Eggs.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class GameState : MonoBehaviour
    {
        [Min(0)] [SerializeField] private int startingFood = 10;
        [Min(0)] [SerializeField] private int startingPopulation = 4;

        public int Food { get; private set; }
        public int Population { get; private set; }

        private void Awake()
        {
            Food = Math.Max(0, startingFood);
            Population = Math.Max(0, startingPopulation);
        }

        public void AddFood(int amount)
        {
            if (amount > 0)
                Food = (int)Math.Min(int.MaxValue, (long)Food + amount);
        }

        public bool TrySpendFood(int amount)
        {
            if (amount < 0 || amount > Food)
                return false;

            Food -= amount;
            return true;
        }

        public void AddPopulation(int amount)
        {
            if (amount > 0)
                Population = (int)Math.Min(int.MaxValue, (long)Population + amount);
        }

        private void OnValidate()
        {
            startingFood = Math.Max(0, startingFood);
            startingPopulation = Math.Max(0, startingPopulation);
        }
    }
}
