using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    //public LevelManager level;          //reference to level manager.
    public GameObject mainMenu;         //represents game object holding main menu canvas.
    [SerializeField]
    private GameObject settingsObject = null;  //game object with settings menus as children
    public GameObject settingsMenu;     //represents game object holding settings menu canvas.
    public GameObject audioMenu;        //represents game object holding audio menu canvas.
    public GameObject graphicsMenu;     //represents game object holding graphics menu canvas.
    public GameObject creditsMenu;      //represents game object holding credits menu canvas.
    public GameObject controlsMenu;     //represents game object holding controls menu canvas.

    [SerializeField]
    private AudioClip buttonSound = null, startSound = null;
    [SerializeField]
    private float buttonSoundVolume = 1, startSoundVolume = 1;

    [SerializeField]
    private Volume volume;

    [Tooltip("The slider for the brightness level of the game")]
    [SerializeField]
    private Slider brightnessSlider = null;

    private void Start()
    {
        if (!volume)
            volume = GameObject.Find("Global Volume").GetComponent<Volume>();
    }

    //Start the game from beginning level
    public void StartGame()
    {
        //level.SwitchLevel(1, 0);
        // No need for a public reference when there is a static one available
        PlayClip.AudioPlayer.PlayOneShot(startSound, startSoundVolume);
        GameManager.Instance.StartNewGame();

        if (PlayerController.Player.TutManager)
            PlayerController.Player.TutManager.Reset();
    }
    public void SaveGame()
    {
        GameManager.Instance.SaveGame();
    }
    public void LoadGame()
    {
        GameManager.Instance.LoadGame();
    }
    //Quit the application.
    public void QuitGame()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    //close all menus and open up main menu.
    public void ToMain()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume);
        settingsObject.SetActive(false);
        creditsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    //Close all menus and open up settings menu.
    public void ToSettings()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume);
        mainMenu.SetActive(false);
        settingsObject.SetActive(true);
    }

    //close all menus and open up audio menu.
    public void ToAudio()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume, true);
        settingsMenu.SetActive(false);
        audioMenu.SetActive(true);
    }

    //close all menus and open up graphics menu.
    public void ToGraphics()
    {
        SetBrightnessSlider();
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume, true);
        settingsMenu.SetActive(false);
        graphicsMenu.SetActive(true);
    }

    //close alal menus and open up credits
    public void ToCredits()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume, true);
        mainMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void ToControls()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume, true);
        settingsMenu.SetActive(false);
        controlsMenu.SetActive(true);
    }

    public void Back()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume, true);
        settingsMenu.SetActive(true);
        audioMenu.SetActive(false);
        graphicsMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }

    public void ResumeGame()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume, true);
        PlayerController.Player.PauseMenu.TogglePause();
    }

    public void LoadMainMenu()
    {
        PlayClip.AudioPlayer.PlayOneShot(buttonSound, buttonSoundVolume, true);
        PlayerController.Player.PauseMenu.TogglePause();
        GameManager.Instance.SwitchLevel(0, 0);
    }

    public void SetBrightness(float brightness)
    {
        if (volume.profile.TryGet(out ColorAdjustments adjustments))
            adjustments.postExposure.value = brightness;
    }

    private void SetBrightnessSlider()
    {
        if (!brightnessSlider)
            return;

        if (!volume)
            volume = GameObject.Find("Global Volume").GetComponent<Volume>();

        if (volume.profile.TryGet(out ColorAdjustments adjustments))
            brightnessSlider.value = adjustments.postExposure.value;
    }
}
