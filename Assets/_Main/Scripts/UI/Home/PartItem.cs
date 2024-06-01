using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartItem : CustomMonoBehaviour
{
    [SerializeField]
    private Image _imgPartIcon;
    public Image ImgPartIcon { get { return _imgPartIcon; } }
    [SerializeField]
    private TextMeshProUGUI _textPartOrderName;
    [SerializeField]
    private TextMeshProUGUI _textPartTitle;
    [SerializeField]
    private Button button;
    private int index;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnEnable()
    {
        button.onClick.AddListener(changeData);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {

    }
    void changeData()
    {
        //_muc3UI.ChangePart(index);
    }

    public void SetUpUI(string partOrderName, string partTitle, string partIconpath, int newIndex)
    {
        _textPartOrderName.text = partOrderName;
        //Debug.Log("PartOrder Name: " + partOrderName);
        _textPartTitle.text = partTitle;
        //Debug.Log("PartOrder Title: " + partTitle);
        index = newIndex;
        loadImage(partIconpath);
    }

    private void loadImage(string imgPath)
    {

        //Create an array of file paths from which to choose
        if (imgPath.Trim() == "")
        {

            imgPath = "/Images/Circle.png";
        }
        var filePath = Application.streamingAssetsPath + imgPath;  //Get path of file
        //Converts desired path into byte array
        byte[] pngBytes = System.IO.File.ReadAllBytes(filePath);

        //Creates texture and loads byte array data to create image
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(pngBytes);

        //Creates a new Sprite based on the Texture2D
        Sprite fromTex = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        //Assigns the UI sprite
        _imgPartIcon.sprite = fromTex;
    }

    public void MouseEnter()
    {
        _textPartTitle.fontStyle |= TMPro.FontStyles.Underline;
        _textPartOrderName.fontStyle |= TMPro.FontStyles.Underline;
    }
    public void MouseExit()
    {
        _textPartTitle.fontStyle = _textPartTitle.fontStyle & ~TMPro.FontStyles.Underline;
        _textPartOrderName.fontStyle = _textPartOrderName.fontStyle & ~TMPro.FontStyles.Underline;
    }

    //không ảnh hưởng đến game chỉ giúp hỗ trợ kéo thả 
    #region editor
    protected override void LoadComponent()
    {
        base.LoadComponent();
        _textPartOrderName = transform.Find("Part Order Name").GetComponent<TextMeshProUGUI>();
        _textPartTitle = transform.Find("Part Title").GetComponent<TextMeshProUGUI>();
        _imgPartIcon = transform.Find("Part Icon").GetComponent<Image>();
        button = transform.Find("Part Btn").GetComponent<Button>();
        //_muc3UI = GameObject.Find("Muc 3").GetComponent<Muc3UI>();
    }
    #endregion
}
