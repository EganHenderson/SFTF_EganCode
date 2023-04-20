using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseCanvas = null;
    [SerializeField]
    private MainMenu mainMenu = null;

    public bool paused = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PlayerController.Player.PlayerViewCamera == true)
        {
            if (NotebookController.Instance.IsDrawing)
                NotebookController.Instance.ToggleNotebook();
            else if (PlayerController.Player.CameraEnabled)
                PlayerController.Player.ToggleCamera();
            else
                TogglePause();
        }
    }

    public void TogglePause()
    {
        pauseCanvas.SetActive(!pauseCanvas.activeInHierarchy);

        if (pauseCanvas.activeInHierarchy)
        {
            paused = true;

            Time.timeScale = 0;
            PlayerController.Player.InternalLockUpdate(CursorLockMode.None, true);
            PlayerController.Player.CanBringUpCamera = false;
            PlayerController.Player.CanBringUpFlashlight = false;
            PlayerController.Player.CanBringUpNotebook = false;
            NotebookController.Instance.IsDrawing = false;
            AudioListener.pause = true;

            mainMenu.settingsMenu.SetActive(true);
            mainMenu.audioMenu.SetActive(false);
            mainMenu.graphicsMenu.SetActive(false);
        }
        else
        {
            paused = false;

            if (NotebookController.Instance.NotebookActive)
            {
                Time.timeScale = 1;
                PlayerController.Player.InternalLockUpdate(CursorLockMode.Confined, true);
                NotebookController.Instance.IsDrawing = true;
            }

            else if (DialogueManager.Instance.DialogueOpen)
            {
                Time.timeScale = 0;
                PlayerController.Player.InternalLockUpdate(CursorLockMode.Confined, true);
            }

            else
            {
                Time.timeScale = 1;
                PlayerController.Player.InternalLockUpdate(CursorLockMode.Locked, false);

                PlayerController.Player.CanBringUpFlashlight = true;

                /*
                if (PlayerController.Player.TutManager)
                {
                    if (PlayerController.Player.TutManager.cPressed)
                        PlayerController.Player.CanBringUpCamera = true;
                    if (PlayerController.Player.TutManager.tabPressed)
                        PlayerController.Player.CanBringUpNotebook = true;
                }
                else
                {
                    PlayerController.Player.CanBringUpCamera = true;
                    PlayerController.Player.CanBringUpNotebook = true;
                }
                */

                PlayerController.Player.CanBringUpCamera = true;
                PlayerController.Player.CanBringUpNotebook = true;
            }

            AudioListener.pause = false;
        }
    }
}
