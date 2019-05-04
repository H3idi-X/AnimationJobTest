using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace jp.geometry
{
    // http://kan-kikuchi.hatenablog.com/entry/PerlinNoise_Anime
    public class WindGenerator : MonoBehaviour
    {
        private Vector3 seed = Vector3.zero;
        private float rateOfShaking = 1.0f;
        private float scaleOfShaking = 12;
        private Vector3 xyzScale = Vector3.one;
        public Vector3 result;
        // Start is called before the first frame update
        void Start()
        {
            seed = new Vector3(Random.Range(0f, 100f), Random.Range(0f, 100f), Random.Range(0f, 100f));

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 bigNoize =
                CreateVector3Noise(ratio: 0.8f, frequencyRate: rateOfShaking * 0.4f);
            Vector3 smallNoize =
                CreateVector3Noise(ratio: 0.1f, frequencyRate: rateOfShaking);
            result = (bigNoize + smallNoize) * scaleOfShaking;
        }

        private Vector3 CreateVector3Noise(float ratio, float frequencyRate)
        {
            float frequency = Time.time * frequencyRate;

            return new Vector3(
              (Mathf.PerlinNoise(frequency, seed.x) - 0.5f) * ratio * xyzScale.x,
              (Mathf.PerlinNoise(frequency, seed.y) - 0.5f) * ratio * xyzScale.y,
              (Mathf.PerlinNoise(frequency, seed.z) - 0.5f) * ratio * xyzScale.z
            );
        }
    }
}