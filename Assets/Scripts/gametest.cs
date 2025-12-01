using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
public class GameTest : MonoBehaviour
{
    public GameObject card;
    public GameObject table;
    public GameObject tablelandscape;
    public GameObject tableportrait;

    public Vector2 pos;
    public Vector2 scale;

    public Camera maincam;
    float cwid = 0f;
    float chgt = 0f;
    float defaultortho = 5.0f;

    float aspectratio = 16.0f / 9.0f;

    public Sprite[] art1list;
    public Sprite[] art2list;

    void Start()
    {
        defaultortho = maincam.orthographicSize;
        /*
        for (int i = 0; i < 3; i++)
        {
            GameObject c = Instantiate(card, new Vector2(i * 3, 0), Quaternion.identity);
            c.transform.localScale = new Vector2((i + 1), (i + 1));
            Transform t = c.transform.Find("name");
            TextMeshPro tmp = t.GetComponent<TextMeshPro>();
            tmp.text = "Card " + i;
            t = c.transform.Find("text");
            tmp = t.GetComponent<TextMeshPro>();
            tmp.text = "Rules " + i;
            t = c.transform.Find("art1");
            SpriteRenderer sp = t.GetComponent<SpriteRenderer>();
            sp.sprite = art1list[i];
            // sp.color = Color.darkRed;
            t = c.transform.Find("art2");
            sp = t.GetComponent<SpriteRenderer>();
            // sp.color = Color.red;
            sp.sprite = art2list[i];
        }
        */
    }
    
    public void setCardTarget(Transform transform)
    {
        this.pos = transform.position;
        this.scale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (pos != Vector2.zero) {
            float moveby = 10.0f * Time.deltaTime;
            Vector2 cur = card.transform.position;
            Vector2 move = new Vector2((pos.x - cur.x) * moveby, (pos.y - cur.y) * moveby);
            if (move.magnitude < 0.001)
            {
                card.transform.position = pos;
                card.transform.localScale = scale;
                pos = Vector2.zero;
            }
            else
            {
                card.transform.position += (Vector3)move;
                cur = card.transform.localScale;
                card.transform.localScale += new Vector3((scale.x - cur.x) * moveby, (scale.y - cur.y) * moveby, 0);
            }
        }

        float h = maincam.scaledPixelHeight;
        float w = maincam.scaledPixelWidth;
        if ((h != chgt) || (w != cwid))
        {
            chgt = h;
            cwid = w;
            // Debug.Log($"cw:{w} ch:{h}, pw:{maincam.pixelWidth}, ph:{maincam.pixelHeight}, os:{maincam.orthographicSize}");
            // don't clip the 16:9 table space
            if (h > w) // portrait 
            {
                maincam.orthographicSize = defaultortho * ((h < (w * aspectratio)) ? aspectratio : h/w); 
                tablelandscape.SetActive(false);
                table = tableportrait;
            }
            else
            {
                maincam.orthographicSize = defaultortho * ((w > (h * aspectratio)) ? 1 : (h/w * aspectratio));
                tableportrait.SetActive(false);
                table = tablelandscape;
            }
            table.SetActive(true);
        }
    }
}
