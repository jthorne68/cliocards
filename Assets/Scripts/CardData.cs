using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class CardInfo
{
    public string name;
    public string art;
    public string desc;
    public string rules;
    public string type;
}

[System.Serializable]
public class CardList
{
    public CardInfo[] cards;
}

public class RuleTiming
{
    public string stat = ""; // if empty, this is immediate
    public bool isgain = false;
    public bool isloss = false;
    public bool ismatch = false;
    public int matchvalue;
    public bool istriggered(string s, int v, int a)
    {
        return (stat == s) && (((a > 0) && isgain) || 
            ((a < 0) && isloss) || ((v == matchvalue) && ismatch));
    }
}

public class CardRule
{
    public string stat = "";
    public string amount = "";
    public RuleTiming startat = new();
    public RuleTiming endat = new();
}

//////////////////////////////////////////
//
// rule syntax
//
// separate rules with commas, spaces are ignored

// {stat}{symbol}{amount}[/startstat][startsymbol][startval][/endstat][endsymbol][endvar]
// 
// symbol = + or -
// timing = +, - or = for gain, loss, match
// variable and timevar are stat names
//
// example:add 5 WEALTH immediately
// WEALTH+5
//
// example:add 10 WEALTH and subtract 2 stability immediately
// WEALTH+10,stability-2
//
// example:add 2 WEALTH each year, ending at year 20
// WEALTH+2/year+/year=20
// 
// example: add 5 stability when quarter is 4
// stability+5/quarter=4
// 
// example: add 1 WEALTH each time sales increase until next quarter
// WEALTH+1/sales+/quarter+
//
// rules can be combined for temporary changes:
// example:add 1 play space now, lose 1 play space next year (two separate rules)
// play+1,play-1/year+/year+ (both triggers and ends when year increases)


public class CardRules
{
    public List<CardRule> rules = new List<CardRule>();
}

public class CardData
{
    public List<CardRules> cardrules;

    public CardList cardlist;

    public CardData() // constructor
    {
        string cardsjson = System.IO.File.ReadAllText("cards.json");
        cardlist = JsonUtility.FromJson<CardList>(cardsjson);
        parserules();
    }

    public List<CardRule> rulesfor(int id) { return cardrules[id].rules; }
	
    void parserules()
    {
        cardrules = new List<CardRules>();
        for (int i = 0; i < cardlist.cards.Length; i++)
        {
            string r = cardlist.cards[i].rules + ","; // ensure termination
            CardRules rlist = new CardRules();

            CardRule rule = new();
            string name = "";
            string value = "";
            int part = 1;
            string operation = "";

            // process string r into rules
            foreach(char c in r) 
            {
                if ((c == '|') || (c == ','))
                {
                    // complete the component
                    if (name != "")
                    {
                        if (part == 1)
                        {
                            rule.stat = name;
                            rule.amount = operation + value;
                        }
                        else
                        {
                            RuleTiming tm = (part == 2) ? rule.startat : rule.endat;
                            tm.stat = name;
                            tm.isgain = operation == "+";
                            tm.isloss = operation == "-";
                            tm.ismatch = operation == "=";
                            tm.matchvalue = value == "" ? 0 : Int32.Parse(value);
                        }
                        part++;
                        name = "";
                        value = "";
                        operation = "";
                    }
                    if (c == ',')
                    {
                        rlist.rules.Add(rule);
                        rule = new CardRule();
                        part = 1;
                    }
                }
                else
                {
                    if (operation != "") value += c;
                    else
                    {
                        if ("+-=".IndexOf(c) != -1) 
                            operation += c;
                        else name += c;
                    }
                }
            }

            cardrules.Add(rlist);
        }
    }
    public CardInfo cardinfo(int id) { return id < cardlist.cards.Length ? cardlist.cards[id] : null; }

    public int idfor(string name)
    {
        for (int id = 1; id < cardlist.cards.Length; id++) 
            if (cardlist.cards[id].name == name) return id;
        return 0;
    }

    public int totalcards() { return cardlist.cards.Length; }

    public int randomcard(string type, int rarity, List<int> skip)
    {
        int ix = 0;
        while (true)
        {
            ix = UnityEngine.Random.Range(3, cardlist.cards.Length);
            if ((cardlist.cards[ix].type == type) && (skip.IndexOf(ix) == -1)) return ix;
        }
    }

}
