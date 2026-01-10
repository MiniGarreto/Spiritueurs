using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Audio")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = true;
    public bool playOnAwake = true;
    public float fadeDuration = 1f;

    AudioSource src;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            src = GetComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            src.volume = volume;
            if (musicClip != null) src.clip = musicClip;
            if (playOnAwake) Play(fade: false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play(AudioClip clip = null, bool fade = false)
    {
        if (clip != null) src.clip = clip;
        if (src.clip == null) return;
        StopAllCoroutines();
        if (fade)
            StartCoroutine(FadeInCoroutine(fadeDuration));
        else
        {
            src.volume = volume;
            if (!src.isPlaying) src.Play();
        }
    }

    public void Stop(bool fade = false)
    {
        if (fade)
            StartCoroutine(FadeOutCoroutine(fadeDuration));
        else
            src.Stop();
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (src != null) src.volume = volume;
    }

    IEnumerator FadeInCoroutine(float duration)
    {
        src.volume = 0f;
        if (!src.isPlaying) src.Play();
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, volume, t / duration);
            yield return null;
        }
        src.volume = volume;
    }

    IEnumerator FadeOutCoroutine(float duration)
    {
        float startVol = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        src.Stop();
        src.volume = volume;
    }
}