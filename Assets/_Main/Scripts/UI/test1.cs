using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class test1 : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI textMeshPro;
    // Start is called before the first frame update
    void Start()
    {
        ///var m_Path = Application.dataPath;

        //Output the Game data path to the console
        //Debug.Log("dataPath : " + m_Path);
        //textMeshPro.text = m_Path;

    }
    void OnMouseEnter()
    {
        Debug.Log("OnMouseEnter");
    }
    private void OnMouseOver()
    {
        Debug.Log("OnMouseOver");
    }

    // Update is called once per frame
    void Update()
    {
        //sDebug.Log(DateTime.Now.TimeOfDay);
    }
}
