using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Footsteps : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] woodFloorFootsteps = null;
    [SerializeField]
    private AudioClip[] grassFootsteps = null;
    [SerializeField]
    private AudioSource audioSource = null;

    private float footStepDelay = 0.70f;
    private float nextFootstep = 0;
    private int currentClip = 0;
    private int nextClip;
    public bool outside = true;

    void Update()
    {
        if (PlayerController.Player.Running)
            footStepDelay = 0.35f;
        else if (footStepDelay != 0.70f)
            footStepDelay = 0.70f;

        if (outside && SceneManager.GetActiveScene().name == "MainScene")
            outside = false;

        if (PlayerController.Player.CanMove && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W)))
        {
            nextFootstep -= Time.deltaTime;
            if (nextFootstep <= 0)
            {
                if (!outside)
                    audioSource.PlayOneShot(woodFloorFootsteps[currentClip], 0.1f);
                else
                    audioSource.PlayOneShot(grassFootsteps[currentClip], 0.8f);
                nextFootstep += footStepDelay;
            }

            if (!outside)
            {
                nextClip = Random.Range(0, woodFloorFootsteps.Length);
                while (nextClip == currentClip)
                    nextClip = Random.Range(0, woodFloorFootsteps.Length);
            }
            else
            {
                nextClip = Random.Range(0, grassFootsteps.Length);
                while (nextClip == currentClip)
                    nextClip = Random.Range(0, grassFootsteps.Length);
            }

            currentClip = nextClip;
        }
    }
}