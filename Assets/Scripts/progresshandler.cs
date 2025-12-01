using TMPro;
using UnityEngine;

public class ProgressHandler : MonoBehaviour
{
    public string stat;
    int total;
    int progress;
    TextMeshPro tmp;
    Transform bar;
    Transform nextbar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void setprogress(int p, int t, int n = -10000)
    {
        if (tmp == null) tmp = gameObject.transform.Find("valuetext").GetComponent<TextMeshPro>();
        if (bar == null) bar = gameObject.transform.Find("progress");
        if (nextbar == null) nextbar = gameObject.transform.Find("nextprog");
        progress = p;
        total = t;
        tmp.text = stat + " " + progress + ((n != -10000 && n != 0) ? " \u2192 " + (progress + n) + " / " : " / ") + total;
        if (p < 0) p = 0;
        bar.localScale = new Vector2(Mathf.Clamp((float)p / (float)t, 0f, 1.0f), bar.localScale.y);
        if (n != -10000) { // leave secondary bar alone if no difference is provided
            p += n;
            if (p < 0) p = 0;
            nextbar.localScale = new Vector2(Mathf.Clamp((float)p / (float)t, 0f, 1.0f), nextbar.localScale.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
