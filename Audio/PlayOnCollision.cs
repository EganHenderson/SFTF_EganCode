using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayOnCollision : MonoBehaviour
{
    [Tooltip("The audio clips to play on collision. Plays one at random from the array.")]
    [SerializeField]
    private AudioClip[] clips = null;

    private int currentClip = -1, nextClip;
    private AudioSource source = null;

    private void Start()
    {
        if (!source)
            source = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (clips.Length > 0)
                PlayRandomClip();
    }

    private void PlayRandomClip()
    {
        if (clips.Length > 1)
        {
            nextClip = Random.Range(0, clips.Length);
            while (nextClip == currentClip)
                nextClip = Random.Range(0, clips.Length);

            currentClip = nextClip;
            source.PlayOneShot(clips[currentClip]);
        }

        else
            source.PlayOneShot(clips[0]);
    }
}
