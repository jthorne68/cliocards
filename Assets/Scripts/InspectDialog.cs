using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class InspectDialog : MonoBehaviour
{
    TableController controller;
    public GameObject text;
    public GameObject inspectslot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();

        // show description
        int id = controller.zoomid;

        GameObject c = Instantiate(CardLibrary.instance.card, inspectslot.transform);
        c.GetComponent<CardHandler>().ismoving = false;
        CardLibrary.instance.setupcard(c, id, controller.state);

        string ctext = "Glossary of symbols:\n\n";
        foreach (KeyValuePair<string, string> s in TableState.symboltable) ctext += s.Key + ": " + s.Value + "\n";
        // use some markdown-like syntax for interpreting?
        string txt = CardLibrary.cardinfo(id).text;
        if (txt != "") ctext += "\n\n" + txt;

        text.GetComponent<TextMeshPro>().text = ctext;
    }

    public void OnClose()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
