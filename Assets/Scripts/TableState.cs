using UnityEngine;
using System.Collections.Generic;

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
    public const string DEALS = "deals"; // 0-5
    public const string MAXDEALS = "maxdeals"; // 0-5
    public const string QUARTER = "quarter"; // 1-4
    public const string YEAR = "year"; // 1-25
    public const string STARTYEAR = "startyear"; // 1900-2000
    public const string REMOVE = "remove"; // this removes the current card from the deck
    public const string CAPITAL = "capital"; // any value

    public const string DECK = "deck"; // used for rules that add or remove cards
    public const string PERM = "perm"; // used for rules that add or remove permanent effects

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

    public Dictionary<string, int> values; // all game state numbers

    private CardData data;

	// default constructor
	public TableState(CardData d)
	{
        data = d;
		challenge = 0;
		perms = new();
		mycards = new();
		deck = new();
		discard = new();
		values = new Dictionary<string, int>();
		play = new List<int>(new int[MAX_SLOTS]);
        hand = new List<int>(new int[MAX_HAND]);
	}
	
	// copy constructor
	public TableState(TableState t)
	{
        data = t.data;
		challenge = t.challenge;
		perms = new List<int>(t.perms);
		mycards = new List<int>(t.mycards);
		deck = new List<int>(t.deck);
		hand = new List<int>(t.hand);
		play = new List<int>(t.play);
		discard = new List<int>(t.discard);
		values = new Dictionary<string, int>(t.values);
	}


	public void newgame()
	{			
		// set up default values according to current game settings
        perms = new();
        values = new();

        setval(MAXDEALS, 2);
        setval(MAXHAND, 5);
        setval(MAXPLAYS, 3);
        setval(YEAR, 0);
        setval(STARTYEAR, 1900);
        setval(CAPITAL, 100);

        setval(GOAL, 20);
        setval(STABILITY, 100);

        setval("debt", 10);
        setval("investment", 1);
        setval("elite", 10);

        setval("education", 1);
        setval("housing", 1);
        setval("health", 1);

        challenge = 0;

        // starter cards
        mycards = new List<int>();
        for (int j = 0; j < 5; j++) {
            mycards.Add(data.idfor("Sales"));
            mycards.Add(data.idfor("Repayment"));
        }

        // mycards.Add(data.idfor("Invest"));
        // mycards.Add(data.idfor("Sell-off"));

        //int testperm = data.idfor("Deficit");
        //perms.Add(testperm);
	}
	
	// value processing

    public int getval(string key, int d = 0) 
	{ 
		int v = 0; 
		return (values != null) && values.TryGetValue(key, out v) ? v : d; 
	}
	
    public void setval(string key, int v = 0) 
    {
		values[key] = v;
    }
	
    public void addval(string key, int a = 0) 
	{ 
		processrules(key, a.ToString()); 
	}

    public string subvals(string s)
    {
        if (s.IndexOf('{') != -1)
            foreach (KeyValuePair<string, int> e in values)
                s = s.Replace("{" + e.Key + "}", "<color=yellow>" + e.Value.ToString() + "</color>");
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
        char op = amount.Length == 0 ? ' ' : amount[0];
        if ("+-=".IndexOf(op) == -1) op = '+';
        else amount = amount.Remove(0, 1);

        // first, check to see if rule is adding or removing a card from the deck
        if (stat == DECK)
        {
            int id = data.idfor(amount);
            if (op == '+') mycards.Add(id);
            else if (op == '-') mycards.Remove(id);
        }
        if (stat == PERM)
        {
            int id = data.idfor(amount);
            if (op == '+')
            {
                // permanent effects must be unique
                if (!perms.Contains(id)) perms.Add(id);
            }
            else if (op == '-') perms.Remove(id);
        }

        if (!values.TryGetValue(stat, out v)) values[stat] = v = 0;
        if (!values.TryGetValue(amount, out a)) int.TryParse(amount, out a);
        switch (op)
        {
            case '=': a = a - v; break;
            case '-': a = -a; break;
        }
        v += a;
        setval(stat, v);
        for (int i = 0; i < perms.Count; i++) {
            CardRules rules = data.cardrules[perms[i]];
            for (int j = 0; j < rules.rules.Count; j++) {
                CardRule r = rules.rules[j];
                if (r.startat.istriggered(stat, v, a))
                    result.AddRange(processrules(r.stat, r.amount));
                if (r.endat.istriggered(stat, v, a)) perms[i] = 0;
            }
        }
        perms.RemoveAll(i => i == 0);
        return result;
    }	
	
	
	// card manipulation utilities

	public void newyear()
	{
        setval(WEALTH, 0);
        setval(DEALS, getval(MAXDEALS));
        setval(HAND, getval(MAXHAND));
        setval(PLAYS, getval(MAXPLAYS));

        addval(YEAR, 1);
        setval(QUARTER, 1);

        challenge = data.randomcard("year", 0, new List<int>());

        // challenge rules apply at the end of year/quarter, not beginning
        List<CardRule> r = data.rulesfor(challenge);
        for (int i = 0; i < r.Count; i++) processrules(r[i].stat, r[i].amount);
	}

	
	public void shuffledeck()
	{
        deck = new List<int>(mycards);
        discard = new List<int>();

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
		List<CardRule> rules = data.rulesfor(id);
		for (int j = 0; j < rules.Count; j++) {
			CardRule rule = rules[j];
            if (rule.stat == REMOVE)
            {
                mycards.Remove(id);
                results.Add(rule);
            }
            else if (string.IsNullOrEmpty(rule.startat.stat))
                results.AddRange(processrules(rule.stat, rule.amount));
            else
                perms.Add(id);
		}
        return results;
	}
	
	public void processplays()
	{
		int curplays = getval(PLAYS);
        for (int i = 0; i < curplays; i++) playcardindex(i);
	}

}