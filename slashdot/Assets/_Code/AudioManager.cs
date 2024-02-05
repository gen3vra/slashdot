using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public bool muted;
    public AudioSource MenuIntroAudioSource;
    public AudioSource MenuAudioSource;
    public AudioSource MainAudioSource;
    public AudioSource EnemyLaserAudioSource;
    public AudioSource GameOverAudioSource;
    private AudioSource audioSource;
    [Header("Audio Clips")]
    public AudioClip[] shipWoosh;
    public AudioClip shipBreak;
    public AudioClip slashKill;
    public AudioClip kill;
    public AudioClip comboRankUp;
    public AudioClip hit;
    public AudioClip notification;

    public enum SoundType
    {
        shipBreak,
        shipWoosh,
        slashKill,
        kill,
        comboRankUp,
        hit,
        notification,
        enemyLaser
    }
    Coroutine checkMenuMusicCoroutine;
    void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one AudioManager in the scene.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        // Play audioSources to load it
        MainAudioSource.Play();
        MainAudioSource.Pause();
        GameOverAudioSource.Play();
        GameOverAudioSource.Pause();
        checkMenuMusicCoroutine = StartCoroutine(CheckMenuMusic());
    }

    IEnumerator CheckMenuMusic()
    {
        MenuIntroAudioSource.Play();
        while (true)
        {
            if (!MenuAudioSource.isPlaying && MenuIntroAudioSource.time > 9)
            {
                MenuAudioSource.Play();
            }
            yield return null;
        }
    }

    public void PlaySoundOnce(SoundType type)
    {
        if (muted)
            return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.volume = 0.5f;
        switch (type)
        {
            case SoundType.shipBreak:
                audioSource.clip = shipBreak;
                break;
            case SoundType.shipWoosh:
                audioSource.clip = shipWoosh[Random.Range(0, shipWoosh.Length)];
                break;
            case SoundType.slashKill:
                audioSource.volume = 1f;
                audioSource.clip = slashKill;
                break;
            case SoundType.kill:
                audioSource.clip = kill;
                break;
            case SoundType.comboRankUp:
                audioSource.clip = comboRankUp;
                break;
            case SoundType.hit:
                audioSource.clip = hit;
                break;
            case SoundType.notification:
                audioSource.clip = notification;
                break;
            case SoundType.enemyLaser:
                EnemyLaserAudioSource.Play();
                return;

        }
        audioSource.Play();

    }

    public void StartGame()
    {
        if (muted)
            return;

        if (checkMenuMusicCoroutine != null)
            StopCoroutine(checkMenuMusicCoroutine);
        MenuIntroAudioSource.Stop();
        MenuAudioSource.Stop();
        GameOverAudioSource.Pause();
        MainAudioSource.Play();
    }
    public void GameOver()
    {
        if (muted)
            return;

        MainAudioSource.Pause();
        GameOverAudioSource.Play();
    }
    /*
    IEnumerator FadeInGameOverAudio()
    {
        GameOverAudioSource.volume = 0f;
        GameOverAudioSource.UnPause();
        float time = 0f;
        float duration = 3f;
        float startVolume = 0f;
        while (time < duration)
        {
            GameOverAudioSource.volume = Mathf.Lerp(startVolume, 1f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        GameOverAudioSource.volume = 1f;
    }

    IEnumerator FadeInMainAudio()
    {
        MainAudioSource.volume = 0f;
        MainAudioSource.UnPause();
        float time = 0f;
        float duration = 1f;
        float startVolume = 0f;
        while (time < duration)
        {
            MainAudioSource.volume = Mathf.Lerp(startVolume, 1f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        MainAudioSource.volume = 1f;
    }*/
}
