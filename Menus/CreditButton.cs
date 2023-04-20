using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditButton : MonoBehaviour
{
    [SerializeField]
    private GameObject afterCreditsDialogue = null, background = null;

    public void NextButton()
    {
        //if the player has found all collectables, show them the after credits scene
        if (PlayerController.Player.CollectableCount >= 5)
        {
            gameObject.SetActive(false);
            background.SetActive(true);
            afterCreditsDialogue.SetActive(true);
            afterCreditsDialogue.GetComponent<DialogueTrigger>().TriggerDialogue();
        }
        //else just go back to the main menu
        else
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
            LevelManager.Instance.SwitchLevel(0, 0);
        }
    }
}
