using UnityEngine;

namespace EnchantedForest
{
    /// <summary>Marker on solid obstacles. Hitting one costs a life.</summary>
    public class Obstacle : MonoBehaviour
    {
    }

    /// <summary>A collectible spirit firefly. Trigger collider + spin + bob.</summary>
    public class FireflyPickup : MonoBehaviour
    {
        public float spinPerSec = 110f;
        public float bobAmp = 0.15f, bobSpeed = 2.5f;
        float t;
        Vector3 basePos;

        void Awake() => basePos = transform.position;

        void Update()
        {
            t += Time.deltaTime;
            transform.Rotate(0f, spinPerSec * Time.deltaTime, 0f);
            transform.position = basePos + Vector3.up * Mathf.Sin(t * bobSpeed) * bobAmp;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<RunnerController>(out var runner))
            {
                runner.Collect();
                Destroy(gameObject);
            }
        }
    }
}
