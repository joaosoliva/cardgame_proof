using UnityEngine;

namespace CardgameProof.Core
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        public bool IsMuted { get; private set; }

        private AudioSource sfxSource;
        private AudioSource bgSource;

        [Header("Optional authored clips (fallback to procedural if null)")]
        [SerializeField] private AudioClip buttonClip;
        [SerializeField] private AudioClip pickClip;
        [SerializeField] private AudioClip placeClip;
        [SerializeField] private AudioClip invalidClip;
        [SerializeField] private AudioClip revealClip;
        [SerializeField] private AudioClip clueClip;
        [SerializeField] private AudioClip researchClip;
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip wrongClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip backgroundClip;

        public static AudioManager EnsureInstance()
        {
            if (Instance != null) return Instance;
            AudioManager existing = FindFirstObjectByType<AudioManager>();
            if (existing != null) return existing;
            GameObject go = new GameObject("AudioManager");
            return go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            bgSource = gameObject.AddComponent<AudioSource>();
            bgSource.playOnAwake = false;
            bgSource.loop = true;
            bgSource.volume = 0.08f;
            BuildFallbackClipsIfMissing();
            StartBackgroundLoop();
        }

        public void ToggleMute() { IsMuted = !IsMuted; sfxSource.mute = IsMuted; bgSource.mute = IsMuted; }
        public void PlayButton() => PlayOneShot(buttonClip);
        public void PlayCardPick() => PlayOneShot(pickClip);
        public void PlayCardPlace() => PlayOneShot(placeClip);
        public void PlayInvalid() => PlayOneShot(invalidClip);
        public void PlayReveal() => PlayOneShot(revealClip);
        public void PlayClue() => PlayOneShot(clueClip);
        public void PlayResearch() => PlayOneShot(researchClip);
        public void PlayCorrect() => PlayOneShot(correctClip);
        public void PlayWrong() => PlayOneShot(wrongClip);
        public void PlayWin() => PlayOneShot(winClip);

        private void StartBackgroundLoop()
        {
            if (backgroundClip == null) return;
            bgSource.clip = backgroundClip;
            bgSource.Play();
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (IsMuted || clip == null) return;
            sfxSource.PlayOneShot(clip, 0.8f);
        }

        private void BuildFallbackClipsIfMissing()
        {
            if (buttonClip == null) buttonClip = MakeTone(720f, 0.07f);
            if (pickClip == null) pickClip = MakeTone(480f, 0.06f);
            if (placeClip == null) placeClip = MakeTone(540f, 0.08f);
            if (invalidClip == null) invalidClip = MakeTone(180f, 0.12f);
            if (revealClip == null) revealClip = MakeTone(640f, 0.12f);
            if (clueClip == null) clueClip = MakeTone(760f, 0.11f);
            if (researchClip == null) researchClip = MakeTone(420f, 0.11f);
            if (correctClip == null) correctClip = MakeTone(940f, 0.14f);
            if (wrongClip == null) wrongClip = MakeTone(220f, 0.14f);
            if (winClip == null) winClip = MakeTone(1040f, 0.22f);
            if (backgroundClip == null) backgroundClip = MakeSoftBackground();
        }

        private static AudioClip MakeTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Clamp01(1f - (i / (float)sampleCount));
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.18f * envelope;
            }
            AudioClip clip = AudioClip.Create($"tone_{frequency}", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeSoftBackground()
        {
            int sampleRate = 44100;
            float duration = 2.5f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.03f + Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.015f;
                data[i] = wave;
            }
            AudioClip clip = AudioClip.Create("bg_loop", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
