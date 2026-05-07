using UnityEngine;

namespace EvoTowers.Task1
{
    public class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;

        private AudioSource source;
        private float lastAttackTime;
        private float lastHitTime;

        public float Volume
        {
            get => volume;
            set
            {
                volume = Mathf.Clamp01(value);
                if (source != null)
                {
                    source.volume = volume;
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = volume;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PlayButton() => PlayTone(720f, 0.045f, 0.18f);
        public void PlayBuild() => PlayTone(520f, 0.08f, 0.28f);
        public void PlayDeath() => PlayTone(180f, 0.12f, 0.24f);
        public void PlayWave() => PlayTone(440f, 0.16f, 0.32f);
        public void PlayBoss() => PlayTone(95f, 0.28f, 0.42f);
        public void PlayVictory() => PlayTone(880f, 0.26f, 0.38f);
        public void PlayFailure() => PlayTone(130f, 0.35f, 0.36f);
        public void PlayEvolve() => PlayTone(760f, 0.16f, 0.34f);

        public void PlayAttack()
        {
            if (Time.unscaledTime - lastAttackTime < 0.08f)
            {
                return;
            }

            lastAttackTime = Time.unscaledTime;
            PlayTone(Random.Range(360f, 440f), 0.035f, 0.08f);
        }

        public void PlayHit()
        {
            if (Time.unscaledTime - lastHitTime < 0.06f)
            {
                return;
            }

            lastHitTime = Time.unscaledTime;
            PlayTone(Random.Range(240f, 300f), 0.025f, 0.06f);
        }

        private void PlayTone(float frequency, float duration, float gain)
        {
            if (source == null || volume <= 0f)
            {
                return;
            }

            source.PlayOneShot(CreateTone(frequency, duration), gain);
        }

        private static AudioClip CreateTone(float frequency, float duration)
        {
            int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - i / (float)sampleCount;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
            }

            AudioClip clip = AudioClip.Create("GeneratedTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
