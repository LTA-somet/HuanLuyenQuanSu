using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LyThuyet : CustomMonoBehaviour
{
    [SerializeField]
    private Button btnNextSlide;
    [SerializeField]
    private Button btnPrevSlide;
    [SerializeField]
    private Image imgContent;
    private int index;
    private int maxIndex;
    private int minIndex;
    private string name;
    // Start is called before the first frame update
    void Start()
    {
        Init();
        //btnBackHome.onClick.AddListener(BackHome);
        //btnLyThuyetLap.onClick.AddListener(LyThuyLap);
        //btnLyThuyetThao.onClick.AddListener(LyThuyetThao);
        btnNextSlide.onClick.AddListener(NextSlide);
        btnPrevSlide.onClick.AddListener(PrevSlide);
    }
    void Init()
    {
        index = minIndex = 38;
        maxIndex = 43;
        name = "Disassembly theory of AK";
        SetImage();
        btnNextSlide.gameObject.SetActive(true);
        btnPrevSlide.gameObject.SetActive(false);
    }
    private void BackHome()
    {
        SceneManager.LoadScene("Home");
    }
    public void LyThuyLap()
    {
        index = minIndex = 44;
        maxIndex = 49;
        name = "Assembly theory of AK";
        SetImage();
        btnNextSlide.gameObject.SetActive(true);
        btnPrevSlide.gameObject.SetActive(false);
    }
    public void LyThuyetThao()
    {
        index = minIndex = 38;
        maxIndex = 43;
        name = "Disassembly theory of AK";
        SetImage();
        btnNextSlide.gameObject.SetActive(true);
        btnPrevSlide.gameObject.SetActive(false);
    }
    public void LyThuyetCauTao()
    {
        index = minIndex = 7;
        maxIndex = 37;
        name = "Structure of AK";
        SetImage();
        btnNextSlide.gameObject.SetActive(true);
        btnPrevSlide.gameObject.SetActive(false);
    }
    public void NextSlide()
    {
        btnPrevSlide.gameObject.SetActive(true);
        if (index >= maxIndex - 1)
        {
            btnNextSlide.gameObject.SetActive(false);
            //return;
        }
        index++;
        SetImage();
    }
    public void PrevSlide()
    {
        btnNextSlide.gameObject.SetActive(true);
        if (index <= minIndex + 1)
        {
            btnPrevSlide.gameObject.SetActive(false);
            //  return;
        }
        index--;
        SetImage();
    }
    private void SetImage()
    {
        imgContent.sprite = loadImage(name + " " + index + ".png");
    }
    public void Pause()
    {
        Time.timeScale = 0;
    }
    public void Resume()
    {
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public Sprite loadImage(string namePath)
    {
        //Create an array of file paths from which to choose
        var filePath = Application.streamingAssetsPath + "/Slides/" + namePath;  //Get path of file
        //Converts desired path into byte array
        byte[] pngBytes = System.IO.File.ReadAllBytes(filePath);

        //Creates texture and loads byte array data to create image
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(pngBytes);

        //Creates a new Sprite based on the Texture2D
        Sprite fromTex = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        //Assigns the UI sprite
        return fromTex;
    }
    protected override void LoadComponent()
    {
        base.LoadComponent();
        Transform topLeft = transform.Find("Top").Find("Left");
        //btnBackHome = topLeft.Find("btn BackHome").GetComponent<Button>();
        //btnLyThuyetLap = topLeft.Find("GameObject").Find("btn Ly Thuyet Lap").GetComponent<Button>();
        //btnLyThuyetThao = topLeft.Find("GameObject").Find("btn Ly Thuyet Thao").GetComponent<Button>();
        Transform content = transform.Find("Content");
        btnNextSlide = content.Find("Btn Next").GetComponent<Button>();
        btnPrevSlide = content.Find("Btn Back").GetComponent<Button>();
        imgContent = content.Find("Image").GetComponent<Image>();
    }
}
