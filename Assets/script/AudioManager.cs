using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            // Définir loop à true spécifiquement pour les musiques de fond
            if (s.name == "MainMenu" || s.name == "Backgroundm")
            {
                s.source.loop = true;
            }
            else
            {
                s.source.loop = s.loop;
            }
            s.source.outputAudioMixerGroup = s.mixer;
        }
    }

    void Start()
    {
        PlayMusicBasedOnScene(); // Gérer la musique en fonction de la scène active
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicBasedOnScene(); // Gérer la musique après le chargement de la nouvelle scène
    }

    public void Play(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Son introuvable : " + sound);
            return;
        }

        // Réduction du volume si le son est "Level" ou "GameOver"
        if (sound == "level" || sound == "GameOver")
        {
            FadeBackgroundMusic(0.2f, 0.5f); // Réduire à 20% en 0.5 secondes
            StartCoroutine(RestoreBackgroundVolume(1f, 0.5f)); // Restaurer après 1 seconde
        }

        s.source.Play();
    }

    public void Stop(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Son introuvable : " + sound);
            return;
        }
        s.source.Stop();
    }

    public void PlayBackgroundMusic(string musicName)
    {
        // Arrête toutes les musiques en cours
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying && (s.name == "MainMenu" || s.name == "Backgroundm"))
            {
                s.source.Stop();
            }
        }

        // Trouve la musique à jouer
        Sound musicToPlay = Array.Find(sounds, item => item.name == musicName);
        if (musicToPlay == null)
        {
            Debug.LogWarning("Musique introuvable : " + musicName);
            return;
        }

        // S'assure que la musique est en mode boucle
        musicToPlay.source.loop = true;
        
        // Joue la musique demandée
        Play(musicName);
    }

    public void PlayMusicBasedOnScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex == 0)
        {
            // Jouer la musique du menu principal
            PlayBackgroundMusic("MainMenu");
        }
        else
        {
            // Jouer la musique de gameplay pour les autres scènes
            PlayBackgroundMusic("Backgroundm");
        }
    }

    public void FadeBackgroundMusic(float targetVolume, float duration)
    {
        Sound bgMusic = Array.Find(sounds, item => item.name == "Backgroundm");
        if (bgMusic == null) return;

        StartCoroutine(FadeVolume(bgMusic.source, targetVolume, duration));
    }

    private IEnumerator FadeVolume(AudioSource audioSource, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private IEnumerator RestoreBackgroundVolume(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        FadeBackgroundMusic(1f, duration); // Restaurer à 100% du volume
    }
}