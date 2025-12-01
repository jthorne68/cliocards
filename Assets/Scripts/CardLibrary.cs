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

    private List<Color> bgcolors;
    private List<Color> colors1;
    private List<Color> colors2;

    private TableController controller;

    // abstract card data access helper functions

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

    public void setupcard(GameObject c, int id, TableState state)
    {
        CardInfo info = data.cardinfo(id);
        int cix = 0;
        if (info.type == "year") cix = 1;
        else if (info.type == "harm") cix = 2;
        else if (info.type == "perm") cix = 3;

        if ((c == null) || (info == null)) return;
        Transform t = c.transform.Find("name");
        TextMeshPro tmp = t.GetComponent<TextMeshPro>();
        tmp.text = info.name;
        t = c.transform.Find("text");
        tmp = t.GetComponent<TextMeshPro>();
        // rich text! .Replace("$", "<color=green>$</color>");
        if (state != null) tmp.text = state.subvals(info.desc); 

        t = c.transform.Find("art1");
        SpriteRenderer sp = t.GetComponent<SpriteRenderer>();
        sp.sprite = getart(info.art + "1");
        sp.color = colors1[cix];
        t = c.transform.Find("art2");
        sp = t.GetComponent<SpriteRenderer>();
        sp.color = colors2[cix];
        sp.sprite = getart(info.art + "2");
        t = c.transform.Find("artbg");
        sp = t.GetComponent<SpriteRenderer>();
        sp.color = bgcolors[cix];
    }

    public static CardLibrary instance;

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);

        bgcolors = new List<Color>();
        colors1 = new List<Color>();
        colors2 = new List<Color>();

        Color c;
        ColorUtility.TryParseHtmlString("#47B3C8", out c);
        bgcolors.Add(c);
        ColorUtility.TryParseHtmlString("#FFD66E", out c);
        bgcolors.Add(c);
        ColorUtility.TryParseHtmlString("#9DC6B1", out c);
        bgcolors.Add(c);
        ColorUtility.TryParseHtmlString("#B9C69D", out c);
        bgcolors.Add(c);

        ColorUtility.TryParseHtmlString("#000054", out c);
        colors1.Add(c);
        ColorUtility.TryParseHtmlString("#650F06", out c);
        colors1.Add(c);
        ColorUtility.TryParseHtmlString("#280C31", out c);
        colors1.Add(c);
        ColorUtility.TryParseHtmlString("#15310C", out c);
        colors1.Add(c);

        ColorUtility.TryParseHtmlString("#057373", out c);
        colors2.Add(c);
        ColorUtility.TryParseHtmlString("#CA6D29", out c);
        colors2.Add(c);
        ColorUtility.TryParseHtmlString("#273D56", out c);
        colors2.Add(c);
        ColorUtility.TryParseHtmlString("#4C5627", out c);
        colors2.Add(c);

        artmap = new Dictionary<string, Sprite>();
    }
}
