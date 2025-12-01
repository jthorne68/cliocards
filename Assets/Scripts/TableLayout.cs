using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TableLayout : MonoBehaviour
{
    int MAX_SLOTS = 12;
    int MAX_ROWS = 2;
    int MAX_COLUMNS = 6;
    int PORTRAIT_MAX_ROWS = 3;
    int PORTRAIT_MAX_COLUMNS = 4;

    int MAX_HAND = 8;

    float column_width;
    float row_height;

    int w = 0;
    int h = 0;

    public GameObject slot;
    public List<GameObject> slots = new List<GameObject>();
    public List<GameObject> handslots = new List<GameObject>();

    void Start()
    {
        int i;
        for (i = 0; i < MAX_SLOTS; i++)
            slots.Add(Instantiate(slot, new Vector2(), Quaternion.identity));
        for (i = 0; i < MAX_HAND; i++)
            handslots.Add(Instantiate(slot, new Vector2(), Quaternion.identity));
        Vector2 size = slot.GetComponent<SpriteRenderer>().bounds.size;
        float pad = size.x * 0.1f;
        column_width = size.x + pad;
        row_height = size.y + pad;
        Update();
    }

    void Update()
    {
        int neww = Screen.width;
        int newh = Screen.height;
        if ((neww == w) && (newh == h)) return;
        w = neww;
        h = newh;

        // default landscape layout
        int maxcols = MAX_COLUMNS;
        int maxrows = MAX_ROWS;
        int handcols = 8;
        float leftx = -(column_width * (maxcols - 0.5f) / 2);
        float topy = -(row_height * (maxrows - 0.5f) / 2);
        float handleftx = leftx - column_width;

        if (h > w)
        {
            // portrait layout
            maxcols = PORTRAIT_MAX_COLUMNS;
            maxrows = PORTRAIT_MAX_ROWS;
            handcols = 4;
            handleftx = leftx;
        }
        float handtopy = topy - row_height * maxrows;

        int col = 0;
        int row = 0;
        int i;
        float x = 0, y = 0;
        for (i = 0; i < MAX_SLOTS; i++)
        {
            x = col * column_width;
            y = row * row_height;
            slots[i].transform.position = new Vector2(leftx + x, topy - y);
            col++;
            if (col == maxcols)
            {
                col = 0;
                row++;
            }
        }
        col = 0;
        row = 0;
        for (i = 0; i < MAX_HAND; i++)
        {
            x = col * column_width;
            y = row * row_height;
            Transform t = handslots[i].transform;
            t.position = new Vector2(handleftx + x, handtopy - y);
            col++;
            if (col == handcols)
            {
                col = 0;
                row++;
            }
            SpriteRenderer sp = t.GetComponent<SpriteRenderer>();
            sp.color = Color.darkGray;
        }
    }
}