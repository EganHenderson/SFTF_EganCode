using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour
{
    private bool move = false;
    private GameObject target = null;
    public void SetMove(bool setMove, GameObject setTarget) { move = setMove; target = setTarget; }
    [SerializeField]
    private float moveSpeed = 0.5f;

    [SerializeField]
    private string symbolName = "";

    public string SymbolName { get { return symbolName; } }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform); //ensure the sprite looks at the camera at all times
        if (move)
            MoveTowards();
    }

    //Move the symbol towards the specified game object
    public void MoveTowards()
    {
        transform.position = Vector3.Lerp(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target && other == target.GetComponentInParent<Collider>())
            Ritual.RitualInstance.HitCanvas();
    }
}
