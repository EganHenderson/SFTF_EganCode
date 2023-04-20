using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakePhotograph : MonoBehaviour
{
    [SerializeField]
    private RenderTexture photoTexture = null;
    [SerializeField]
    private Camera photoCamera = null;
    [SerializeField]
    private GameObject photographPrefab = null;
    private Texture2D photoTex;
    [SerializeField]
    private AudioSource shutterSource = null;
    [SerializeField] private GameObject flash = null;
    [SerializeField] private float flashTime = 0.02f;

    [SerializeField]
    private int photoLimit = 15; //the limit on how many photos the player can take
    private int numPhotos = 0; //how many photos have been taken so far

    [SerializeField]
    private Text numPhotoUI = null; //the UI to show how many photos the player can take
    [SerializeField]
    private Slider zoomCamUI = null; //the UI to show the zoom level of the camera

    [SerializeField]
    private float scrollScale = 5f; //the scale of scrolling speed for the zoom function

    private Camera playerCam; //the player's main camera

    private void Awake()
    {
        if (flash) { flash.SetActive(false); }
    }

    private void Start()
    {
        playerCam = PlayerController.Player.PlayerCamTransform.GetComponent<Camera>();
    }

    private void Update()
    {
        if (photoCamera.gameObject.activeInHierarchy)
        {
            photoCamera.fieldOfView = Mathf.Clamp(photoCamera.fieldOfView -= Input.mouseScrollDelta.y * scrollScale * Time.deltaTime, 20, 60);
            playerCam.fieldOfView = photoCamera.fieldOfView;
            zoomCamUI.value = photoCamera.fieldOfView;
        }

        else if (playerCam.fieldOfView != 60)
            playerCam.fieldOfView = 60;
    }

    public void TakePhoto()
    {
        if (shutterSource.isPlaying || numPhotos >= photoLimit)
            return;

        if (shutterSource)
            shutterSource.Play();

        if (flash) { flash.SetActive(true); }

        UpdateUI();
        StartCoroutine(LightFlash());
    }

    private void UpdateUI()
    {
        numPhotos++; //increment the number of photos when one is taken
        numPhotoUI.text = photoLimit - numPhotos + "";

        if (numPhotos >= photoLimit)
            numPhotoUI.color = Color.red;
    }

    public void Reset()
    {
        numPhotos = 0;
        numPhotoUI.text = photoLimit + "";
        zoomCamUI.value = 60;
        numPhotoUI.color = Color.white;
    }

    private IEnumerator LightFlash()
    {
        RenderTexture tempTex = RenderTexture.active;
        RenderTexture.active = photoTexture;

        photoCamera.Render();
        photoTex = new Texture2D(photoCamera.targetTexture.width, photoCamera.targetTexture.height, TextureFormat.RGBAHalf, false);
        photoTex.ReadPixels(new Rect(0, 0, photoCamera.targetTexture.width, photoCamera.targetTexture.height), 0, 0);
        photoTex.Apply();

        RenderTexture.active = tempTex;
        Transform pickupPosition = NotebookController.Instance.CurrentPickupPosition();

        GameObject photographObject = Instantiate(photographPrefab, pickupPosition.position, pickupPosition.rotation, pickupPosition.parent);
        photographObject.transform.localRotation = Quaternion.Euler(90, 180, 0);
        photographObject.GetComponent<Renderer>().material.mainTexture = photoTex;
        photographObject.GetComponent<Collectable>().PickupPosition = pickupPosition;

        PlayerController.Player.PlayerInventory.AddToInventory(photographObject);

        yield return new WaitForSeconds(flashTime);

        if (flash) { flash.SetActive(false); }
    }
}
