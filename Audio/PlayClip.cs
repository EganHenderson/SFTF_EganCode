using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayClip : MonoBehaviour
{
    private int nextClip, currentClip = 0;

    private static PlayClip audioPlayer;
    public static PlayClip AudioPlayer
    {
        get
        {
            if (audioPlayer == null)
            {
                audioPlayer = FindObjectOfType<PlayClip>();
            }

            return audioPlayer;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField]
    private AudioSource oneShotSource = null;

    public void PlayOneShot(AudioClip clip, float volume = 1.0f, bool playWhenPaused = false)
    {
        oneShotSource.ignoreListenerPause = playWhenPaused;

        if (oneShotSource && clip)
            oneShotSource.PlayOneShot(clip, volume);
    }

    public void PlayRandomClip(AudioClip[] clips, float volume = 1.0f, bool playWhenPaused = false)
    {
        oneShotSource.ignoreListenerPause = playWhenPaused;

        if (oneShotSource && clips.Length > 1)
        {
            nextClip = Random.Range(0, clips.Length);
            while (nextClip == currentClip)
                nextClip = Random.Range(0, clips.Length);

            currentClip = nextClip;
            oneShotSource.PlayOneShot(clips[currentClip]);
        }

        else if (oneShotSource)
            oneShotSource.PlayOneShot(clips[0]);
    }
}
