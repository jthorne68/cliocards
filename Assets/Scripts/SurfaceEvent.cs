using UnityEngine;

public class SurfaceEvent : MonoBehaviour
{
    public CardsDialog dlg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void OnMouseDown()
    {
        dlg.surfacemousedown();
    }

    private void OnMouseUp()
    {
        dlg.surfacemouseup();
    }

    private void OnMouseDrag()
    {
        dlg.surfacemousedrag();
    }

    private void OnMouseWheel()
    {
        dlg.surfacemousewheel();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mouseScrollDelta.y != 0f) dlg.surfacemousewheel();
    }
}
