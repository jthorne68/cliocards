using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;


public class TableState
{
	// default stat names
    public const string WEALTH = "wealth"; // 0-inf
    public const string GOAL = "goal"; // 0-inf
    public const string STABILITY = "stability"; // 0-100
    public const string PLAYS = "plays"; // 0-12
    public const string MAXPLAYS = "maxplays"; // 0-12
    public const string HAND = "hand"; // 0-8
    public const string MAXHAND = "maxhand"; // 0-8
    public const string DEALS = "deals"; // 0-inf
    public const string MAXDEALS = "maxdeals"; // 0-inf
    public const string QUARTER = "quarter"; // 1-4
    public const string YEAR = "year"; // 1-25
    public const string STARTYEAR = "startyear"; // 1900-2000
    public const string CAPITAL = "capital"; // any value
    public const string MAXDECADE = "maxdecade"; // 1900-2000

    public const string REMOVE = "remove"; // this removes the current card from the deck
    public const string DELETE = "delete"; // this removes the first card in hand
    public const string PREVENT = "prevent"; // this removes the next card added

    public const string DECK = "deck"; // used for rules that add or remove cards

    public const string TEMP = "temp"; // used for temporary cards that can only be generated

    public const string PERM = "perm"; // used for rules that add or remove permanent effects
    public const string STAT = "stat"; // used for rules that add or remove status effects

    public const string YEAR1 = "year1"; // year cards group 1
    public const string YEAR2 = "year2"; // year cards group 2
    public const string YEAR3 = "year3"; // year cards group 3
    public const string YEAR4 = "year4"; // year cards group 4
    public const string BOSS = "boss"; // boss year cards



    // default stat values
    public const int MAX_SLOTS = 12;
    public const int MAX_HAND = 8;
    public const int MAX_PERMS = 16;


	// state variables

    public int challenge; // current challenge card id

    public List<int> perms; // permanent effects
    public List<int> mycards; // cards owned
    public List<int> deck; // cards left to deal
    public List<int> hand; // cards in hand
    public List<int> play; // cards on table
    public List<int> discard; // cards discarded

    public List<int> challenges; // challenges already seen

    public Dictionary<string, int> values; // all game state numbers

    public static Dictionary<string, string> symboltable = new() {
        { "investment", "\u25B2" },
        { "debt", "\u25BC" },
        { "energy", "\u26a1" },
        { "unrest", "\u270A" },
        { "education", "\uF6FE" }
    };

    public bool isingame = false;

    private CardData data;

    // default constructor
    public TableState()
	{
        isingame = false;
		challenge = 0;
        challenges = new();
		perms = new();
		mycards = new();
		deck = new();
		discard = new();
		values = new Dictionary<string, int>();
        play = new();
        hand = new();
    }

    public void init() // use when NOT constructing from json
    {
        play = new List<int>(new int[MAX_SLOTS]);
        hand = new List<int>(new int[MAX_HAND]);
        setval(STARTYEAR, 1900);
    }

    // copy constructor
    public TableState(TableState t)
	{
        isingame = t.isingame;
        data = t.data;
		challenge = t.challenge;
        challenges = new List<int>(t.challenges);
		perms = new List<int>(t.perms);
		mycards = new List<int>(t.mycards);
		deck = new List<int>(t.deck);
		hand = new List<int>(t.hand);
		play = new List<int>(t.play);
		discard = new List<int>(t.discard);
		values = new Dictionary<string, int>(t.values);
	}

    public void setcarddata(CardData d) { data = d; }

    public void newgame()
	{
        // set up default values according to current game settings
        isingame = true;

        challenges = new();
        perms = new();
        play = new List<int>(new int[MAX_SLOTS]);
        hand = new List<int>(new int[MAX_HAND]);
        discard = new();
        mycards = new();

        setval(YEAR, 0);
        challenge = 0;

        for (int processyear = 1900; processyear <= getval(STARTYEAR); processyear += 10)
            processrulesforcard(data.idfor("" + processyear));

    }

    public void quitgame()
    {
        isingame = false;
        perms = new();
        mycards = new();
        deck = new();
        discard = new();
        play = new List<int>(new int[MAX_SLOTS]);
        hand = new List<int>(new int[MAX_HAND]);
    }

    // value processing

    public int getval(string key, int d = 0) 
	{ 
		int v = 0; 
		return (values != null) && values.TryGetValue(key, out v) ? v : d; 
	}
	
    public void setval(string key, int v = 0) 
    {
        if ((key == REMOVE) || (key == DELETE) || (key == PREVENT))
        {
            Debug.Log("invalid key: " + key);
        }
        if (key == STABILITY) v = Mathf.Clamp(v, -10000, 100);
        else if (key == PLAYS) v = Mathf.Clamp(v, 1, 12);
        else if (key == MAXPLAYS) v = Mathf.Clamp(v, 1, 12);
        else if (key == HAND) v = Mathf.Clamp(v, 1, 8);
        else if (key == MAXHAND) v = Mathf.Clamp(v, 1, 8);
        else if (key == DEALS) v = Mathf.Clamp(v, 0, 5);
        else if (key == MAXDEALS) v = Mathf.Clamp(v, 1, 5);
        values[key] = v;
    }
	
    public List<CardRule> addval(string key, int a = 0) 
	{ 
		return processrules(key, a.ToString()); 
	}

    public string symbolfor(string key)
    {
        if (!symboltable.TryGetValue(key, out string result)) result = "";
        return result;
    }

    public string subvals(string s)
    {
        if (s.IndexOf('{') == -1) return s;
        foreach (KeyValuePair<string, int> e in values)
        {
            string cval;
            if (e.Key == "investment") cval = CardLibrary.colorvalues[1]; // green
            else if (e.Key == "debt") cval = CardLibrary.colorvalues[8]; // purple
            else if (e.Key == "education") cval = CardLibrary.colorvalues[2]; // blue
            else if (e.Key == "energy") cval = CardLibrary.colorvalues[5]; // yellow
            else cval = CardLibrary.colorvalues[5];
            s = s.Replace("{" + e.Key + "}", "<color=" + cval + ">" + e.Value.ToString() + symbolfor(e.Key) + "</color>")
                .Replace("+<color=" + cval + ">-", "<color=" + cval + ">-") // don't show +-
                .Replace("-<color=" + cval + ">-", "<color=" + cval + ">+"); // don't show --
        }
        s = s.Replace("\nremove", "\n<color=" + CardLibrary.colorvalues[7] + ">remove</color>"); // red
        return s;
    }

    // card rule processing

    public List<CardRule> processrules(string stat, string amount)
    {
        List<CardRule> result = new(); // list of rules that were applied
        result.Add(new CardRule { stat = stat, amount = amount });

        // apply a single rule, adding amount to value immediately (defaulting to zero)
        // then checking cards listed under permanent effects to see if other rules trigger
        int v = 0;
        int a = 0;
        int addedid = 0;
        char op = amount.Length == 0 ? ' ' : amount[0];
        if ("+-=".IndexOf(op) == -1) op = '+';
        else amount = amount.Remove(0, 1);

        // first, check to see if rule is adding or removing a card from the deck
        if (stat == DECK)
        {
           int id = data.idfor(amount);
            if (op == '+')
            {
                mycards.Add(id);
                deck.Add(id);
                addedid = id;
                a = 1;
            }
            else if (op == '-')
            {
                mycards.Remove(id);
                a = -1;
            }
        }
        // then check to see if it's adding a permanent/status
        else if ((stat == PERM) || (stat == STAT))
        {
            int id = data.idfor(amount);
            if (op == '+')
            {
                // permanent effects and named status effects must be unique
                // NOTE: This is enforced by store; statuses can stack.
                if (((stat == STAT) && (amount[0] == ' ')) || !perms.Contains(id)) perms.Add(id);
            }
            else if (op == '-') perms.Remove(id);
        }
        else if ((stat == REMOVE) || (stat == DELETE) || (stat == PREVENT))
        {
            // this rule is removing/deleting
            // and should not change stats
        }
        // otherwise, change a value
        else
        { 
            if (!values.TryGetValue(stat, out v)) values[stat] = v = 0;
            if (!values.TryGetValue(amount, out a)) int.TryParse(amount, out a);
            switch (op)
            {
                case '=': a = a - v; break;
                case '-': a = -a; break;
            }
            int oldv = v;
            v += a;
            setval(stat, v);
            if (v == oldv) a = 0; // value didn't change, don't trigger +/- rules
        }

        bool ischallenge = challenge != 0;
        if (ischallenge) perms.Add(challenge);
        List<int> removals = new();
        for (int i = 0; i < perms.Count; i++) {
            CardRules rules = data.cardrules[perms[i]];
            for (int j = 0; j < rules.rules.Count; j++) {
                CardRule r = rules.rules[j];
                if (r.startat.istriggered(stat, v, a))
                {
                    if (r.stat == PREVENT)
                    {
                        mycards.Remove(addedid); // never mind; remove the card immediately from deck and collection
                        deck.RemoveAt(deck.Count - 1);
                    }
                    foreach (CardRule rule in processrules(r.stat, r.amount))
                    {
                        if (rule.id == 0) rule.id = perms[i];
                        result.Add(rule);
                    }
                    // result.AddRange(processrules(r.stat, r.amount));
                }
                if (r.endat.istriggered(stat, v, a)) removals.Add(i); // perms[i] = 0;
            }
        }
        if (ischallenge) perms.RemoveAt(perms.Count - 1);
        for (int j = removals.Count - 1; j >= 0; j--)
            perms.RemoveAt(removals[j]);
        // perms.RemoveAll(i => i == 0);
        return result;
    }	
	
    public List<CardRule> processrulesforcard(int id)
    {
        List<CardRule> result = new();
        List<CardRule> r = data.rulesfor(id);
        for (int i = 0; i < r.Count; i++)
            if (r[i].startat.stat == "") // only process unconditional rules
                result.AddRange(processrules(r[i].stat, r[i].amount));
        return result;
    }

    // card manipulation utilities

    public List<CardRule> newyear()
	{
        setval(WEALTH, 0);

        List<CardRule> result = addval(YEAR, 1);

        setval(DEALS, getval(MAXDEALS));
        setval(HAND, getval(MAXHAND));
        setval(PLAYS, getval(MAXPLAYS));

        result.AddRange(processrules(QUARTER, "=1"));        

        int year = getval(YEAR);
        if (year % 5 == 0) challenge = data.randomcard("boss" + (int)(year / 5), 0, challenges);
        else
        {
            // find an available challenge appropriate for the current year range
            int trych = -1;
            challenge = -1;
            while (challenge == -1)
            {
                challenge = data.randomcard("year1", 0, challenges);
                if ((challenge == -1) || ((year > 6) && (Random.Range(0, 2) == 0)))
                    trych = data.randomcard("year2", 0, challenges);
                if (trych != -1) challenge = trych;
                if ((challenge == -1) || ((year > 11) && (Random.Range(0, 2) == 0)))
                    trych = data.randomcard("year3", 0, challenges);
                if (trych != -1) challenge = trych;
                if ((challenge == -1) || (year > 20) || ((year > 16) && (Random.Range(0, 2) == 0)))
                    trych = data.randomcard("year4", 0, challenges);
                if (trych != -1) challenge = trych;
            }
        }

        // challenge rules apply at the end of year/quarter, not beginning
        challenges.Add(challenge);
        result.AddRange(processrulesforcard(challenge));
        return result;
	}

	
	public void shuffledeck(bool isallcards = true)
	{
        if (isallcards)
        {
            deck = new List<int>(mycards);
            discard = new List<int>();
        }

        for (int i = deck.Count - 1; i > 0; i--) {
            var r = UnityEngine.Random.Range(0, i + 1);
            if (r != i) {
                var temp = deck[i];
                deck[i] = deck[r];
                deck[r] = temp;
            }
        }
	}
	
	public int discardhandindex(int ix)
	{
		if ((ix < 0) || (ix >= hand.Count)) return 0;
		var id = hand[ix];
		if (id != 0) discard.Add(id);
		hand[ix] = 0;
		return id;
	}
	
	public int dealnextcardindex(int ix)
	{
		if (deck.Count == 0) return 0;
		var id = deck[0];
        if (ix >= hand.Count) hand.Add(id); else hand[ix] = id;
		deck.RemoveAt(0);
		return id;
	}
	
	public List<CardRule> playcardindex(int ix)
	{
		List<CardRule> results = new();
        if (ix >= play.Count) return results;
		int id = play[ix];
		if (id == 0) return results;
		play[ix] = 0;
        discard.Add(id);
		List<CardRule> rules = data.rulesfor(id);
		for (int j = 0; j < rules.Count; j++) {
			CardRule rule = rules[j];
            if (rule.stat == REMOVE)
            {
                results.AddRange(processrules(DECK, "-" + data.cardinfo(id).name));
                discard.RemoveAt(discard.Count - 1); // remove from the discard pile
                results.Add(rule);
            }
            else if (rule.stat == DELETE)
            {
                int hid = hand[0];
                hand[0] = 0;
                if (hid != 0) results.AddRange(processrules(DECK, "-" + data.cardinfo(hid).name));
                results.Add(rule);
            }
            else if (rule.stat == PERM)
            {
                int pid = CardLibrary.idfor(rule.amount.Trim('+'));
                if (!perms.Contains(pid)) perms.Add(pid);
            }
            else if (string.IsNullOrEmpty(rule.startat.stat))
                results.AddRange(processrules(rule.stat, rule.amount));
            else
                Debug.Log("Error: Invalid rule");
		}
        return results;
	}
	
	public void processplays()
	{
		int curplays = getval(PLAYS);
        for (int i = 0; i < curplays; i++) playcardindex(i);
	}

}