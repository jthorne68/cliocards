using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ButtonHandler : MonoBehaviour
{
    public UnityEvent pressevent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void settext(string text)
    {
        transform.Find("buttontext").GetComponent<TextMeshPro>().text = text;
    }

    void OnMouseDown()
    {
        pressevent.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
