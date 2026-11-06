using System;
using UnityEngine;

namespace EnchantedForest
{
    /// <summary>
    /// Generates all sounds at runtime — no .wav assets needed.
    /// A short ambient hum loops under the run; collect/hit are one-shots.
    /// </summary>
    public class ProceduralAudio : MonoBehaviour
    {
        public static ProceduralAudio Instance { get; private set; }

        AudioSource humSrc, sfxSrc;
        AudioClip humClip;
        float humTimer;
        bool humRunning;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            humSrc = gameObject.AddComponent<AudioSource>();
            humSrc.loop = true;
            humSrc.playOnAwake = false;
            humSrc.volume = 0.12f;
            sfxSrc = gameObject.AddComponent<AudioSource>();
            sfxSrc.playOnAwake = false;
            sfxSrc.volume = 0.5f;
            humClip = MakeSineSweep(2.0f, 90f, 130f);   // 2s low drone loop
        }

        public void StartHum()
        {
            if (!humRunning && humClip != null)
            {
                humSrc.clip = humClip;
                humSrc.Play();
                humRunning = true;
            }
        }

        public void StopHum()
        {
            humSrc.Stop();
            humRunning = false;
        }

        public void PlayCollect()
        {
            sfxSrc.PlayOneShot(MakeSineSweep(0.18f, 660f, 1320f));
        }

        public void PlayHit()
        {
            sfxSrc.PlayOneShot(MakeNoiseBurst(0.35f, 400f));
        }

        static AudioClip MakeSineSweep(float dur, float startHz, float endHz)
        {
            int sr = 22050;
            int n = Mathf.Max(1, Mathf.RoundToInt(dur * sr));
            var data = new float[n];
            float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float f = Mathf.Lerp(startHz, endHz, i / (float)n);
                ph += 2f * Mathf.PI * f / sr;
                float env = Mathf.Sin(Mathf.PI * i / n);            // fade in/out
                data[i] = Mathf.Sin(ph) * env * 0.9f;
            }
            return ToClip(data, sr, "sine" + startHz);
        }

        static AudioClip MakeNoiseBurst(float dur, float lowpass)
        {
            int sr = 22050;
            int n = Mathf.RoundToInt(dur * sr);
            var data = new float[n];
            float last = 0f;
            var rnd = new System.Random(1234);
            for (int i = 0; i < n; i++)
            {
                float white = (float)(rnd.NextDouble() * 2 - 1);
                last = last + 0.06f * (white - last);                // one-pole LP
                float env = 1f - i / (float)n;
                data[i] = last * env * 2f;
            }
            return ToClip(data, sr, "noise");
        }

        static AudioClip ToClip(float[] data, int sr, string tag)
        {
            var clip = AudioClip.Create(tag, data.Length, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
