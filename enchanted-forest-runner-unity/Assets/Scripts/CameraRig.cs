using UnityEngine;

namespace EnchantedForest
{
    /// <summary>Follows the runner with smoothing + a subtle impact shake on hit.</summary>
    public class CameraRig : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 4.8f, -10.5f);
        public Vector3 lookAhead = new Vector3(0, 1.3f, 8f);
        public float followSmooth = 6f;
        public float shakeAmount = 0.4f;
        public float shakeDecay = 8f;

        Camera cam;
        float shake;

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.025f, .08f, .12f);
            cam.fieldOfView = 65;
        }

        void Update()
        {
            if (target == null) return;
            shake = Mathf.Lerp(shake, 0f, shakeDecay * Time.deltaTime);
            var wanted = target.position + offset;
            var pos = Vector3.Lerp(transform.position, wanted, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
            var jitter = Random.insideUnitSphere * shake;
            transform.position = pos + jitter;
            transform.rotation = Quaternion.LookRotation((target.position + lookAhead) - pos + jitter);
        }

        public void Shake(float amount) => shake = Mathf.Max(shake, amount);
    }
}
