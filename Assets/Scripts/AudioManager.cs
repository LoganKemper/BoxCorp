using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxCorp
{
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        [SerializeField] private AudioSource _audioSourcePrefab;
        [SerializeField] private int _initialPoolSize = 10;

        private readonly Queue<AudioSource> _pool = new();

        protected override void Awake()
        {
            base.Awake();

            // Pre-warm pool.
            for (int i = 0; i < _initialPoolSize; i++)
            {
                _pool.Enqueue(CreateNew());
            }
        }

        public AudioSource PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            return Play(clip, position, 1f, volume, pitch);
        }

        public AudioSource PlaySFX2D(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            return Play(clip, Vector3.zero, 0f, volume, pitch);
        }

        private AudioSource Play(AudioClip clip, Vector3 position, float spatialBlend, float volume, float pitch)
        {
            if (clip == null)
            {
                Debug.LogWarning($"[{nameof(AudioManager)}] Tried to play a null AudioClip.");
                return null;
            }

            AudioSource source = (_pool.Count > 0) ? _pool.Dequeue() : CreateNew();

            source.transform.position = position;
            source.clip = clip;
            source.spatialBlend = spatialBlend;
            source.volume = volume;
            source.pitch = pitch;

            source.gameObject.SetActive(true);
            source.Play();

            StartCoroutine(ReturnWhenDone(source));
            return source;
        }

        private IEnumerator ReturnWhenDone(AudioSource source)
        {
            float duration = source.clip.length / Mathf.Max(source.pitch, 0.01f);
            yield return new WaitForSeconds(duration);

            while (source.isPlaying)
            {
                yield return null;
            }

            Return(source);
        }

        private void Return(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            source.transform.SetParent(transform, false);
            _pool.Enqueue(source);
        }

        private AudioSource CreateNew()
        {
            AudioSource source = Instantiate(_audioSourcePrefab, transform);
            source.gameObject.SetActive(false);
            return source;
        }
    }
}
