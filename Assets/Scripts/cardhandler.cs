using UnityEngine;

public class CardHandler : MonoBehaviour
{
    public bool ismoving = false;
    public bool isdead = false;
    public Vector2 pos;
    public Vector2 scale;

    public void movetoward(Transform t)
    {
        pos = t.position;
        scale = t.localScale;
        ismoving = true;
    }

    public void fixcard(GameObject newparent, GameObject slot)
    {
        Transform t = gameObject.transform;
        t.SetParent(newparent.transform);
        t.position = slot.transform.position;
        t.localScale = slot.transform.localScale;
        if (slot.name.StartsWith("perma"))
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            t.Find("cardbackshadow").gameObject.SetActive(false);
            t.Find("rulebg").gameObject.SetActive(false);
            t.Find("name").gameObject.SetActive(false);
            t.Find("text").gameObject.SetActive(false);
        }
        ismoving = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (ismoving)
        {
            Transform t = gameObject.transform;
            t.eulerAngles = Vector3.zero;
            float moveby = 10.0f * Time.deltaTime;
            Vector2 cur = t.position;
            Vector2 move = new Vector2((pos.x - cur.x) * moveby, (pos.y - cur.y) * moveby);
            if (move.magnitude < 0.0002)
            {
                t.position = pos;
                t.localScale = scale;
                ismoving = false;
                if (isdead) 
                    Destroy(gameObject);
            }
            else
            {
                t.position += (Vector3)move;
                cur = t.localScale;
                t.localScale += new Vector3((scale.x - cur.x) * moveby, (scale.y - cur.y) * moveby, 0);
                t.eulerAngles = new Vector3(0, 0, -(move.x * 10 / moveby));
            }
        }
    }
}
