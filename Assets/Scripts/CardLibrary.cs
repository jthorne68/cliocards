using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;


public class CardLibrary : MonoBehaviour
{
    public GameObject card; // card template prefab

	private CardData data;

    private Dictionary<string, Sprite> artmap;

    private TableController controller;

    // abstract card data access helper functions

    private Dictionary<char, Color> colors = new();

    public static string[] colorvalues = {
        "#6A8293",
        "#4F965A",
        "#4F90C8",
        "#8286A9",
        "#858585",
        "#959066",
        "#9C6E45",
        "#B45F5C",
        "#A85D71",
        "#9B5E7D"
    };

    /*
    private static string[] colornames = {
        "0#AAB1B7",
        "1#A0B9A4",
        "2#A1B7CC",
        "3#B2B4C0",
        "4#B2B4C0",
        "5#B2B4C0",
        "6#BBAB9C",
        "7#CBB8B7",
        "8#C0A5AC",
        "9#BBA5B1",
        "A#6A8293",
        "B#4F965A",
        "C#4F90C8",
        "D#8286A9",
        "E#858585",
        "F#959066",
        "G#9C6E45",
        "H#B45F5C",
        "I#A85D71",
        "J#9B5E7D",
        "K#1A252F",
        "L#112F15",
        "M#112D49",
        "N#262939",
        "O#222222",
        "P#2F2D19",
        "Q#331D09",
        "R#450C13",
        "S#38151F",
        "T#321725"
    };
    */

    public Color colorfor(char c)
    {
        return colors[c];
    }

    public static int idfor(string name) { return instance.data.idfor(name); }
    public static CardRules cardrules(int id) { return id < 0 ? null : id >= instance.data.cardrules.Count ? null : 
            instance.data.cardrules[id]; }
    public static int randomcard(string type, int rarity, List<int> skip) { return instance.data.randomcard(type, rarity, skip); }
    public static List<CardRule> rulesfor(int id) { return instance.data.rulesfor(id); }
    public static CardInfo cardinfo(int id) { return instance.data.cardinfo(id); }

    public static CardData getdataref() { return instance.data; }

    public CardLibrary()
    {
        data = new();
        instance = this;
    }

    // game object manipulation functions
    void Start()
    {
        instance = this;
        controller = GameObject.Find("TableController").GetComponent<TableController>();
    }

    public Sprite getart(string name)
    {
        Sprite s = artmap.GetValueOrDefault(name);
        if (s == null) s = artmap[name] = Resources.Load<Sprite>("Art\\" + name);
        return s;
    }

    public List<int> getreferencelist()
    {
        List<int> ids = new();
        for (int i = 0; i < data.totalcards(); i++)
        {
            CardInfo info = cardinfo(i);
            if ((info.type == "") || (info.type == "perm") || (info.type == "temp")) ids.Add(i);
        }
        return ids;
    }

    public void setupcard(GameObject c, int id, TableState state)
    {
        CardInfo info = data.cardinfo(id);

        if ((c == null) || (info == null)) return;
        Transform t = c.transform.Find("name");
        TextMeshPro tmp = t.GetComponent<TextMeshPro>();
        tmp.text = info.name;
        t = c.transform.Find("text");
        tmp = t.GetComponent<TextMeshPro>();
        if (state != null) tmp.text = state.subvals(info.desc);
        int len = info.art.Length;
        if (len < 3) return;
        t = c.transform.Find("art1");
        SpriteRenderer sp = t.GetComponent<SpriteRenderer>();
        sp.sprite = getart(info.art.Substring(0, len - 3) + "1");
        sp.color = colorfor(info.art[len - 2]);
        t = c.transform.Find("art2");
        sp = t.GetComponent<SpriteRenderer>();
        sp.color = colorfor(info.art[len - 1]);
        sp.sprite = getart(info.art.Substring(0, len - 3) + "2");
        t = c.transform.Find("artbg");
        sp = t.GetComponent<SpriteRenderer>();
        sp.color = colorfor(info.art[len - 3]);
    }

    public static CardLibrary instance;

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);

        Color clr;
        for (int i = 0; i < 10; i++)
        {
            ColorUtility.TryParseHtmlString(colorvalues[i], out clr);
            colors[(char)((int)'A' + i)] = Color.Lerp(clr, Color.black, 0.2f);
            colors[(char)((int)'0' + i)] = Color.Lerp(clr, Color.white, 0.6f);
            colors[(char)((int)'K' + i)] = Color.Lerp(clr, Color.black, 0.8f);
        }

        /*
        foreach (string s in colornames) {
            char k = s[0];
            ColorUtility.TryParseHtmlString(s.Remove(0, 1), out clr);
            if (k >= '0' && k <= '9') {
                ColorUtility.TryParseHtmlString(s.Remove(0, 1), out clr);
                clr = Color.Lerp(clr, Color.white, 0.9f);
            }
            else if ((k >= 'K') && (k <= 'T') {

            }
            colors[k] = clr;
        }
        */

        artmap = new Dictionary<string, Sprite>();
    }
}
