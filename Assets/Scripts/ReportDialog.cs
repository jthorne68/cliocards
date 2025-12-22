using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class ReportDialog : MonoBehaviour
{
    TableController controller;

    public const int SUMMARY = 0;
    public const int WIN = 1;
    public const int LOSE = 2;
    public int reporttype = SUMMARY;
    public string msg;

    public GameObject summary;
    public GameObject win;
    public GameObject loss;
    public GameObject message;

    public AudioClip winmusic;
    public AudioClip losemusic;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
        if (reporttype == WIN)
        {
            win.SetActive(reporttype == WIN);
            GetComponent<ParticleSystem>().Play();
            audio.clip = winmusic;
            audio.Play();
        }
        else if (reporttype == LOSE)
        {
            loss.SetActive(reporttype == LOSE);
            audio.clip = losemusic;
            audio.Play();
        }
        else if (reporttype == SUMMARY)
        {
            summary.SetActive(reporttype == SUMMARY);
        }
        message.GetComponent<TextMeshPro>().text = msg;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async public void OnClose()
    {
        await controller.fadescreen();
        Destroy(gameObject);
        if (reporttype != SUMMARY) {
            controller.state.isingame = false;
            controller.showmenu();
        }
        else
        {
            controller.showdialog(controller.storedlg);
        }
    }

}
