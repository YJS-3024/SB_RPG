using System.Collections.Generic;
using UnityEngine;

namespace yjs.DevKit
{
    /// <summary>
    /// 확률 판정과 가중치 랜덤 선택에 사용하는 유틸리티입니다.
    /// </summary>
    public static class RandomUtility
    {
        /// <summary>
        /// 0에서 1 사이 확률 값으로 성공 여부를 판정합니다.
        /// </summary>
        public static bool Chance01(float probability)
        {
            return Random.value < Mathf.Clamp01(probability);
        }

        /// <summary>
        /// 0에서 100 사이 퍼센트 값으로 성공 여부를 판정합니다.
        /// </summary>
        public static bool ChancePercent(float percent)
        {
            return Chance01(percent * 0.01f);
        }

        /// <summary>
        /// 최소값과 최대값 사이의 랜덤 부동소수점 값을 반환합니다.
        /// </summary>
        public static float Range(float min, float max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// 최소값과 최대값 사이의 랜덤 정수 값을 반환합니다. 최대값은 포함하지 않습니다.
        /// </summary>
        public static int Range(int min, int max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// -1 또는 1을 랜덤으로 반환합니다.
        /// </summary>
        public static int Sign()
        {
            return Random.value < 0.5f ? -1 : 1;
        }

        /// <summary>
        /// 가중치 목록에서 선택된 인덱스를 반환합니다. 선택할 수 없으면 -1을 반환합니다.
        /// </summary>
        public static int WeightedIndex(IList<float> weights)
        {
            if (weights == null || weights.Count == 0)
                return -1;

            float totalWeight = 0f;
            for (int i = 0; i < weights.Count; i++)
                totalWeight += Mathf.Max(0f, weights[i]);

            if (totalWeight <= 0f)
                return -1;

            float randomValue = Random.value * totalWeight;
            for (int i = 0; i < weights.Count; i++)
            {
                randomValue -= Mathf.Max(0f, weights[i]);
                if (randomValue <= 0f)
                    return i;
            }

            return weights.Count - 1;
        }

        /// <summary>
        /// 값과 가중치 쌍 목록에서 랜덤 값을 선택합니다.
        /// </summary>
        public static T WeightedValue<T>(IList<T> values, IList<float> weights, T defaultValue = default)
        {
            if (values == null || weights == null || values.Count == 0 || values.Count != weights.Count)
                return defaultValue;

            int index = WeightedIndex(weights);
            return index >= 0 ? values[index] : defaultValue;
        }
    }
}
