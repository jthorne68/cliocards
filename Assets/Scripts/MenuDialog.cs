using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuDialog : MonoBehaviour
{

    TableController controller;
    TableState state;
    TextMeshPro yeartext;

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
        updateyear();
    }

    public void OnYearPlus()
    {
        int year = state.getval(TableState.STARTYEAR);
        if (year >= 2000) return;
        year += 10;
        state.setval(TableState.STARTYEAR, year);
        updateyear();
    }

    public void OnReference()
    {
        List<int> ids = new();
        for (int i = 0; i < CardLibrary.getdataref().totalcards(); i++) ids.Add(i);
        controller.showcardcollection("All cards", ids, controller.menudlg);
    }

    public void OnExit()
    {
        Application.Quit();
    }

    public void updateyear()
    {
        yeartext.text = "" + state.getval(TableState.STARTYEAR);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
