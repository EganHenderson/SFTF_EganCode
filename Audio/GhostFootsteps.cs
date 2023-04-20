using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostFootsteps : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] footsteps = null;
    [SerializeField]
    private AudioSource audioSource = null;

    [SerializeField] private float footStepDelay = 0.45f;
    private float nextFootstep = 0;
    private int currentClip = 0;
    private int nextClip;

    private Transform ghostBody = null;
    private Vector3 lastPosition;

    private void Start()
    {
        ghostBody = GetComponent<Transform>();
        lastPosition = ghostBody.position;

        if (footsteps.Length > 1)
            StartCoroutine(SecondStep());
    }

    void Update()
    {
        if (ghostBody.position != lastPosition)
        {
            nextFootstep -= Time.deltaTime;
            if (nextFootstep <= 0)
            {
                audioSource.PlayOneShot(footsteps[currentClip], 1.0f);
                nextFootstep += footStepDelay;
            }

            nextClip = Random.Range(0, footsteps.Length);
            while (nextClip == currentClip)
                nextClip = Random.Range(0, footsteps.Length);

            currentClip = nextClip;
        }

        lastPosition = ghostBody.position;
    }

    IEnumerator SecondStep()
    {
        yield return new WaitForSeconds(footStepDelay / 2.0f);

        if (ghostBody.position != lastPosition)
            audioSource.PlayOneShot(footsteps[currentClip == 0 ? footsteps.Length - 1 : 0]);
    }
}