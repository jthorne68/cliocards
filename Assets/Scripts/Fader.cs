using UnityEngine;
using UnityEngine.Rendering;

public class Fader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float direction = 0.0f;
    SpriteRenderer render;
    bool isfading = false;

    void Start()
    {
    }

    public void fade(bool isout = true)
    {
        if (render == null) {
            render = gameObject.GetComponent<SpriteRenderer>();
            render.color = Color.black;
        }
        isfading = true;
        direction = isout ? 1.0f : -1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isfading) return;
        Color c = render.color;
        c.a += direction * Time.deltaTime;
        if (direction > 0 && c.a > 1.0f) {
            c.a = 1.0f;
            isfading = false;
        }
        else if (direction < 0 && c.a < 0.0f)
        {
            c.a = 0.0f;
            isfading = false;
        }
        render.color = c;
    }
}
