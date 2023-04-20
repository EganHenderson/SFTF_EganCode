using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using UnityEngine.UI;

//This needs some comments, but it also needs to be cleaned up quite a bit. Once it has been declunked, ensure comments are added appropriately
public class NotebookController : MonoBehaviour
{
    private static NotebookController instance;
    public static NotebookController Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<NotebookController>();

            return instance;
        }
    }

    [Serializable]
    public struct Page
    {
        public GameObject pageObject;
        public bool ableToDraw;
    }

    private Door currentDoor; //LevelExit for giving player key when drawing correct symbol
    public Door NotebookControllerCurrentDoor
    {
        get { return currentDoor; }
        set { currentDoor = value; }
    }

    [SerializeField]
    private GameObject linePrefab = null;
    private GameObject currentLine = null;
    private LineRenderer lineRenderer = null;

    [SerializeField]
    private List<Vector2> mousePositions = new List<Vector2>();

    [SerializeField]
    private List<Page> pages = null; //all the pages in the notebook
    private int currentPageIndex = 0;
    private Page currentPage = new Page();
    public Page CurrentPage { get { return currentPage; } } //returns the current page for use outside of this class
    private int currentPickupPositionIndex;
    private List<Transform> pickupPositions = new List<Transform>();

    [Tooltip("Hello")]
    [SerializeField]
    private float width = 1.0f;
    private Vector2 click = Vector2.zero;

    private List<GameObject> lines = new List<GameObject>(); //the list of lines for the current page

    private bool isDrawing;

    [SerializeField]
    private GameObject drawingUI = null;
    [SerializeField]
    private GameObject notebookObject = null;
    [SerializeField]
    private Transform leftPage = null, rightPage = null;

    [SerializeField]
    private AudioSource source = null;
    [Tooltip("The audio clip to play when the player successfully unlocks a door with the correct symbol and the clip to play when failing a symbol check.")]
    [SerializeField]
    private AudioClip unlockedDoorClip = null, failedSymbolClip = null;
    [SerializeField]
    private AudioClip[] pageTurnClips = null;

    private int layerMask = 1 << 8; //the layer we want to check when checking bounds, in this case, checking layer 8

    [Tooltip("The distance between clouds we want the recognized symbol to be from one in the training set. Lower bound = 0.0f")]
    [SerializeField]
    private float recognizeValue = 20f;

    private bool wasLightOn;
    private bool pauseCoroutineRunning = false;

    private readonly SymbolRecognizer recognizer = new SymbolRecognizer(); //recognizer for recognizing symbols
    public SymbolRecognizer GetRecognizer { get { return recognizer; } } //get the symbol recognizer for use outside the class

    private bool wasInteractActive = false;

    //setter/getter for isDrawing
    public bool IsDrawing
    {
        get { return isDrawing; }
        set { isDrawing = value; }
    }
    //getter for if the notebook is visible and enabled
    public bool NotebookActive { get { return notebookObject.activeInHierarchy; } }

    //Get the currentPickupPosition and increment it to the next one
    public Transform CurrentPickupPosition()
    {
        if (currentPickupPositionIndex >= pickupPositions.Count)
            currentPickupPositionIndex = 0;

        return pickupPositions[currentPickupPositionIndex++];
    }

    //Get the ableToDraw bool of the current page
    public bool GetPageAbleToDraw()
    {
        return currentPage.ableToDraw;
    }

    //Set the ableToDraw of the current page
    public void SetPageAbleToDraw(bool able)
    {
        currentPage.ableToDraw = able;
    }

    //Set the ableToDraw of all pages
    public void SetAllPagesAbleToDraw(bool able)
    {
        Page temp = currentPage;

        for (int i = 0; i < pages.Count; i++)
        {
            currentPage = pages[i];
            currentPage.ableToDraw = able;
        }

        currentPage = temp;
    }

    private void Start()
    {
        recognizer.LoadTrainingSet();

        if (pages.Count > 0)
            currentPage = pages[0];

        else
            Debug.LogError("No pages set for draw controller, notebook will not work!");

        isDrawing = drawingUI.activeInHierarchy;

        foreach (Page page in pages)
        {
            foreach (Transform child in page.pageObject.transform)
            {
                if (child.CompareTag("NotebookPickupLocation"))
                    pickupPositions.Add(child);
            }
        }
    }

    public void ClearDrawings()
    {
        lines.Clear();
        foreach (Page page in pages)
        {
            foreach (Transform child in page.pageObject.transform)
            {
                if (child.GetComponent<LineRenderer>())
                    Destroy(child.gameObject);
            }
        }
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Count - 1)
        {
            lines.Clear();

            if (pageTurnClips != null)
                PlayClip.AudioPlayer.PlayRandomClip(pageTurnClips);

            currentPage.ableToDraw = false;
            currentPage.pageObject.SetActive(false);
            currentPage = pages[++currentPageIndex];

            currentPage.pageObject.transform.position = leftPage.position;
            currentPage.pageObject.transform.rotation = leftPage.rotation;

            currentPage.pageObject.SetActive(true);
            currentPage.ableToDraw = true;

            AddLines();
        }
    }

    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            lines.Clear();

            if (pageTurnClips != null)
                PlayClip.AudioPlayer.PlayRandomClip(pageTurnClips);

            currentPage.ableToDraw = false;

            currentPage.pageObject.transform.position = rightPage.position;
            currentPage.pageObject.transform.rotation = rightPage.rotation;

            currentPage.pageObject.SetActive(false);
            currentPage = pages[--currentPageIndex];

            currentPage.pageObject.transform.position = leftPage.position;
            currentPage.pageObject.transform.rotation = leftPage.rotation;

            currentPage.pageObject.SetActive(true);
            currentPage.ableToDraw = true;

            AddLines();
        }
    }

    public void ResetPage()
    {
        if (currentPageIndex == 0)
            return;

        currentPage.ableToDraw = false;

        currentPage.pageObject.transform.position = rightPage.position;
        currentPage.pageObject.transform.rotation = rightPage.rotation;

        currentPage.pageObject.SetActive(false);
        currentPage = pages[0];

        currentPage.pageObject.transform.position = leftPage.position;
        currentPage.pageObject.transform.rotation = leftPage.rotation;

        currentPage.pageObject.SetActive(false);
        currentPage.ableToDraw = false;
    }

    void CreateLine(Vector2 inPos)
    {
        Transform pageTransform = currentPage.pageObject.transform;
        currentLine = Instantiate(linePrefab, pageTransform.position, pageTransform.rotation, pageTransform);
        currentLine.transform.localPosition = new Vector3(currentLine.transform.localPosition.x, currentLine.transform.localPosition.y, currentLine.transform.localPosition.z - (width / 17f));
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        mousePositions.Clear();
        lineRenderer.widthMultiplier = width;
        lineRenderer.SetPosition(0, inPos);
        lineRenderer.SetPosition(1, inPos);
        mousePositions.Add(inPos);
        mousePositions.Add(inPos);
        lines.Add(currentLine);
    }

    void UpdateLine(Vector2 newPos)
    {
        if (!lineRenderer)
            return;

        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPos);
        mousePositions.Add(newPos);
    }

    void DeleteLine(Vector2 erasePos)
    {
        foreach (GameObject line in lines)
        {
            if (line)
            {
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    Vector3 temp = new Vector3(lineRenderer.GetPosition(i).x, lineRenderer.GetPosition(i).y, line.transform.localPosition.z - (width / 17f));

                    if (CheckDistance(temp, new Vector3(erasePos.x, erasePos.y, line.transform.localPosition.z - (width / 17f))))
                    {
                        lines.Remove(line);
                        Destroy(line);
                        return;
                    }
                }
            }
        }
    }

    private bool CheckDistance(Vector3 pos1, Vector3 pos2)
    {
        if (Vector3.Distance(pos1, pos2) <= 0.005f)
            return true;

        return false;
    }

    public void ToggleNotebook()
    {
        isDrawing = !isDrawing;
        if (isDrawing)
            TakeOutNotebook();
        else
            PutAwayNotebook();

        if (wasInteractActive)
        {
            wasInteractActive = false;
            PlayerController.Player.EnableInteractPrompt();
        }
        else if (PlayerController.Player.InteractPrompt.activeInHierarchy)
        {
            wasInteractActive = true;
            PlayerController.Player.DisableInteractPrompt();
        }
    }

    void Update()
    {
        if (isDrawing && currentPage.ableToDraw)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //perform a check if the player clicked within the collider's bounds
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 3f, layerMask);

                if (hit.collider && hit.collider.Equals(currentPage.pageObject.transform.GetComponent<Collider>()))
                {
                    source.Play(); //start playing the sound from the beginning

                    click = currentPage.pageObject.transform.InverseTransformPoint(hit.point);
                    CreateLine(click);
                }
            }

            if (Input.GetMouseButton(0))
            {
                //perform a check if the player clicked within the collider's bounds
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 3f, layerMask);

                if (hit.collider && hit.collider.Equals(currentPage.pageObject.transform.GetComponent<Collider>()))
                {
                    click = currentPage.pageObject.transform.InverseTransformPoint(hit.point);

                    if (mousePositions.Count > 1)
                    {
                        if (Vector3.Distance(click, mousePositions[mousePositions.Count - 1]) > .01f)
                        {
                            if (!source.isPlaying) //play the drawing sound
                                source.UnPause();
                            UpdateLine(click);
                        }
                        else if (!pauseCoroutineRunning)
                            StartCoroutine(PauseDrawingSound());
                    }
                }
            }

            else if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
            {
                //perform a check if the player clicked within the collider's bounds
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 3f, layerMask);

                if (hit.collider && hit.collider.Equals(currentPage.pageObject.transform.GetComponent<Collider>()))
                {
                    click = currentPage.pageObject.transform.InverseTransformPoint(hit.point);
                    DeleteLine(click);
                }
            }

            else
                source.Stop();
        }

        else if (source.isPlaying)
            source.Stop();
    }

    private IEnumerator PauseDrawingSound()
    {
        pauseCoroutineRunning = true;
        yield return new WaitForSeconds(0.2f);
        source.Pause();
        pauseCoroutineRunning = false;
    }

    //Take out the notebook, displaying it and allowing the player to draw
    private void TakeOutNotebook()
    {
        wasLightOn = PlayerController.Player.FlashlightEnabled;
        PlayerController.Player.FlashlightEnabled = false;
        PlayerController.Player.CanBringUpFlashlight = false;

        currentPage.pageObject.SetActive(true);
        currentPage.ableToDraw = true;

        PlayerController.Player.CanLook = false;
        PlayerController.Player.CanMove = false;
        drawingUI.SetActive(true);
        notebookObject.SetActive(true);
        PlayerController.Player.InternalLockUpdate(CursorLockMode.Confined, true);
    }

    //Put away the notebook, hiding it and not allowing the player to draw
    private void PutAwayNotebook()
    {
        //Upload(); removed since we only want to save drawings when saving the game
        if (wasLightOn)
        {
            wasLightOn = false;
            PlayerController.Player.FlashlightEnabled = true;
        }

        PlayerController.Player.CanBringUpFlashlight = true;

        foreach (Page page in pages)
            page.pageObject.SetActive(false);

        PlayerController.Player.CanLook = true;
        PlayerController.Player.CanMove = true;
        drawingUI.SetActive(false);
        notebookObject.SetActive(false);
        PlayerController.Player.InternalLockUpdate(CursorLockMode.Locked, false);
    }

    //Uses the PDollar AI to recognize the symbol based on the current page
    public void RecognizeAddKey()
    {
        LineRenderer[] currentLines = currentPage.pageObject.GetComponentsInChildren<LineRenderer>();
        List<string> symbolsToRecognize = new List<string>();

        if (Ritual.RitualInstance && Ritual.RitualInstance.IsFighting)
        {
            if (currentLines.Length > 0 && currentLines[0].positionCount > 2)
            {
                foreach (GameObject symbol in Ritual.RitualInstance.Symbols)
                    symbolsToRecognize.Add(symbol.GetComponent<Symbol>().SymbolName);

                recognizer.RecognizeSymbol(currentLines, symbolsToRecognize);

                Debug.Log(recognizer.GetResultSymbol + " : " + recognizer.GetScore);

                if (Ritual.RitualInstance.SymbolCreated)
                {
                    if (recognizer.GetResultSymbol == Ritual.RitualInstance.SymbolCreated.GetComponent<Symbol>().SymbolName && recognizer.GetScore <= recognizeValue)
                    {
                        PlayerController.Player.IncreaseSanity(1);
                        Ritual.RitualInstance.SymbolDestroyed();
                        ClearPage();
                    }

                    else
                        PlayClip.AudioPlayer.PlayOneShot(failedSymbolClip, 0.5f);
                }
            }
        }

        else
        {
            if (currentDoor && currentLines.Length > 0 && currentLines[0].positionCount > 2)
            {
                for (int i = 0; i < currentDoor.Keys.Count; i++)
                    symbolsToRecognize.Add(currentDoor.Keys[i].symbol);

                recognizer.RecognizeSymbol(currentLines, symbolsToRecognize);

                Debug.Log(recognizer.GetResultSymbol + " : " + recognizer.GetScore);

                bool didAddKey = false;

                for (int i = 0; i < currentDoor.Keys.Count; i++)
                {
                    if (recognizer.GetResultSymbol == currentDoor.Keys[i].symbol && recognizer.GetScore <= recognizeValue && !PlayerController.Player.CheckForKey(currentDoor.Keys[i]))
                    {
                        ClearPage();
                        PlayerController.Player.IncreaseSanity(1);
                        currentDoor.AddKeyToPlayer(i);
                        didAddKey = true;
                        PlayClip.AudioPlayer.PlayOneShot(unlockedDoorClip, 1f);
                        Debug.Log("Key added to player");
                    }
                }

                if (!didAddKey)
                    PlayClip.AudioPlayer.PlayOneShot(failedSymbolClip, 0.5f);
            }
        }
    }

    private void AddLines()
    {
        foreach (Transform child in currentPage.pageObject.transform)
            if (child.GetComponent<LineRenderer>())
                lines.Add(child.gameObject);
    }

    public void ClearPage()
    {
        lines.Clear();
        foreach (Transform child in currentPage.pageObject.transform)
            if (child.GetComponent<LineRenderer>())
                Destroy(child.gameObject);
    }

    public void Reset()
    {
        ClearDrawings();
        ResetPage();
        currentPickupPositionIndex = 0;
    }
}
