using UnityEngine;

namespace EnchantedForest
{
    /// <summary>
    /// Owns all game rules: distance, score, lives, and the run state.
    /// A single singleton created at runtime by the bootstrap.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum RunState { Idle, Playing, GameOver }

        public static GameManager Instance { get; private set; }

        [Header("Balance")]
        public float baseSpeed = 12f;
        public float speedRampPerDistance = 0.012f;
        public int maxLives = 3;
        public int fireflyScore = 25;
        public float comboWindow = 4f;
        public int comboStep = 5;
        public float invulnerabilityTime = 1.5f;

        public RunState State { get; private set; } = RunState.Idle;
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public float Distance { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool IsInvulnerable { get; private set; }

        int combo;
        float comboTimer;

        public static event System.Action<int> OnLivesChanged;
        public static event System.Action<int> OnScoreChanged;
        public static event System.Action<RunState> OnStateChanged;
        public static event System.Action<float> OnHitFlash;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Application.targetFrameRate = 60;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.018f, .055f, .09f);
            RenderSettings.fogDensity = 0.026f;
            RenderSettings.ambientLight = new Color(.09f, .17f, .28f);
            Lives = maxLives;
        }

        public void StartRun()
        {
            Score = 0; Distance = 0; Lives = maxLives;
            State = RunState.Playing;
            OnStateChanged?.Invoke(State);
            OnScoreChanged?.Invoke(Score);
            OnLivesChanged?.Invoke(Lives);
        }

        /// <summary>Called each frame while playing to advance distance/speed and combo decay.</summary>
        public void UpdateDistance(float dt)
        {
            if (State != RunState.Playing) return;
            CurrentSpeed = baseSpeed + Distance * speedRampPerDistance;
            Distance += CurrentSpeed * dt;
            Score = Mathf.FloorToInt(Distance);
            OnScoreChanged?.Invoke(Score);

            comboTimer -= dt;
            if (comboTimer <= 0f) combo = 0;
        }

        public void CollectFirefly()
        {
            if (State != RunState.Playing) return;
            combo++;
            comboTimer = comboWindow;
            int gained = fireflyScore + (combo - 1) * comboStep;
            Score += gained;
            OnScoreChanged?.Invoke(Score);
        }

        public void HitObstacle()
        {
            if (State != RunState.Playing || IsInvulnerable) return;
            Lives--;
            combo = 0;
            comboTimer = 0f;
            OnLivesChanged?.Invoke(Lives);
            OnHitFlash?.Invoke(0.5f);
            if (Lives <= 0)
            {
                State = RunState.GameOver;
                OnStateChanged?.Invoke(State);
            }
            else
            {
                IsInvulnerable = true;
                Invoke(nameof(ClearInvulnerable), invulnerabilityTime);
            }
        }

        void ClearInvulnerable() => IsInvulnerable = false;

        public void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
