using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuDialog : MonoBehaviour
{

    TableController controller;
    TableState state;
    TextMeshPro yeartext;
    TextMeshPro difftext;

    public GameObject closebtn;
    public GameObject resignbtn;
    public GameObject playbtn;
    public GameObject leftbtn;
    public GameObject rightbtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
        state = controller.state;
        updatevisibility();
        yeartext = GameObject.Find("yeartext").GetComponent<TextMeshPro>();
        difftext = GameObject.Find("difftext").GetComponent<TextMeshPro>();
        updateyear();
    }

    void updatevisibility()
    {
        bool isgame = controller.state.isingame;
        closebtn.SetActive(isgame);
        resignbtn.SetActive(isgame);

        playbtn.SetActive(!isgame);
        leftbtn.SetActive(!isgame);
        rightbtn.SetActive(!isgame);
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

    public void OnResign()
    {
        controller.state.quitgame();
        updatevisibility();
    }

    public void OnYearMinus()
    {
        int year = state.getval(TableState.STARTYEAR);
        if (year <= 1900) return;
        year -= 10;
        state.setval(TableState.STARTYEAR, year);
        leftbtn.SetActive(year > 1900);
        rightbtn.SetActive(true);
        updateyear();
    }

    public void OnYearPlus()
    {
        int year = state.getval(TableState.STARTYEAR);
        if (year >= 2000) return;
        year += 10;
        state.setval(TableState.STARTYEAR, year);
        rightbtn.SetActive(year < 2000);
        leftbtn.SetActive(true);
        updateyear();
    }

    public void OnReference()
    {
        List<int> ids = new();
        for (int i = 0; i < CardLibrary.getdataref().totalcards(); i++)
        {
            CardInfo info = CardLibrary.cardinfo(i);
            if ((info.type == "") || (info.type == "perm")) ids.Add(i);
        }
        controller.showcardcollection("All cards", ids, controller.menudlg);
    }

    public void OnExit()
    {
        Application.Quit();
    }

    public void updateyear()
    {
        int startyear = state.getval(TableState.STARTYEAR);
        yeartext.text = "" + startyear;

        string dtext = "";
        for (;startyear > 1900; startyear -= 10)
            dtext += startyear + " " + CardLibrary.cardinfo(CardLibrary.idfor("" + startyear)).desc + "\n";            
        difftext.text = dtext;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
