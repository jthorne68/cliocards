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
    private static string[] colornames = {
        "0#475A67",
        "1#57816E",
        "2#6B9873",
        "3#90AE82",
        "4#B4B89D",
        "5#C1BD9A",
        "6#B8A481",
        "7#AE8067",
        "8#9D5C57",
        "9#97444C",
        "A#6F3142",
        "B#5C2843",
        "C#521F3D",
        "D#261327",
        "E#342547",
        "F#363A5C",
        "G#44537C",
        "H#517BA3",
        "I#6790AD",
        "J#81ABB9",
        "K#9BC2C1",
        "L#C5C5C5",
        "M#AFAFAF",
        "N#999999",
        "O#838383",
        "P#6C6C6C",
        "Q#575757",
        "R#414141",
        "S#292929",
        "T#131313",
        "U#271D17",
        "V#31271C",
        "W#47362E",
        "X#5C4C3B",
        "Y#90765D",
        "Z#AE937B",
    };

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
        foreach (string s in colornames) {
            char k = s[0];
            if (ColorUtility.TryParseHtmlString(s.Remove(0, 1), out clr))
                colors[k] = clr;
        }

        artmap = new Dictionary<string, Sprite>();
    }
}
