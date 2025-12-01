using UnityEngine;

public class MenuDialog : MonoBehaviour
{

    TableController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
        if (!controller.isingame) Destroy(GameObject.Find("closebutton"));
    }

    public void OnClose()
    {
        Destroy(gameObject);
    }

    async public void OnNewGame()
    {
        await controller.fadescreen();
        OnClose();
        controller.fadehandler.fade(false);
        controller.newgame();
    }

    public void OnExit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
