using UnityEngine;
using UnityEngine.InputSystem;

namespace EnchantedForest
{
    /// <summary>
    /// Controls the player avatar: lane-based horizontal movement, jumping,
    /// and forward scrolling. Attaches to the player GameObject.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class RunnerController : MonoBehaviour
    {
        public float laneMin = -3f, laneMax = 3f, laneStep = 3f;
        public float laneLerp = 9f, maxLaneVel = 12f;
        public float jumpVelocity = 8.2f;
        public int playerLayer = 0;

        CharacterController cc;
        float laneTarget = 0f, verticalVel;

        public float LaneTarget => laneTarget;

        public GameObject Body;       // visual root (capsule + head + ears)
        public TrailRenderer RuneTrail;
        public FlashLight Blinker;    // used to flash while invulnerable

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) playerLayer = 0;
            if (transform.TryGetComponent(out Blinker)) Blinker.target = gameObject;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameManager.RunState.Playing)
            {
                if (GameManager.Instance != null && GameManager.Instance.State == GameManager.RunState.GameOver)
                    if (Keyboard.current?.rKey.wasPressedThisFrame == true)
                        GameManager.Instance.Restart();
                return;
            }

            // ---- input ----
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame)
                    laneTarget = Mathf.Max(laneMin, laneTarget - laneStep);
                if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame)
                    laneTarget = Mathf.Min(laneMax, laneTarget + laneStep);
                if ((k.spaceKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) && cc.isGrounded)
                    verticalVel = jumpVelocity;
            }

            // ---- movement ----
            verticalVel += Physics.gravity.y * Time.deltaTime;
            var pos = transform.position;
            float sideVel = Mathf.Clamp((laneTarget - pos.x) * laneLerp, -maxLaneVel, maxLaneVel);
            cc.Move(new Vector3(sideVel, verticalVel, GameManager.Instance.CurrentSpeed) * Time.deltaTime);
            if (cc.isGrounded && verticalVel < 0f) verticalVel = -1f;

            // ---- invulnerability blink (only hides the player while invulnerable) ----
            bool show = true;
            if (GameManager.Instance.IsInvulnerable)
                show = Mathf.PingPong(Time.time, 0.12f) > 0.06f;
            Blinker?.SetVisible(show);
        }

        /// <summary>Wired to the CharacterController's collision by the runner.</summary>
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.TryGetComponent<Obstacle>(out _))
            {
                GameManager.Instance?.HitObstacle();
                if (GameManager.Instance != null)
                {
                    ProceduralAudio.Instance?.PlayHit();
                    WorldFX.Instance?.SpawnImpact(transform.position);
                }
            }
        }

        /// <summary>Collect a firefly (called by the firefly's trigger).</summary>
        public void Collect()
        {
            if (GameManager.Instance.IsInvulnerable) return;
            GameManager.Instance.CollectFirefly();
            ProceduralAudio.Instance?.PlayCollect();
            WorldFX.Instance?.SpawnCollect(transform.position + Vector3.up);
        }
    }

    /// <summary>Turns the visual child objects on/off for the hit blink effect.</summary>
    public class FlashLight : MonoBehaviour
    {
        public GameObject target;
        Renderer[] r;
        public void SetVisible(bool on)
        {
            if (r == null) r = target.GetComponentsInChildren<Renderer>();
            foreach (var renderer in r) renderer.enabled = on;
        }
    }
}
