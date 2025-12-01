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

        items = new List<int>();
        for (int i = 0; i < SHELFCOUNT; i++)
            items.Add(CardLibrary.randomcard(i >= 8 ? "perm" : "", 0, items)); // TODO: replace placeholder data

        prices = new List<int>();
        cart = new List<bool>();
        for (int i = 0; i < SHELFCOUNT; i++)
        {
            prices.Add(1 + Random.Range(2, 5)); // TODO: replace placeholder data
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

        if (carttotal < capital)
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
                    if (info.type == "perm")
                        state.perms.Add(id);
                    else
                        state.mycards.Add(id);
                    /* cards/perms can do these
                    {
                        if (i == 8) { } // TODO: do card/perm remove
                        else if (i == 9) state.addval(TableState.MAXDEALS, 1);
                        else if (i == 10) state.addval(TableState.MAXHAND, 1);
                        else if (i == 11) state.addval(TableState.MAXPLAYS, 1);
                    }
                    */
                }
            }

            state.addval(TableState.CAPITAL, -carttotal);

            controller.fadehandler.fade(false);
            controller.startnewyear();
        }
    }

}
