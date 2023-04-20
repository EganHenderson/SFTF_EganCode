using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Dialogue that gets sent through")]
    public Dialogue dialogue;
    [SerializeField]
    [Tooltip("Testing to make sure everything goes through, nothing more")]
    private Dialogue testDialogue;
    private List<GameObject> text;
    //[SerializeField][Tooltip("Text Boxes that get serialized and the place for the choices to spawn at")]
    //public GameObject textBoxes, spawnPoint;
    [Tooltip("Text Boxes that get serialized.")]
    [SerializeField] public GameObject textBoxes;
    [Tooltip("The name of the file to load for this dialogue. Do not include .txt!!!")]
    public string fileName;
    int endSpot = 0;
    [SerializeField]
    [Tooltip("Future choices, serialized because I was testing stuff and for easier testing to see if there are any issues")]
    private List<FutureDialogue> futureDialogue;
    [SerializeField]
    private GameObject temp;
    [Tooltip("True for dialogue that should only trigger once in the game. True for the dialogue that ends the game.")]
    [SerializeField]
    private bool isPhoneCall = false, isFinalDialogue, isAfterCredits = false;
    private bool isPhoneCallDone = false;

    [Tooltip("The index of this dialogue")]
    [SerializeField]
    private int dialogueIndex = -1;

    [SerializeField]
    private AudioClip phoneRing = null;

    public void TriggerDialogue()//Sets up the Dialogue for the Dialogue Manager
    {
        //if statement to ensure dialogue only happens once if so desired
        if (isPhoneCallDone)
            return;
        if (isPhoneCall)
        {
            isPhoneCallDone = true;
            if (phoneRing && !DialogueManager.Instance.HasTriggered(dialogueIndex))
                PlayClip.AudioPlayer.PlayOneShot(phoneRing, 0.05f, true);
        }
        if (isFinalDialogue)
            DialogueManager.Instance.EndGame = true;
        if (isAfterCredits)
            DialogueManager.Instance.AfterGame = true;

        if (temp != null)
        {
            temp.SetActive(false);
        }
       
        if (text == null)
            text = new List<GameObject>();
        foreach (GameObject game in text)
        {
            Destroy(game);
        }
        string[] contents; //The file that we will be using
        // This checks against null reference exceptions as well
        if (dialogue.textDoc != null && dialogue.textDoc.Length == 0) //if empty then we read in the file
        {
            string fileContents = Resources.Load<TextAsset>("Narrative/" + fileName).text;
            contents = fileContents.Split("\n"[0]);
        }
        else //otherwise we set the contents to the textDoc we set earlier
        {
            contents = dialogue.textDoc;
        }
        List<string> names = new List<string>(); //names getting added to the dialogue
        List<string> sentences = new List<string>();//sentences getting added
        if (dialogue != null)
        {
            foreach (string s in dialogue.name)
            {
                names.Add(s);
            }
            foreach (string s in dialogue.dialogue)
            {
                sentences.Add(s);
            }
        }

        if (contents != null)
            for (int i = 0; i < contents.Length; i++) //reads the textfile
            {
                if (contents[i] == ("[Start Choices]\r")) //if it finds this, then it stops or it stops at the end of the array
                {
                    setupChoices(i + 1, contents);
                    break;
                }
                if (i % 2 == 0)
                    names.Add(contents[i]); //Adds the current text to the list
                else
                    sentences.Add(contents[i]); //Adds the current sentence to the list
            }

        this.dialogue = new Dialogue(names, sentences, dialogueIndex); //Puts it all together in a nice and nifty class
        this.text = new List<GameObject>();
        List<string> tempList = new List<string>();

        if (contents != null)
            for (int i = endSpot; i < contents.Length; i++) //reads the textdoc from the ending position to pass the doc forward
            {
                tempList.Add(contents[i]);
            }

        string[] newDoc = tempList.ToArray();//Sets new doc to an array

        if (futureDialogue != null) //Checks to see if it is null, otherwise it doesn't run
        {
            //Debug.Log(futureDialogue.Count);
            foreach (FutureDialogue future in this.futureDialogue)
            {
                /* Sets up the text box that is going to be initialized
                 * Adds and changes the functions that need to be changed
                 * Sets up the choices
                 * creates a spawn point
                 * changes the text
                 * Sets the transform
                 * Sets the document
                 * Adds it to the list
                 */
                DialogueTrigger tempDialogue;
                GameObject newText = Instantiate(textBoxes, DialogueManager.Instance.SpawnPoint);
                tempDialogue = newText.GetComponent<DialogueTrigger>();

                newText.GetComponentInChildren<Text>().text = future.choice;
                tempDialogue.dialogue = future.dialogue;
                tempDialogue.dialogue.textDoc = newDoc;
                tempDialogue.fileName = fileName;
                tempDialogue.textBoxes = textBoxes;
                tempDialogue.text = this.text;
                newText.GetComponent<Button>().interactable = true;
                newText.SetActive(false);
                future.textDoc = newDoc;
                this.text.Add(newText);
            }
        }

        //Passes the information to the DialogueManager
        DialogueManager.Instance.StartDialogue(dialogue, text);
        dialogue.dialogue.Clear();
        dialogue.name.Clear();
        dialogue.textDoc = null;
    }
    private void setupChoices(int index, string[] file) //Sets up the choice
    {
        futureDialogue = new List<FutureDialogue>(); //Initialization fo FutureDialogue is here
        int i = index;
        while (i < file.Length && !file[i].Equals("[End Choices]\r")) //Reads the array until it finds [End Choices]
        {
            char[] temp = file[i].ToCharArray(); //Turns the current string to an array
            if (temp[0] == '[') //if it finds a bracket, this is a choice
            {
                Dialogue tempD;
                /* Sets the current choice to the file and trims off the '[' and ']'
                 * Creates a list of sentences from CreateChoiceDialogue
                 * names = n and s = sentences
                 */
                string choice = file[i].Substring(1, file[i].Length - 3);
                List<string> sentences = CreateChoiceDialogue(i + 1, file);
                List<string> n = new List<string>();
                List<string> s = new List<string>();
                int ins = 0;
                foreach (string se in sentences)
                {
                    //Debug.Log("Name: " + (0 == ins % 2));
                    if (ins % 2 == 0) //Whenever this is an even number, the name gets added
                        n.Add(se);
                    else //otherwise the sentences gets added
                        s.Add(se);
                    ins++;
                }
                tempD = new Dialogue(n, s, dialogueIndex);//creates a new dialogue value
                futureDialogue.Add(new FutureDialogue(choice, tempD)); //Adds to thefuture dialogue list
            }
            i++;
        }
        endSpot = i + 1; //When it finds the end spot it stores where it left off for later
    }
    private List<string> CreateChoiceDialogue(int index, string[] file) //Creates the actual dialogue
    {
        List<string> tempList = new List<string>();
        for (int i = index; i < file.Length; i++)
        {
            char[] temp = file[i].ToCharArray();
            if (temp[0] == ('['))//Just reads through the file until it finds this
            {
                return tempList;
            }
            tempList.Add(file[i]);//Adds the current one to the list
        }
        return tempList;
    }
}
public class FutureDialogue //Could've probably done this with Dialogue but the reality was I needed the string of choice to host Dialogue
{
    public string choice;
    public Dialogue dialogue;
    public string[] textDoc;
    public FutureDialogue(string choice, Dialogue dialogue)
    {
        this.choice = choice;
        this.dialogue = dialogue;
    }
    public void SetTextDoc(List<string> textDoc)
    {
        this.textDoc = textDoc.ToArray();
    }
}