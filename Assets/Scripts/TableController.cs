using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System.Data;
using UnityEngine.Audio;

public class SlotInfo
{
    public List<GameObject> slots;
    public List<GameObject> cards;
    public List<int> ids;
    public int ix;
    public bool isperm = false;
}

public class TableController : MonoBehaviour
{
	public TableState state;
	
    public GameObject menudlg;
    public GameObject carddlg;
    public GameObject eventdlg;
    public GameObject reportdlg;
    public GameObject storedlg;
    public GameObject introdlg;
    public GameObject creditsdlg;
    public GameObject optionsdlg;

    public AudioSource audiosource;
    public AudioClip shufflesound;
    public AudioClip playsound;
    public AudioClip dealsound;
    public AudioClip scoresound;
    public AudioClip wealthupsound;
    public AudioClip wealthdownsound;
    public AudioClip stabilityupsound;
    public AudioClip stabilitydownsound;
    public AudioClip buysound;
    public AudioClip collapsesound;
    public AudioClip crowdsound;
    public AudioClip succeedsound;
    public AudioClip burnsound;
    public AudioClip addsound;
    public AudioClip bosssound;

    GameObject curdlg; // currenly displayed dialog

    // game object management

    public GameObject tablelandscape;
    public GameObject tableportrait;
    public GameObject table;

    List<GameObject> playslots;
    List<GameObject> permslots;
    List<GameObject> handslots;

    List<GameObject> playcards;
    List<GameObject> handcards;
    List<GameObject> permcards;

    GameObject zoomcard;
    GameObject dragcard;
    int drag;

    GameObject challengecard;

    GameObject deckslot;
    GameObject discardslot;
    GameObject zoomslot;
    GameObject selectslot;
    GameObject challengeslot;

    GameObject dealbutton;
    GameObject nextbutton;
    GameObject menubutton;

    TextMeshPro yeardisplay;
    TextMeshPro capitaldisplay;
    TextMeshPro statdisplay;

    ProgressHandler wealthhandler;
    ProgressHandler stablehandler;

    public Fader fadehandler;
    public GameObject fader;
    public GameObject tutorial;

    public GameObject floater;
    public GameObject particleburn;
    public GameObject particleflash;

    Sprite framesprite;
    Sprite backsprite;

    // camera management

    Camera maincam;

    float cwid = 0f;
    float chgt = 0f;
    float defaultortho = 4.5f;
    float aspectratio = 16.0f / 9.0f;

    // values to hide from report
    HashSet<string> defaultvalues = new();

    public bool iscrumbling = false;
    public bool isanimating = false;

    private string filename;

    public void animatenumber(string key, string amount, Transform t)
    {
        float floatsp = 0.2f;
        float wobble = 0.0f;
        if (stablehandler == null) return;
        if (wealthhandler == null) return;
        if (capitaldisplay == null) return;
        Color color = new Color(1.0f, 1.0f, 1.0f);
        Vector2 pos = Vector2.zero;
        Vector2 move = Vector2.zero;
		int a = (amount.IndexOf('-') == -1) ? 1 : -1;
        if (key == TableState.WEALTH)
        {
            color = CardLibrary.instance.colorfor('B');
            pos = (Vector2)t.position + // wealthhandler.transform.position + 
                new Vector2(Random.Range(-wobble, wobble), Random.Range(-wobble, wobble)); 
            move = new Vector2(Random.Range(-wobble, wobble), a > 0 ? floatsp : -floatsp);
        }
        if (key == TableState.STABILITY)
        {
            color = CardLibrary.instance.colorfor('C');
            pos = (Vector2)t.position + // stablehandler.transform.position + 
                new Vector2(Random.Range(-wobble, wobble), Random.Range(-wobble, wobble));
            move = new Vector2(Random.Range(-wobble, wobble), a > 0 ? floatsp : -floatsp);
        }
        if (key == TableState.CAPITAL)
        {
            pos = t.position; // capitaldisplay.transform.position;
            move = new Vector2(0, -floatsp);
        }
        if (pos == Vector2.zero) return;
        GameObject f = Instantiate(floater, pos, Quaternion.identity);
        f.transform.SetParent(table.transform);
        f.GetComponent<TextMeshPro>().text = amount;
        f.GetComponent<FloaterAnim>().move = move;
        f.GetComponent<TextMeshPro>().color = color;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadehandler = fader.GetComponent<Fader>();

        defaultvalues.Add(TableState.CAPITAL);
        defaultvalues.Add(TableState.WEALTH);
        defaultvalues.Add(TableState.GOAL);
        defaultvalues.Add(TableState.STABILITY);
        defaultvalues.Add(TableState.PLAYS);
        defaultvalues.Add(TableState.MAXPLAYS);
        defaultvalues.Add(TableState.HAND);
        defaultvalues.Add(TableState.MAXHAND);
        defaultvalues.Add(TableState.DEALS);
        defaultvalues.Add(TableState.MAXDEALS);
        defaultvalues.Add(TableState.QUARTER);
        defaultvalues.Add(TableState.YEAR);
        defaultvalues.Add(TableState.STARTYEAR);
        defaultvalues.Add(TableState.REMOVE);

        maincam = GameObject.Find("Main Camera").GetComponent<Camera>();
        table = tablelandscape;

        audiosource = GetComponent<AudioSource>();

        framesprite = Resources.Load<Sprite>("cardframe");
        backsprite = Resources.Load<Sprite>("cardback");

        playslots = new List<GameObject>(TableState.MAX_SLOTS);
        playcards = new List<GameObject>(TableState.MAX_SLOTS);
        handslots = new List<GameObject>(TableState.MAX_HAND);
        handcards = new List<GameObject>(TableState.MAX_HAND);
        permslots = new List<GameObject>(TableState.MAX_PERMS);
        permcards = new List<GameObject>(TableState.MAX_PERMS);

        loadstate();

        showdialog(introdlg);
    }

    public void loadstate()
    {
        filename = Application.persistentDataPath + "/cliodata.json";
        if (System.IO.File.Exists(filename)) {
            string statejson = System.IO.File.ReadAllText(filename);
            if (statejson != null)
                state = JsonConvert.DeserializeObject<TableState>(statejson);
        }
        if (state == null) {
            state = new TableState();
            state.init();
        }
        state.setcarddata(CardLibrary.getdataref());

        updatelayout();

        if (state.isingame)
        {
            // place all of the saved cards
            challengecard = createcard(state.challenge, challengeslot, challengeslot);
            for (int i = 0; i < state.hand.Count; i++) handcards[i] = createcard(state.hand[i], handslots[i], handslots[i]);
            for (int i = 0; i < state.play.Count; i++) playcards[i] = createcard(state.play[i], playslots[i], playslots[i]);
        }

        updatestats(true);
    }

    public void savestate()
    {
        try { System.IO.File.Delete(filename.Replace(".json", ".json.bk9")); }
        catch { Debug.Log("No backup 9 to delete"); }
        for (int i = 8; i >= 0; i--) {
            try { 
                System.IO.File.Move(filename.Replace(".json", i == 0 ? ".json" : (".json.bk" + i)), 
                    filename.Replace(".json", ".json.bk" + (i + 1))); 
            }
            catch { Debug.Log("No backup " + i + " to rename"); }
        }
        string statejson = JsonConvert.SerializeObject(state);
        System.IO.File.WriteAllText(filename, statejson);
    }


    public async Task fadescreen(bool isout = true)
    {
        fadehandler.fade(isout);
        await Task.Delay(1000);
    }
    public void closetutorial()
    {
        tutorial.SetActive(false);
    }

    public void newgame()
    {
        // clear any leftover game objects
        for (int i = 0; i < playcards.Count; i++) { Destroy(playcards[i]); playcards[i] = null; }
        for (int i = 0; i < handcards.Count; i++) { Destroy(handcards[i]); handcards[i] = null; }
        for (int i = 0; i < permcards.Count; i++) { Destroy(permcards[i]); permcards[i] = null; }
        //foreach (GameObject c in playcards) Destroy(c);
        //foreach (GameObject c in handcards) Destroy(c);
        //foreach (GameObject c in permcards) Destroy(c);
        Destroy(zoomcard);
        Destroy(dragcard);
        Destroy(challengecard);

        state.newgame();

        startnewyear();
    }

    public async void startnewyear()
    {
        tutorial.SetActive((state.getval(TableState.STARTYEAR) == 1900) && 
            (state.getval(TableState.YEAR) == 0));
        if (challengecard != null) Destroy(challengecard);

        List<CardRule> result = state.newyear();
        shuffledeck();
        updatelayout();
        int year = state.getval(TableState.YEAR);
        challengecard = createcard(state.challenge, challengeslot, challengeslot);
//        if (year % 5 == 0)
//        {
            challengecard.transform.position = Vector2.zero + Vector2.up * 5;
            CardHandler h = challengecard.GetComponent<CardHandler>();
            h.scale *= 2;
            h.pos = Vector2.zero;
            audiosource.PlayOneShot(year % 5 == 0 ? bosssound : succeedsound);
            await Task.Delay(2000);
            GameObject prevcard = challengecard;
            challengecard = createcard(state.challenge, challengecard, challengeslot);
            Destroy(prevcard);
//        }

        foreach (CardRule rule in result) await animaterule(rule);

        await dealhand();
    }

    Vector3 getmousepos()
    {
        Vector3 pos = maincam.ScreenToWorldPoint(Input.mousePosition);
        pos.x *= 2; // no idea why yet
        pos.y *= 2;
        return pos;
    }

    GameObject createcard(int id, GameObject startslot, GameObject destslot)
    {
        if (id == 0) return null; // don't make blank cards
        GameObject c = Instantiate(CardLibrary.instance.card, startslot.transform.position, Quaternion.identity);
        c.transform.SetParent(table.transform);
        c.transform.localScale = startslot.transform.localScale;
        CardLibrary.instance.setupcard(c, id, state);
        CardHandler ch = c.GetComponent<CardHandler>();
        ch.movetoward(destslot.transform);
        if ((destslot == discardslot) || (destslot == deckslot))
            ch.isdead = true; // don't preserve discards after animating
        return c;
    }

    SlotInfo slotinfo(GameObject target)
    {
        if (target == null) return null;
        SlotInfo info = new();
        int i;
        for (i = 0; i < handslots.Count; i++) {
            if (handslots[i] == target) {
                info.slots = handslots;
                info.cards = handcards;
                info.ids = state.hand;
                info.ix = i;
                return info;
            }
        }
        for (i = 0; i < playslots.Count; i++)
        {
            if (playslots[i] == target)
            {
                info.slots = playslots;
                info.cards = playcards;
                info.ids = state.play;
                info.ix = i;
                return info;
            }
        }
        for (i = 0; i < permslots.Count; i++)
        {
            if (permslots[i] == target)
            {
                info.slots = permslots;
                info.cards = permcards;
                info.ids = state.perms;
                info.ix = i;
                info.isperm = true;
                return info;
            }
        }
        return null;
    }

    void updatestats(bool ispreviewing = false)
    {
        int curwealth = state.getval(TableState.WEALTH);
        int curstab = state.getval(TableState.STABILITY);
        int curgoal = state.getval(TableState.GOAL);
        int curcapital = state.getval(TableState.CAPITAL);
        int capitaldiff = 0;
        Dictionary<string, int> diffs = new();
        string st = "";
        if (ispreviewing)
        {
            TableState newstate = new TableState(state);
            newstate.processplays();
            int wealthdiff = newstate.getval(TableState.WEALTH) - curwealth;
            int stabilitydiff = newstate.getval(TableState.STABILITY) - curstab;
            int goaldiff = newstate.getval(TableState.GOAL) - curgoal;
            capitaldiff = newstate.getval(TableState.CAPITAL) - curcapital;
            wealthhandler.setprogress(curwealth, curgoal, wealthdiff, goaldiff);
            stablehandler.setprogress(curstab, 100, stabilitydiff);
            foreach (KeyValuePair<string, int> kvp in newstate.values)
                if (!defaultvalues.Contains(kvp.Key) && (kvp.Value != state.getval(kvp.Key)))
                    diffs[kvp.Key] = kvp.Value - state.getval(kvp.Key);
        }
        else {
            wealthhandler.setprogress(curwealth, state.getval(TableState.GOAL));
            stablehandler.setprogress(curstab, 100);
        }
        capitaldisplay.text = "Capital: " + curcapital + (capitaldiff == 0 ? "" : " \u2192 " + (curcapital + capitaldiff));
        int diff = 0;
        foreach (KeyValuePair<string, int> kvp in state.values) if (!defaultvalues.Contains(kvp.Key))
            st += kvp.Key + ": " + kvp.Value + (diffs.TryGetValue(kvp.Key, out diff) ? 
                    " \u2192 " + (int)(kvp.Value + diff) + "\n" : "\n");
        foreach (KeyValuePair<string, string> s in TableState.symboltable) st = st.Replace(s.Key, s.Value + " " + s.Key);
        statdisplay.text = st;
    }

    void shuffledeck()
    {
        audiosource.PlayOneShot(shufflesound);
        state.shuffledeck();
    }

    void setbuttontext(GameObject button, string text)
    {
        button.GetComponent<ButtonHandler>().settext(text);
    }

    void updatestackslot(GameObject s, int value)
    {
        TextMeshPro tmp = s.transform.Find("cardcount").GetComponent<TextMeshPro>();
        tmp.text = "" + value;
        s.GetComponent<SpriteRenderer>().sprite = value > 0 ? backsprite : framesprite;
    }

    void updatebuttons()
    {
        setbuttontext(dealbutton, "Deal (" + state.getval(TableState.DEALS) + " left)");
        setbuttontext(nextbutton, "End Quarter " + state.getval(TableState.QUARTER));

        // also update deck/discard counters
        updatestackslot(deckslot, state.deck.Count);
        updatestackslot(discardslot, state.discard.Count);
    }

    async Task discardhand()
    {
        deselectall();
        for (int i = 0; i < state.hand.Count; i++) {
			var id = state.discardhandindex(i);
            if (id != 0) {
                Destroy(handcards[i]);
                createcard(id, handslots[i], discardslot);
                updatebuttons();
                await Task.Delay(100);
            }
        }
    }

    public async Task dealhand()
    {
        if (isanimating) return;
        if (state.getval(TableState.DEALS) <= 0) return;
        isanimating = true;

        deselectall();

        state.addval(TableState.DEALS, -1);
        updatebuttons();

        updatestats(true);
        await discardhand();
        await Task.Delay(500);

        for (int i = 0; i < state.getval(TableState.HAND); i++)
        {
            if (state.deck.Count == 0)
            {
                // shuffle ONLY the discard into hand
                audiosource.PlayOneShot(shufflesound);
                int j;
                for (j = 0; j < state.discard.Count; j++) state.deck.Add(state.discard[j]);
                state.discard = new List<int>();
                state.shuffledeck(false);
                await Task.Delay(500);
            }
			var id = state.dealnextcardindex(i);
			if (id != 0) {
				audiosource.PlayOneShot(dealsound);
				updatebuttons();
				handcards[i] = createcard(state.hand[i], deckslot, handslots[i]);
				await Task.Delay(100);
			}
        }
        isanimating = false;
        savestate();
    }

    public async void ondealbutton()
    {
        await dealhand();
    }

    public async Task animaterule(CardRule rule, Transform transform = null)
    {
        if (transform == null) {
            GameObject c = null;
            for (int i = 0; i < state.perms.Count; i++) {
                if (state.perms[i] == rule.id) {
                    transform = permslots[i].transform;
                    c = permcards[i];
                }
            }
            if (rule.id == state.challenge)
            {
                transform = challengeslot.transform;
                c = challengecard;
            }

            // edge case for adding a card
            if (rule.stat == TableState.DECK)
            {
                audiosource.PlayOneShot(addsound);
                int cid = CardLibrary.idfor(rule.amount.Trim('+'));
                if (cid != 0)
                {
                    GameObject ac = createcard(cid, c, zoomslot);
                    await Task.Delay(500);
                    Destroy(ac);
                    ac = createcard(cid, zoomslot, deckslot);
                    await Task.Delay(500);
                    Destroy(ac);
                    transform = null; // no further animation needed
                }
            }

            if (transform == null) return; // nowhere to animate?

            if (c) {
                c.GetComponent<CardHandler>().ismoving = true;
                if (c == challengecard)
                {
                    c.transform.position += (Vector3)(Vector2.up * 0.3f);
                    c.transform.localScale *= 1.3f;
                }
                else
                {
                    c.transform.position += (Vector3)(Vector2.down * 0.3f);
                    c.transform.localScale *= 2.0f;
                }
            }
        }
        bool isup = rule.amount.IndexOf('-') == -1;
        if (rule.stat == TableState.WEALTH)
            audiosource.PlayOneShot(isup ? wealthupsound : wealthdownsound);
        else if (rule.stat == TableState.STABILITY)
            audiosource.PlayOneShot(isup ? stabilityupsound : stabilitydownsound);
        else
            audiosource.PlayOneShot(scoresound);
        animatenumber(rule.stat, rule.amount, transform);
        await Task.Delay(80);
    }

    public async void nextquarter()
    {
        if (isanimating) return;

        isanimating = true;

        deselectall();

        int curplays = state.getval(TableState.PLAYS);
        for (int i = 0; i < curplays; i++)
        {
            int id = state.play[i];
			List<CardRule> results = state.playcardindex(i);

			// play relevant animations for results
			for (int j = 0; j < results.Count; j++) {
				CardRule rule = results[j];
                if (rule.stat == TableState.DECK)
                {
                    // animation for adding a card to the deck
                    if (rule.amount.IndexOf('-') == -1) {
                        audiosource.PlayOneShot(addsound);
                        int cid = CardLibrary.idfor(rule.amount.Trim('+'));
                        GameObject c = createcard(cid, playslots[i], zoomslot);
                        await Task.Delay(500);
                        Destroy(c);
                        c = createcard(cid, zoomslot, deckslot);
                        await Task.Delay(500);
                        Destroy(c);
                    }
                }
                else if ((rule.stat == TableState.REMOVE) || (rule.stat == TableState.DELETE))
                {
                    GameObject slot = playslots[i];
                    if (rule.stat == TableState.REMOVE)
                    {
                        Destroy(playcards[i]);
                        playcards[i] = null;
                    }
                    else
                    {
                        Destroy(handcards[0]);
                        handcards[0] = null;
                        slot = handslots[0];
                    }
                    if (id == 0)
                    {
                        // get the id for the card that tried to get added
                        for (int k = 0; k < results.Count; k++) if (results[k].stat == TableState.DECK) 
                            id = CardLibrary.idfor(results[k].amount.Trim('+'));
                        slot = deckslot;
                    }
                    GameObject c = createcard(id, slot, slot);
                    CardHandler h = c.GetComponent<CardHandler>();
                    h.scale = Vector2.zero;
                    h.pos.y -= 0.3f; // move up a bit
                    Destroy(Instantiate(particleburn, c.transform.position, Quaternion.identity), 4.0f);
                    audiosource.PlayOneShot(burnsound);
                }
                else if (rule.stat == TableState.PREVENT)
                {
                    // TODO: animation for prevention rule triggered
                    GameObject c = createcard(CardLibrary.idfor("Repayment"), deckslot, deckslot);
                    CardHandler h = c.GetComponent<CardHandler>();
                    h.scale = Vector2.zero;
                    h.pos.y -= 0.3f; // move up a bit
                    Destroy(Instantiate(particleburn, c.transform.position, Quaternion.identity), 4.0f);
                    audiosource.PlayOneShot(burnsound);
                    updatelayout();
                }
                else
                    await animaterule(rule, rule.id == 0 ? playslots[i].transform : null);
			}
			
            if (id != 0) {
                if (playcards[i])
                {
                    Destroy(playcards[i]);
                    Destroy(Instantiate(particleflash, createcard(id, playslots[i], discardslot).transform.position, 
                        Quaternion.identity), 2.0f);
                }
                updatestats();
                updatelayout();
                await Task.Delay(80);
            }
        }

        await discardhand();

        List<CardRule> rules;
        if (state.getval(TableState.QUARTER) < 4) { // shuffle and continue
            state.setval(TableState.DEALS, state.getval(TableState.MAXDEALS));
            rules = state.addval(TableState.QUARTER, 1);
            foreach (CardRule rule in rules) await animaterule(rule);
            isanimating = false;
            await dealhand();
            isanimating = true;
        }
        else // check for win/loss condition
        {
            // process end-of-year rules
            rules = state.processrules(TableState.QUARTER, "=5");
            foreach (CardRule rule in rules) await animaterule(rule);

            shuffledeck(); // end of year

            int rtype = ReportDialog.SUMMARY;
            string msg;

            if (state.getval(TableState.STABILITY) <= 0)
            {
                rtype = ReportDialog.LOSE;
                msg = "stability at zero\nsociety has collapsed";
                audiosource.PlayOneShot(crowdsound);
            }
            else if (state.getval(TableState.WEALTH) < state.getval(TableState.GOAL))
            {
                rtype = ReportDialog.LOSE;
                msg = "annual goal missed\nbusiness has collapsed";
            }
            else if (state.getval(TableState.YEAR) == 25)
            {
                int maxdecade = state.getval(TableState.MAXDECADE);
                if (maxdecade < 2000) state.setval(TableState.MAXDECADE, state.getval(TableState.STARTYEAR) + 10);
                rtype = ReportDialog.WIN;
                msg = "business survived 25 years\nsocietal stability at " + state.getval(TableState.STABILITY) + "%\nthe future is ";
                if (state.getval(TableState.STABILITY) >= 75) msg += "promising";
                else if (state.getval(TableState.STABILITY) >= 50) msg += "uncertain";
                else if (state.getval(TableState.STABILITY) >= 25) msg += "doubtful";
                else msg += "bleak";
            }
            else
            {
                audiosource.PlayOneShot(succeedsound);
                int wealth = state.getval(TableState.WEALTH);
                int capital = state.getval(TableState.CAPITAL);
                int growth = state.getval("growth");
                int newgoal = wealth + (int)(wealth * growth / 100);
                int newyear = state.getval(TableState.YEAR) + 1;
                msg = "business reached\n" + wealth + " wealth\n\ngoal for " + 
                    (state.getval(TableState.STARTYEAR) + newyear) + "\nis now " + 
                    wealth + " + " + growth + "% = "+ newgoal + 
                    "\ncapital = " + capital + " + " + wealth + " = " + (capital + wealth);
                state.setval(TableState.GOAL, newgoal);
                state.addval(TableState.CAPITAL, wealth);
            }

            if (rtype == ReportDialog.LOSE)
            {
                // perform crumbling animation
                GameObject savetable = table;
                await Task.Delay(1500);
                audiosource.PlayOneShot(collapsesound);
                iscrumbling = true;
                table = Instantiate(table); // make a complete copy
                tablelandscape.SetActive(false);
                tableportrait.SetActive(false);
                await Task.Delay(2000);
                fadehandler.fade();
                await Task.Delay(1000);
                Destroy(table);
                table = savetable;
                table.SetActive(true);
                iscrumbling = false;
            }

            if (rtype != ReportDialog.SUMMARY) state.isingame = false;
            await fadescreen();
            curdlg = Instantiate(reportdlg);
            ReportDialog r = curdlg.GetComponent<ReportDialog>();
            r.reporttype = rtype;
            r.msg = msg;
            fadehandler.fade(false);
        }

        isanimating = false;

        savestate();
    }

    void zoominto(int id = 0)
    {
        if (zoomcard != null) Destroy(zoomcard);
        if (id == 0) return;
        zoomcard = createcard(id, zoomslot, zoomslot);
        zoomcard.transform.localScale *= 0.8f;
        zoomcard.transform.position += (Vector3)(Vector2.up * 0.5f);
    }

    void deselectall()
    {
        // remove selections from all slots
        for (int i = 0; i < playslots.Count; i++) playslots[i].GetComponent<SlotHandler>().select(false);
        for (int i = 0; i < handslots.Count; i++) handslots[i].GetComponent<SlotHandler>().select(false);
        selectslot = null;
        drag = 0;
        if (dragcard != null) Destroy(dragcard);
        zoominto();
    }

    public void clickslot(GameObject slot)
    {
        SlotInfo info = slotinfo(slot);
        if (slot == zoomslot)
        {
            // TODO: show single-card details (use a text box like the stats)
            return; 
        }
        if ((info != null) && info.isperm)
        {
            deselectall();
            zoominto(info.ids[info.ix]);
            drag = 0;
            return;
        }
        if (slot == selectslot) {
            deselectall();
            drag = 0;
            selectslot = null;
            return;
        }
        if ((selectslot != null) && (selectslot != slot) && (slot.GetComponent<SlotHandler>().isactive))
        {
            // swap selected slot and clicked slot
            SlotInfo selinfo = slotinfo(selectslot);
            int selid = selinfo.ids[selinfo.ix];
            int newid = info.ids[info.ix];
            selinfo.ids[selinfo.ix] = newid;
            Destroy(selinfo.cards[selinfo.ix]);
            if (info.ids[info.ix] != 0)
            {
                // also move the target to where this came from
                selinfo.cards[selinfo.ix] = createcard(newid, slot, selectslot);
                Destroy(info.cards[info.ix]);
            }
            info.ids[info.ix] = selid;
            info.cards[info.ix] = createcard(selid, selectslot, slot);
            deselectall();
            updatestats(true);
        }
        else {
            if (slot == deckslot)
                showcardcollection("Deck Remaining", state.deck);
            else if (slot == discardslot)
                showcardcollection("Discards", state.discard);
            else if (slot == challengeslot)
                challengecard.SetActive(!challengecard.activeSelf);
            else if (slot == zoomslot)
            {
                // show a card details dialog
            }
            else if (info.cards[info.ix] != null)
            {
                Destroy(info.cards[info.ix]);
                drag = info.ids[info.ix];
                dragcard = createcard(drag, slot, slot);
                zoominto(drag);
                info.ids[info.ix] = 0;
            }
        }
    }

    public void releaseslot(GameObject slot)
    {
        if (drag != 0) audiosource.PlayOneShot(playsound); // when holding a card, make a sound

        // see if the card has been dropped on a different empty slot
        SlotInfo origin = slotinfo(slot);
        if (origin == null) return;

        Collider2D hit = Physics2D.OverlapPoint(maincam.ScreenToWorldPoint(Input.mousePosition));
        GameObject targetslot = slot;
        if (hit)
        {
            targetslot = hit.gameObject;
            if (!targetslot.GetComponent<SlotHandler>().isactive)
                targetslot = slot; // dropping on inactive slot is equivalent to empty/go back
        }

        if (drag != 0)
        {
            SlotInfo target = slotinfo(targetslot);
            if (target == null)
            {
                targetslot = slot;
                target = origin;
            }
            int targetid = target.ids[target.ix];
            bool isselecting = false;
            if (targetslot == slot)
            { // dropped on itself
                isselecting = selectslot == null;
                selectslot = isselecting ? slot : null;
            }
            slot.GetComponent<SlotHandler>().select(isselecting);
            if (!isselecting)
            {
                selectslot = null;
                zoominto();
            }

            if (targetid != 0)
            {
                // move the target to where this came from
                origin.ids[origin.ix] = targetid;
                Destroy(origin.cards[origin.ix]);
                origin.cards[origin.ix] = createcard(targetid, targetslot, slot);
            }
            target.ids[target.ix] = drag;
            Destroy(target.cards[target.ix]);
            target.cards[target.ix] = createcard(drag, dragcard, targetslot);

            if (dragcard != null) Destroy(dragcard);
            drag = 0;
            updatestats(true); // redraw stats based on new game state
        }
    }


    public async void showdialog(GameObject dialog)
    {
        tutorial.SetActive(false);
        if (curdlg) Destroy(curdlg);
        curdlg = Instantiate(dialog);
        curdlg.transform.SetParent(table.transform);
        await fadescreen(false);
    }

    public void showmenu()
    {
        showdialog(menudlg);
    }

    public void showcardcollection(string title, List<int> ids, GameObject returndlg = null)
    {
        showdialog(carddlg);
        curdlg.GetComponent<CardsDialog>().setup(title, ids, state, this, returndlg);        
    }

    public void showmainmenu()
    {
        showdialog(menudlg);
    }

    void fixcard(GameObject c, GameObject slot)
    {
        if (c == null) return;
        c.GetComponent<CardHandler>().fixcard(table, slot);
    }

    void setslotactive(GameObject s, bool isactive)
    {
        SpriteRenderer sp = s.GetComponent<SpriteRenderer>();
        Color c = sp.color;
        c.a = (isactive) ? 1.0f : 0.2f;
        sp.color = c;
        Transform t = s.transform.Find("backdrop");
        if (t != null) {
            sp = t.GetComponent<SpriteRenderer>();
            c = sp.color;
            c.a = (isactive) ? 0.1f : 0.0f;
            sp.color = c;
        }
        s.GetComponent<SlotHandler>().isactive = isactive;
    }

    void updatelayout()
    {
        if (zoomcard != null) Destroy(zoomcard);

        // assign slots
        int i;
        GameObject g;
        for (i = 0; i < TableState.MAX_SLOTS; i++) {
            g = GameObject.Find("slot" + i);
            setslotactive(g, i < state.getval(TableState.PLAYS));
            if (i >= playslots.Count) playslots.Add(g); else playslots[i] = g;
            if (i >= playcards.Count) playcards.Add(null);
            fixcard(playcards[i], g);
        }
        for (i = 0; i < TableState.MAX_HAND; i++) {
            g = GameObject.Find("hand" + i);
            setslotactive(g, i < state.getval(TableState.HAND));
            if (i >= handslots.Count) handslots.Add(g); else handslots[i] = g;
            if (i >= handcards.Count) handcards.Add(null);
            fixcard(handcards[i], g);
        }
        for (i = 0; i < TableState.MAX_PERMS; i++) {
            g = GameObject.Find("perma" + i);
            setslotactive(g, i < state.perms.Count);
            if (i >= permslots.Count) permslots.Add(g); else permslots[i] = g;
            if (i >= permcards.Count) permcards.Add(null);
            Destroy(permcards[i]); // always re-assign these
            permcards[i] = null;
            if (i < state.perms.Count) permcards[i] = createcard(state.perms[i], g, g);
            fixcard(permcards[i], g);
        }

        deckslot = GameObject.Find("deck");
        discardslot = GameObject.Find("discard");
        zoomslot = GameObject.Find("zoom");
        challengeslot = GameObject.Find("challenge");
        fixcard(challengecard, challengeslot);

        setslotactive(zoomslot, true);
        setslotactive(deckslot, true);
        setslotactive(discardslot, true);
        setslotactive(challengeslot, true);

        dealbutton = GameObject.Find("btndeal");
        menubutton = GameObject.Find("btnmenu");
        nextbutton = GameObject.Find("btnnext");
        setbuttontext(menubutton, "Menu");

        updatebuttons();

        wealthhandler = GameObject.Find("income").GetComponent<ProgressHandler>();
        wealthhandler.stat = "wealth";
        stablehandler = GameObject.Find("stability").GetComponent<ProgressHandler>();
        stablehandler.stat = TableState.STABILITY;
        yeardisplay = GameObject.Find("yeardsp").GetComponent<TextMeshPro>();
        yeardisplay.text = "Year: " + (state.getval(TableState.YEAR) + state.getval(TableState.STARTYEAR)) +
            " / " + (state.getval(TableState.STARTYEAR) + 25);
        capitaldisplay = GameObject.Find("capitaldsp").GetComponent<TextMeshPro>();
        // capitaldisplay.text = "Capital: " + state.getval(TableState.CAPITAL);
        statdisplay = GameObject.Find("statdsp").GetComponent<TextMeshPro>();

        if (curdlg) curdlg.transform.SetParent(table.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (iscrumbling) { // randomly lower and rotate all on-screen game objects
            foreach (Transform t in table.transform)
            {
                t.position += (Vector3)(new Vector2(0, -Random.Range(-1, 5)) * Time.deltaTime);
                t.eulerAngles += new Vector3(0, 0, Random.Range(-45, 45) * Time.deltaTime);
            }
        }
        else { // update active layout for orientation and screen size
            if (dragcard != null)
            {
                Vector3 saveangle = dragcard.transform.eulerAngles;
                dragcard.transform.eulerAngles = Vector3.zero;
                Vector3 mousepos = dragcard.transform.InverseTransformPoint(getmousepos());
                CardHandler dh = dragcard.GetComponent<CardHandler>();
                Vector2 dist = dragcard.transform.position - mousepos;
                if (dh.ismoving || (dist.magnitude > 0.1))
                {
                    // Debug.Log(dist.magnitude);
                    dh.pos = mousepos;
                    dh.ismoving = true;
                }
                dragcard.transform.eulerAngles = saveangle;
            }

            float h = maincam.scaledPixelHeight;
            float w = maincam.scaledPixelWidth;
            if ((h != chgt) || (w != cwid))
            {
                chgt = h;
                cwid = w;
                bool isportrait = h > w;
                bool isreorienting = (isportrait != (table == tableportrait));

                if (isportrait) // portrait 
                {
                    maincam.orthographicSize = defaultortho * ((h < (w * aspectratio)) ? aspectratio : h / w);
                    tablelandscape.SetActive(false);
                    table = tableportrait;
                }
                else
                {
                    maincam.orthographicSize = defaultortho * ((w > (h * aspectratio)) ? 1 : (h / w * aspectratio));
                    tableportrait.SetActive(false);
                    table = tablelandscape;
                }
                table.SetActive(true);
                if (isreorienting) updatelayout();
            }
        }
    }
}
