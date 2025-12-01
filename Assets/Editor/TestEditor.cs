using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using static CardLibrary;
using static UnityEngine.Analytics.IAnalytic;

public class TestEditor : EditorWindow
{
    private CardData data;
    private TableState state;

    private VisualElement root;
    private ListView cardlist;
    private ListView decklist;
    List<int> cards;
    List<int> deck;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/TestEditor")]
    public static void ShowExample()
    {
        TestEditor wnd = GetWindow<TestEditor>();
        wnd.titleContent = new GUIContent("TestEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        root = rootVisualElement;

        // Instantiate UXML
        root.Add(m_VisualTreeAsset.Instantiate());

        data = new CardData();
        state = new TableState(data);
        state.newgame();

        cardlist = root.Q<ListView>("cards");
        cardlist.itemsSource = cards = new();
        cardlist.makeItem = () => new Label();
        cardlist.bindItem = (element, index) => {
            (element as Label).text = data.cardinfo(cards[index]).name; 
        };

        decklist = root.Q<ListView>("deck");
        decklist.itemsSource = deck = new();
        decklist.makeItem = () => new Label();
        decklist.bindItem = (element, index) => { 
            (element as Label).text = data.cardinfo(deck[index]).name; 
        };

        updatesim();

        root.Q<Button>("newbtn").clicked += onNewgame;
        root.Q<Button>("addbtn").clicked += onAddCard;
        root.Q<Button>("delbtn").clicked += onRemoveCard;
    }

    public void onNewgame()
    {
        updatesim();
    }

    public void onAddCard()
    {
        int sel = cardlist.selectedIndex;
        if (sel == -1) return;
        state.mycards.Add(sel);
        updatesim();
    }

    public void onRemoveCard()
    {
        int sel = decklist.selectedIndex;
        if (sel == -1) return;
        state.mycards.RemoveAt(sel);
        updatesim();
    }

    public void updatesim()
    {
        // update lists

        cards.Clear();
        for (int i = 0; i < data.totalcards(); i++) cards.Add(i);
        deck.Clear();
        for (int i = 0; i < state.mycards.Count; i++) deck.Add(state.mycards[i]);
        cardlist.RefreshItems();
        decklist.RefreshItems();

        // run simulation
        string result = "REPORT:\n";
        result += "Deck size: " + state.mycards.Count + "\n";

        int tries = 100;
        int totalw = 0;
        int minw = 1000;
        int maxw = 0;
        int totals = 0;
        int mins = 1000;
        int maxs = 0;

        // establish starting parameters
        state.setval(TableState.PLAYS, state.getval(TableState.MAXPLAYS));
        state.setval(TableState.WEALTH, 0);
        state.setval(TableState.STABILITY, 0);

        for (int t = 0; t < tries; t++)
        {
            TableState test = new TableState(state);
            test.shuffledeck();
            // just play the first three cards in the deck
            for (int c = 0; c < test.getval(TableState.PLAYS); c++) test.play[c] = test.deck[c];
            test.processplays();
            int w = test.getval(TableState.WEALTH);
            int s = test.getval(TableState.STABILITY);
            totalw += w;
            totals += s;
            if (w < minw) minw = w;
            if (w > maxw) maxw = w;
            if (s < mins) mins = s;
            if (s > maxs) maxs = s;
        }
        result += "\nWealth min: " + minw +
            "\nWealth max: " + maxw +
            "\nStability min: " + mins +
            "\nStability max: " + maxs +
            "\nWealth avg: " + (int)totalw / tries +
            "\nStability avg: " + (int)totals / tries;

        // display output
        root.Q<Label>("output").text = result;
    }
}
