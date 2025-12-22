using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering;

public class StoreDialog : MonoBehaviour
{
    const int SHELFCOUNT = 11;

    TableController controller;
    TableState state;

    List<GameObject> shelves;
    List<GameObject> cards;
    List<int> prices;
    List<int> items;

    List<bool> cart;

    TextMeshPro total;

    public int capital = 0;
    public int carttotal = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
        state = controller.state;
        total = transform.Find("total").gameObject.GetComponent<TextMeshPro>();

        capital = state.getval(TableState.CAPITAL);

        items = new();
        List<int> invalidperms = new();
        invalidperms.AddRange(state.perms); // no duplicate permanents
        if (state.getval(TableState.YEAR) % 5 != 0)
        {
            // after boss fight, allow these plan-increasing permanents to appear
            invalidperms.Add(CardLibrary.idfor("Distrust"));
            invalidperms.Add(CardLibrary.idfor("Diversification"));
            invalidperms.Add(CardLibrary.idfor("Monopoly"));
            invalidperms.Add(CardLibrary.idfor("Consolidation"));
            invalidperms.Add(CardLibrary.idfor("Moral Panic"));
            invalidperms.Add(CardLibrary.idfor("Migrant Labor"));
            invalidperms.Add(CardLibrary.idfor("Deficit Spending"));
            invalidperms.Add(CardLibrary.idfor("Expansion"));
        }
        for (int i = 0; i < SHELFCOUNT; i++)
        {
            bool isperm = i >= 8;
            int storecard = CardLibrary.randomcard(isperm ? "perm" : "", 0, isperm ? invalidperms : items);
            items.Add(storecard);
            invalidperms.Add(storecard);
        }

        prices = new List<int>();
        cart = new List<bool>();
        int lowprice = 20;
        int highprice = 40;
        int inflation = state.getval("inflation");
        for (int i = 0; i < state.getval(TableState.YEAR); i++)
        {
            lowprice = lowprice + (int)(lowprice * inflation / 100);
            highprice = highprice + (int)(highprice * inflation / 100);
        }
        for (int i = 0; i < SHELFCOUNT; i++)
        {
            prices.Add(1 + Random.Range(lowprice, highprice)); // TODO: replace placeholder data
            cart.Add(false);
        }

        shelves = new List<GameObject>();
        cards = new List<GameObject>();
        for (int i = 0; i < SHELFCOUNT; i++) {
            GameObject shelf = transform.Find("shelf" + i).gameObject;
            shelves.Add(shelf);
            GameObject c = Instantiate(CardLibrary.instance.card, shelf.transform.position, Quaternion.identity);
            c.transform.SetParent(transform);
            c.transform.localScale = shelf.transform.localScale;
            CardLibrary.instance.setupcard(c, items[i], state);
            c.GetComponent<SortingGroup>().sortingLayerName = "UI Cards";
            c.GetComponent<CardHandler>().ismoving = false;
            cards.Add(c);
        }
        /*
        shelves.Add(transform.Find("btnremove").gameObject);
        shelves.Add(transform.Find("btndeals").gameObject);
        shelves.Add(transform.Find("btnhand").gameObject);
        shelves.Add(transform.Find("btnplays").gameObject);
        */

        updatestats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void updatestats()
    {
        carttotal = 0;
        for (int i = 0; i < SHELFCOUNT; i++) {
            transform.Find("cost" + i).gameObject.GetComponent<TextMeshPro>().text = "" + prices[i];
            if (cart[i]) carttotal += prices[i];
        }
        total.text = "CART\n" + capital + "\n- " + carttotal + "\n= " + (capital - carttotal);
    }

    public void buyitem(int itemnum)
    {
        GameObject shelf = shelves[itemnum];
        cart[itemnum] = !cart[itemnum];
        if (cart[itemnum]) controller.audiosource.PlayOneShot(controller.buysound);
        cards[itemnum].SetActive(!cart[itemnum]);
        updatestats();
    }

    public async void checkout()
    {

        if (carttotal <= capital)
        {
            await controller.fadescreen();

            // subtract the cash buy the stuff
            Destroy(gameObject);

            for (int i = 0; i < SHELFCOUNT; i++)
            {
                if (cart[i])
                {
                    int id = items[i];
                    CardInfo info = CardLibrary.cardinfo(id);
                    if (info.type == "perm") {
                        state.processrulesforcard(id); // process any immediate effects
                        state.perms.Add(id); // save permanently
                    }
                    else
                        state.mycards.Add(id);
                }
            }

            state.addval(TableState.CAPITAL, -carttotal);

            controller.fadehandler.fade(false);
            controller.startnewyear();
        }
    }

}
