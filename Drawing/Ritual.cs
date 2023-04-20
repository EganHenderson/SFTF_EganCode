using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ritual : MonoBehaviour
{
    private bool hasCreatedSymbol = false;

    [SerializeField]
    private GameObject[] symbolPrefabs = null;
    private GameObject symbolCreated = null;

    [SerializeField]
    private Transform symbolTarget = null, ghost = null;

    [Tooltip("fireClip is the sound to play when the ghost fires a symbol, hitClip is the sound when a symbol hits the canvas, startClip plays upon start of the fight, destroyClip plays when a player destroys the symbol, and winClip plays when Alex is defeated.")]
    [SerializeField]
    private AudioClip fireClip = null, hitClip = null, startClip = null, destroyClip = null, winClip = null;

    [Tooltip("Audio source for the fire clip and the hit clip, respectively.")]
    [SerializeField]
    private AudioSource fireSource = null, hitSource = null;

    [Tooltip("Number of symbols required to lose sanity and to win the game, respectively.")]
    [SerializeField]
    private int symbolCountSanity = 1, symbolCountWin = 6;

    [SerializeField]
    private RedCubeAI alex = null;

    private bool isFighting = false;
    [SerializeField] private int symbolCountDestroyed = 0;
    private int symbolCountHit = 0;

    [Tooltip("Objects to turn on/off after beating alex")]
    [SerializeField]
    private GameObject simonDialogue = null, simonDialogueEnd = null, effects = null, effectsEnd = null, alexModel = null, flickerLight = null, canvasObject = null;
    [SerializeField]
    private DialogueTrigger playerDialogue = null;
    private bool hasTriggeredDialogue = false;

    public bool HasTriggeredDialogue { set { hasTriggeredDialogue = value; } }

    public Interactable canvas;

    public bool IsFighting { get { return isFighting; } set { isFighting = value; } }

    public GameObject[] Symbols { get { return symbolPrefabs; } }
    public GameObject SymbolCreated { get { return symbolCreated; } }

    private static Ritual ritual;
    public static Ritual RitualInstance
    {
        get
        {
            if (!ritual) { ritual = FindObjectOfType<Ritual>(); }
            return ritual;
        }
    }

    public void StartFight()
    {
        if (!isFighting)
        {
            flickerLight.SetActive(false);
            PlayClip.AudioPlayer.PlayOneShot(startClip, 0.5f);
            isFighting = true;
        }
    }

    public void SymbolAttack()
    {
        if (!hasCreatedSymbol)
        {
            hasCreatedSymbol = true;
            fireSource.PlayOneShot(fireClip);
            symbolCreated = Instantiate(symbolPrefabs[Random.Range(0, symbolPrefabs.Length)], ghost.position, Quaternion.identity);
            symbolCreated.GetComponent<Symbol>().SetMove(true, symbolTarget.gameObject);
        }
    }

    public void SymbolDestroyed()
    {
        if (hasCreatedSymbol)
        {
            fireSource.PlayOneShot(destroyClip, 0.5f);
            NotebookController.Instance.ClearPage();
            Destroy(symbolCreated);
            hasCreatedSymbol = false;
            symbolCountDestroyed++;

            if (symbolCountDestroyed >= symbolCountWin)
            {
                isFighting = false;
                WinGame();
            }
        }
    }

    public void SymbolHit()
    {
        if (hasCreatedSymbol)
        {
            Destroy(symbolCreated);
            hasCreatedSymbol = false;
            symbolCountHit++;
        }
    }

    public void HitCanvas()
    {
        hitSource.PlayOneShot(hitClip, 0.7f);

        SymbolHit();

        if (symbolCountHit >= symbolCountSanity)
        {
            PlayerController.Player.DecreaseSanity();
            symbolCountHit = 0;
        }
    }

    //when the player beats alex, stop fighting, play a win sound, turn on/off a bunch of stuff, and kill alex
    private void WinGame()
    {
        isFighting = false;
        PlayClip.AudioPlayer.PlayOneShot(winClip, 0.25f, true);
        PlayerController.Player.IsFinishedGame = true;
        PlayerController.Player.ClearKeys();
        simonDialogue.SetActive(false);
        simonDialogueEnd.SetActive(true);
        effects.SetActive(false);
        effectsEnd.SetActive(true);
        alexModel.SetActive(false);
        canvasObject.SetActive(false);
        StartCoroutine(GoBackDialogue());
        alex.StartDeath();
    }

    private IEnumerator GoBackDialogue()
    {
        yield return new WaitForSeconds(5.0f);

        if (!hasTriggeredDialogue)
        {
            hasTriggeredDialogue = true;
            playerDialogue.TriggerDialogue();
        }
    }

    public void ResetCanvas()
    {
        flickerLight.SetActive(true);
        canvas.enabled = true;
    }
}
