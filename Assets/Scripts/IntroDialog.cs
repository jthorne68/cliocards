using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class IntroDialog : MonoBehaviour
{
    TableController controller;

    public GameObject[] text;
    public GameObject ruin1;
    public GameObject ruin2;

    public List<TextMeshPro> texts = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.Find("TableController").GetComponent<TableController>();
        showintro();
    }

    async void showintro()
    {
        try
        {
            await Task.Delay(2000, destroyCancellationToken);
            if (text[0] != null) texts.Add(text[0].GetComponent<TextMeshPro>());
            await Task.Delay(3000, destroyCancellationToken);
            if (text[1] != null) texts.Add(text[1].GetComponent<TextMeshPro>());
            await Task.Delay(4000, destroyCancellationToken);
            if (text[2] != null) texts.Add(text[2].GetComponent<TextMeshPro>());
            await Task.Delay(4000, destroyCancellationToken);
            if (gameObject != null) OnClose();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async void OnClose()
    {
        await controller.fadescreen();
        Destroy(gameObject);
        controller.showmenu();
    }

    // Update is called once per frame
    void Update()
    {
        ruin1.transform.position += Vector3.up * 0.5f * Time.deltaTime;
        ruin2.transform.position += Vector3.up * 0.5f * Time.deltaTime;
        foreach (TextMeshPro t in texts) {
            float a = t.color.a + Time.deltaTime;
            if (a > 1.0f) a = 1.0f;
            t.color = new Color(1.0f, 1.0f, 1.0f, a);
        }
    }
}
