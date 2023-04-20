using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class should be added to an object to you want to be clickable in the notebook
//Clicking on the object will enlarge it so it can be read/examined
//Used for the photographs and collectables in the notebook
public class Clickable : MonoBehaviour
{
    [Tooltip("The collider of the object to be checked for clicks")]
    [SerializeField] 
    private Collider collide = null;
    [Tooltip("The Collectable script of this object")]
    [SerializeField]
    private Collectable collectable = null;

    // Update is called once per frame
    void Update()
    {
        if (NotebookController.Instance && NotebookController.Instance.NotebookActive && Input.GetMouseButtonDown(0))
            CheckIfClicked();
    }

    private void CheckIfClicked()
    {
        //perform a check if the player clicked within the collider's bounds
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 3f);

        if (hit.collider && hit.collider.Equals(collide))
            EnlargeObject();
    }

    private void EnlargeObject()
    {
        NotebookController.Instance.ToggleNotebook();
        transform.position = PlayerController.Player.HoldingLocation.position;
        transform.parent = PlayerController.Player.HoldingLocation;
        transform.localScale *= 2.4390244f;
        collectable.InNotebook = true;
        collectable.HoldAndPocket();
    }
}
