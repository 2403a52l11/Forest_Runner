using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace EnchantedForest
{
    /// <summary>
    /// Canvas-based HUD built entirely in code: distance readout, life hearts,
    /// a start card, and a game-over card with a Retry button.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        Text distanceText, titleText, gameoverText, titleSub, overSub;
        CanvasGroup titleCard, overCard;
        RectTransform heartRoot;
        int shownLives = -1;
        Font font;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvas();
            GameManager.OnScoreChanged += s => distanceText.text = $"DISTANCE {s}";
            GameManager.OnLivesChanged += l => DrawHearts(l);
            GameManager.OnStateChanged += s =>
            {
                bool over = s == GameManager.RunState.GameOver;
                titleCard.alpha = s == GameManager.RunState.Idle ? 1f : 0f;
                overCard.alpha = over ? 1f : 0f;
                overCard.interactive = over;
                overCard.blocksRaycasts = over;
            };
        }

        void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.State == GameManager.RunState.Idle)
                if ((Keyboard.current?.spaceKey.wasPressedThisFrame == true) ||
                    ((Mouse.current?.leftButton.wasPressedThisFrame == true) && titleCard.alpha > 0.5f))
                {
                    StartRun();
                }
        }

        void StartRun()
        {
            GameManager.Instance.StartRun();
            ProceduralAudio.Instance?.StartHum();
            WorldFX.Instance?.StartAmbient();
        }

        void BuildCanvas()
        {
            var canvasGO = new GameObject("HUD Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            var rt = canvasGO.GetComponent<RectTransform>();

            // ---- top-left status panel ----
            var panel = MakePanel(rt, "StatusPanel", new Vector2(0, 1), new Vector2(36, -40), new Vector2(460, 130));
            var title = MakeLabel(panel, "StatusTitle", "ENCHANTED FOREST RUNNER", 24, new Color(.7f, 1f, .8f));
            Place(title, new Vector2(0, 1), new Vector2(16, 14), new Vector2(420, 40));
            distanceText = MakeLabel(panel, "Distance", "DISTANCE 0", 22, Color.white);
            Place(distanceText, new Vector2(0, 0), new Vector2(16, 14), new Vector2(420, 40));

            // ---- top-right hearts ----
            heartRoot = MakePanel(rt, "Hearts", new Vector2(1, 1), new Vector2(-30, -40), new Vector2(460, 110)).transform as RectTransform;

            // ---- center start card ----
            titleCard = MakeCenterCard(rt, "TitleCard", out titleText, out titleSub);
            titleText.text = "ENCHANTED FOREST RUNNER";
            titleText.fontSize = 52;
            titleSub.text = "Arrow keys / A D to change lane • Space to jump\n\nClick or press SPACE to begin";
            titleSub.alignment = TextAnchor.MiddleCenter;

            // ---- center game-over card ----
            overCard = MakeCenterCard(rt, "GameOverCard", out gameoverText, out overSub);
            gameoverText.text = "THE FOREST CLAIMED YOU";
            gameoverText.fontSize = 46;
            overSub.text = "Press R or click RUN AGAIN to retry";
            overSub.alignment = TextAnchor.MiddleCenter;
            var retry = MakeButton(overCard.transform, "RetryBtn", "RUN AGAIN");
            retry.onClick.AddListener(() => GameManager.Instance.Restart());

            overCard.alpha = 0f; overCard.interactive = false;
            DrawHearts(GameManager.Instance.Lives);
        }

        // ---------- primitives ----------

        RectTransform MakePanel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = anchor;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.35f);
            img.raycastTarget = false;
            return r;
        }

        Text MakeLabel(Transform parent, string name, string text, int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var label = go.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        static void Place(Component c, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var r = (RectTransform)c.transform;
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = anchor;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
        }

        CanvasGroup MakeCenterCard(Transform parent, string name, out Text big, out Text sub)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.pivot = Vector2.one * 0.5f;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.06f, 0.10f, 0.80f);
            var cg = go.AddComponent<CanvasGroup>();

            big = MakeLabel(go.transform, "Big", "", 48, new Color(.7f, 1f, .8f));
            big.fontStyle = FontStyle.Bold;
            Place(big, new Vector2(0, 1), new Vector2(0, 220), new Vector2(1200, 90));
            big.alignment = TextAnchor.MiddleCenter;

            sub = MakeLabel(go.transform, "Sub", "", 24, Color.white);
            Place(sub, new Vector2(0, 1), new Vector2(0, 100), new Vector2(1200, 120));
            sub.alignment = TextAnchor.MiddleCenter;

            return cg;
        }

        Button MakeButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0);
            r.anchorMax = new Vector2(0.5f, 0);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0, 150);
            r.sizeDelta = new Vector2(340, 74);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(.15f, .45f, .4f, 1f);
            var btn = go.AddComponent<Button>();
            var t = MakeLabel(go.transform, "Label", label, 30, Color.white);
            Place(t, Vector2.zero, Vector2.zero, Vector2.zero);
            t.rectTransform.anchorMin = Vector2.zero;
            t.rectTransform.anchorMax = Vector2.one;
            t.alignment = TextAnchor.MiddleCenter;
            return btn;
        }

        void DrawHearts(int lives)
        {
            if (heartRoot == null) return;
            if (lives == shownLives) return;
            shownLives = lives;
            for (int i = heartRoot.childCount - 1; i >= 0; i--)
                Destroy(heartRoot.GetChild(i).gameObject);
            for (int i = 0; i < GameManager.Instance.maxLives; i++)
            {
                var h = MakeLabel(heartRoot, "Heart" + i, i < lives ? "♥" : "♡", 44,
                                  i < lives ? new Color(1f, .35f, .5f, 1f) : new Color(1, 1, 1, 0.22f));
                Place(h, new Vector2(0, 1), new Vector2(i * 54, -8), new Vector2(54, 54));
                h.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}
