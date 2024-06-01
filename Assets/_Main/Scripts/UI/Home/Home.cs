using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Home : CustomMonoBehaviour
{
    [SerializeField]
    Button btnLyThuyet;
    [SerializeField]
    Button btnThucHanh;
    // Start is called before the first frame update
    void Start()
    {
        btnLyThuyet.onClick.AddListener(LyThuyet);
        btnThucHanh.onClick.AddListener(ThucHanh);
    }
    void LyThuyet()
    {
        SceneManager.LoadScene("LyThuyet");
    }
    void ThucHanh()
    {
        SceneManager.LoadScene("ThucHanh");
    }

    //Not affect to game, only support drag activity
    #region editor
    protected override void LoadComponent()
    {
        base.LoadComponent();
        //_WeaponGroupCards = transform.Find("Background").gameObject;
        btnLyThuyet = transform.Find("Btn Ly Thuyet").GetComponent<Button>();
        btnThucHanh = transform.Find("Btn Ly Thuc Hanh").GetComponent<Button>();
    }
    #endregion

}
