using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class CardsDialog : MonoBehaviour
{
    public TableController controller;
    public TableState state;
    CardData carddata;
    GameObject returndlg = null;

    string titletext;
    Dictionary<int, int> idcounts;
    List<int> ids;

    GameObject surface;
    float yspeed;
    float prevypos;

    float minypos;
    float maxypos;

    void Start()
    {
        surface = GameObject.Find("surface").gameObject;
        GameObject.Find("title").GetComponent<TextMeshPro>().text = titletext;
        const int MAXCOL = 4;
        float COLWIDTH = 250f;
        float ROWHEIGHT = 350f;
        int row = 0;
        int column = 0;
        minypos = 0;
        carddata = CardLibrary.getdataref();
        for (int i = 0; i < ids.Count; i++)
        {
            int id = ids[i];
            if (carddata.cardinfo(id).type != "COMMENT")
            {
                GameObject c = Instantiate(CardLibrary.instance.card, surface.transform);
                c.GetComponent<CardHandler>().ismoving = false;
                CardLibrary.instance.setupcard(c, id, state);
                if (idcounts[id] > 1) c.transform.Find("name").GetComponent<TextMeshPro>().text += " x" + idcounts[id];
                c.transform.localScale = new Vector2(200, 200);
                float x = (column - (((float)MAXCOL) / 2)) * COLWIDTH;
                float y = -row * ROWHEIGHT;
                maxypos = -y;
                c.transform.localPosition = new Vector2(x, y);
                c.GetComponent<SortingGroup>().sortingLayerName = "UI Cards";
                column++;
                if (column > MAXCOL)
                {
                    column = 0;
                    row++;
                }
            }
        }
    }

    public void setup(string text, List<int>cardids, TableState s, TableController c, GameObject r = null)
    {
        controller = c;
        returndlg = r;
        state = s;
        titletext = text;
        ids = new();
        idcounts = new();
        for (int i = 0; i < cardids.Count; i++)
        {
            int id = cardids[i];
            if (idcounts.ContainsKey(id)) idcounts[id]++;
            else
            {
                ids.Add(id);
                idcounts.Add(id, 1);
            }
        }
    }

    public void OnClose()
    {
        Destroy(gameObject);
        if (returndlg != null) controller.showdialog(returndlg);
    }


    public void surfacemousedown()
    {

    }

    public void surfacemousedrag()
    {
        float newypos = Input.mousePosition.y;
        if (prevypos != 0f)
            yspeed = newypos - prevypos;
        prevypos = newypos;
    }

    public void surfacemousewheel()
    {
        yspeed = -Input.mouseScrollDelta.y * 10;
    }

    public void surfacemouseup()
    {
        prevypos = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (yspeed != 0f)
        {
            float newypos = surface.transform.localPosition.y + yspeed;
            if (newypos < minypos) newypos = minypos;
            if (newypos > maxypos) newypos = maxypos;
            surface.transform.localPosition = new Vector3(surface.transform.localPosition.x, newypos, 0f);
            yspeed /= 1.0f + (10.0f * Time.deltaTime);
            if (Mathf.Abs(yspeed) < 0.01f) yspeed = 0f;
        }
    }
}
