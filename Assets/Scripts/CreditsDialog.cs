using UnityEngine;

public class CreditsDialog : MonoBehaviour
{

    public TableController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
    }

    async public void OnClose()
    {
        await controller.fadescreen();
        controller.showmenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
