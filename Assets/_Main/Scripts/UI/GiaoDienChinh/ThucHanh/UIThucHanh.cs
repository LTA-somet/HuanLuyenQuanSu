using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIThucHanh : CustomMonoBehaviour
{
    [SerializeField]
    private Button btnThao;
    [SerializeField]
    private Button btnLap;
    [SerializeField]
    private Button btnBackHome;
    [SerializeField]
    private Button btnBackMenu;
    [SerializeField]
    private Transform menuUI;
    [SerializeField]
    private ControllerPracticeAKGun controllerPracticeAKGun;
    [SerializeField]
    private TextMeshProUGUI textHind;
    private float timeHideHind = 5;
    private bool isThao = true;
    public bool IsThao { get { return isThao; } }

    // Start is called before the first frame update
    void Start()
    {
        //btnLap.onClick.AddListener(Lap);
        //btnThao.onClick.AddListener(Thao);
        //btnBackHome.onClick.AddListener(BackHome);
        //btnBackMenu.onClick.AddListener(BackMenu);
    }

    // Update is called once per frame
    void Update()
    {
        if (timeHideHind < 0)
        {
            HideHind();
        }
        else
        {
            timeHideHind -= Time.deltaTime;
        }
    }
    public void Thao()
    {
        isThao = true;
        controllerPracticeAKGun.isRun = true;
        controllerPracticeAKGun.Init();
        HideHind();
        //menuUI.gameObject.SetActive(false);
        var listModel = controllerPracticeAKGun.ListModel;
        var listModelAnim = controllerPracticeAKGun.ListModelAnim;
        for (int i = 0; i < listModel.Count; i++)
        {
            if (listModelAnim[i].name == "Group002") continue;
            listModel[i].SetActive(false);
            listModelAnim[i].SetActive(true);
        }


    }
    public void Lap()
    {
        isThao = false;
        controllerPracticeAKGun.isRun = true;
        controllerPracticeAKGun.Init();
        HideHind();
        //menuUI.gameObject.SetActive(false);
        var listModel = controllerPracticeAKGun.ListModel;
        var listModelAnim = controllerPracticeAKGun.ListModelAnim;
        for (int i = 0; i < listModel.Count; i++)
        {
            if (listModelAnim[i].name == "Group002") continue;
            listModel[i].SetActive(true);
            listModelAnim[i].SetActive(false);
        }


    }
    public void CheckHind()
    {
        string text;
        if (isThao)
        {
            text = controllerPracticeAKGun.ListNamePartAKGun[controllerPracticeAKGun.NumberModel + 1];
        }
        else
        {
            text = controllerPracticeAKGun.ListNamePartAKGun[controllerPracticeAKGun.ListNamePartAKGun.Count - (controllerPracticeAKGun.NumberModel + 2)];
        }
        switch (text)
        {
            case "Bang dan ":
                {
                    text = "Băng đạn";
                    break;
                }
            case "Que_thong_nong":
                {
                    text = "que thông nòng";
                    break;
                }
            case "Vo_than_tren":
                {
                    text = "vỏ thân trên";
                    break;
                }
            case "Bo_giat_lai":
                {
                    text = "bộ giật lại";
                    break;
                }
            case "bo nong":
                {
                    text = "bó nòng";
                    break;
                }
            case "Group002":
                {
                    text = "khoá nòng";
                    break;
                }
            case "Group004":
                {
                    text = "vỏ tay cầm";
                    break;
                }

            default:
                break;
        }
        timeHideHind = 5;
        ShowHind(text);
    }
    public void ShowHind(string text)
    {
        textHind.transform.parent.gameObject.SetActive(true);
        textHind.text = $"Nhấn vào bộ phận {text}";
    }
    private void HideHind()
    {
        textHind.transform.parent.gameObject.SetActive(false);
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void BackMenu()
    {
        //menuUI.gameObject.SetActive(true);
        controllerPracticeAKGun.isRun = false;
        controllerPracticeAKGun.Init();
    }
    protected override void LoadComponent()
    {
        base.LoadComponent();
        menuUI = transform.Find("Menu Thuc Hanh");
        btnThao = menuUI.transform.Find("Btn Thao").GetComponent<Button>();
        btnLap = menuUI.transform.Find("Btn Lap").GetComponent<Button>();
        btnBackHome = menuUI.transform.Find("Btn Back Home").GetComponent<Button>();
        btnBackMenu = transform.Find("Btn Back Menu").GetComponent<Button>();
        controllerPracticeAKGun = GameObject.Find("Sung AK").GetComponent<ControllerPracticeAKGun>();
    }
}
