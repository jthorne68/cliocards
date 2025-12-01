using UnityEngine;

public class SlotHandler : MonoBehaviour
{
    TableController controller;
    public bool isactive = false;
    GameObject sel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
    }
    void OnMouseDown()
    {
        if (isactive) controller.clickslot(gameObject);
    }

    private void OnMouseUp()
    {
        if (isactive) controller.releaseslot(gameObject);
    }

    public void select(bool ison = true)
    {
        if (sel == null) sel = transform.Find("select").gameObject;
        sel.SetActive(ison);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
