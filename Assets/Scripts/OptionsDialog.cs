using UnityEngine;

public class OptionsDialog : MonoBehaviour
{
    public TableController controller;
    public GameObject fullscreenbutton;
    public GameObject windowedbutton;
    public GameObject soundbutton;
    public GameObject musicbutton;
    public GameObject dragdropbutton;
    public GameObject clickbutton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
        updatebuttons();
    }

    public void OnFullscreen()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        updatebuttons();
    }

    public void OnWindowed()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
        updatebuttons();
    }

    public void OnSound()
    {
        controller.state.issound = !controller.state.issound;
        controller.updateaudio();
        updatebuttons();

    }

    public void OnMusic()
    {
        controller.state.ismusic = !controller.state.ismusic;
        controller.updateaudio();
        updatebuttons();
    }

    public void OnDragDrop()
    {
        controller.state.isdragdrop = true;
        updatebuttons();
    }

    public void OnClickPlace()
    {
        controller.state.isdragdrop = false;
        updatebuttons();
    }

    public void setbuttonon(GameObject button, bool ison)
    {
        button.transform.Find("backdrop").GetComponent<SpriteRenderer>().color = 
            CardLibrary.instance.colorfor(ison ? 'B' : 'T');
    }

    public void updatebuttons()
    {
        setbuttonon(fullscreenbutton, Screen.fullScreenMode == FullScreenMode.FullScreenWindow);
        setbuttonon(windowedbutton, Screen.fullScreenMode == FullScreenMode.Windowed);
        setbuttonon(soundbutton, controller.state.issound);
        setbuttonon(musicbutton, controller.state.ismusic);
        setbuttonon(dragdropbutton, controller.state.isdragdrop);
        setbuttonon(clickbutton, !controller.state.isdragdrop);
    }

    async public void OnClose()
    {
        await controller.fadescreen();
        controller.showmenu();
        controller.savestate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
