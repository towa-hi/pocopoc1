using System.Linq;
using UnityEngine;

namespace Environment.Utilities
{
    public class WeightedDistribution
    {
        // Could add a seed to random if ever needed
        readonly float[] cumulativeProbabilities;
        public WeightedDistribution(float[] weights)
        {
            if (weights == null || weights.Length == 0) Debug.LogError("Cannot create WeightedDistribution with a null or empty input!");
            var sum = weights.Sum();
            cumulativeProbabilities = new float[weights.Length];
            var cumulativeProbability = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulativeProbability += weights[i] / sum;
                cumulativeProbabilities[i] = cumulativeProbability;
            }
        }
        public int Sample() // returns index of weight given in constructor
        {
            if (cumulativeProbabilities.Length == 1) return 0;
            var random = Random.value;
            for (int i = 0; i < cumulativeProbabilities.Length; i++)
            {
                if(random <= cumulativeProbabilities[i])
                {
                    return i;
                }
            }
            Debug.LogWarning("Undefined behaviour! Defaulting to last element.");
            return cumulativeProbabilities.Length - 1;
        }
    }
}
