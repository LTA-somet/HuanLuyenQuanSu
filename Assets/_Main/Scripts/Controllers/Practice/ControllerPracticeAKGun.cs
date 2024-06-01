using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPracticeAKGun : CustomMonoBehaviour
{
    //danh sách các bộ phận của model anim
    [SerializeField]
    private List<GameObject> listModelAnim = new List<GameObject>();
    public List<GameObject> ListModelAnim { get { return listModelAnim; } }

    //danh sách các bộ phận của model không có anim
    [SerializeField]
    private List<GameObject> listModel = new List<GameObject>();
    public List<GameObject> ListModel { get { return listModel; } }
    //danh sách tên các bộ phận của súng Ak
    [SerializeField]
    private List<string> listNamePartAKGun = new List<string>();
    public List<string> ListNamePartAKGun { get { return listNamePartAKGun; } }

    //model chứa animator
    [SerializeField]
    private GameObject ModelAnim;
    //model không chứa animator
    [SerializeField]
    private GameObject Model;

    [SerializeField]
    private PracticeSearchPartAKGun searchPartAKGun;
    public PracticeSearchPartAKGun SearchPartAKGun { get { return searchPartAKGun; } }

    [SerializeField]
    private PracticeAnimator practiceAnimator;
    public PracticeAnimator PracticeAnimator { get { return practiceAnimator; } }


    [SerializeField]
    private UIThucHanh uIThucHanh;
    public UIThucHanh UIThucHanh { get { return uIThucHanh; } }

    private string nameRunState;
    public string NameRunState { get { return nameRunState; } }

    private int numberModel = -1;
    public int NumberModel { get { return numberModel; } }

    public bool isRun = false;
    public bool isRunAnim = false;

    // reset về trang thái mặc định của súng khi thoát ra khỏi màn hình thực hành
    public void Init()
    {
        numberModel = -1;
        nameRunState = null;
        practiceAnimator.Init();
        practiceAnimator.Animator.Play("Take 00");
        if (isRun)
        {
            this.DispatchEvent(EventKey.REQUEST_ENABLE_ANIMATION_CAMERA);
        }
        else
        {
            this.DispatchEvent(EventKey.REQUEST_DISABLE_ANIMATION_CAMERA);
        }
    }

    //kiểm tra xem người dùng chọn đúng bước tiếp thao không
    public void CheckNameRunState(string newName)
    {
        if (isRun && isRunAnim)
        {
            if (uIThucHanh.IsThao)
            {
                if (newName == nameRunState || newName != listNamePartAKGun[numberModel + 1] ||
                practiceAnimator.IsExitState || listNamePartAKGun.Count == 0) return;
                nameRunState = newName;
                numberModel++;
                if (newName == listNamePartAKGun[numberModel])
                {
                    practiceAnimator.CheckState();
                    practiceAnimator.ChangeCurrentState(newName);
                }

            }
            else
            {
                if (listNamePartAKGun.Count - (numberModel + 2) < 0 || newName == nameRunState || listNamePartAKGun.Count == 0 || newName != listNamePartAKGun[listNamePartAKGun.Count - (numberModel + 2)] ||
                practiceAnimator.IsExitState) return;
                nameRunState = newName + " 0";
                numberModel++;
                if (newName == listNamePartAKGun[listNamePartAKGun.Count - (numberModel + 1)])
                {
                    practiceAnimator.ChangeCurrentState(newName + " 0");
                    practiceAnimator.CheckStateLap();
                }

            }
        }

    }
    //chạy khi nhấn nút reset trên editor không liên quan đến game chỉ hỗ trợ kéo thả
    #region editor
    protected override void ResetValue()
    {
        base.ResetValue();
    }
    protected override void LoadComponent()
    {
        base.LoadComponent();
        ModelAnim = transform.Find("Model Anim").gameObject;
        Model = transform.Find("Model").gameObject;

        listModelAnim.Add(ModelAnim.transform.Find("Bang dan ").gameObject);
        listModelAnim.Add(ModelAnim.transform.Find("Que_thong_nong").gameObject);
        listModelAnim.Add(ModelAnim.transform.Find("Vo_than_tren").gameObject);
        listModelAnim.Add(ModelAnim.transform.Find("Bo_giat_lai").gameObject);
        listModelAnim.Add(ModelAnim.transform.Find("bo nong").gameObject);
        listModelAnim.Add(ModelAnim.transform.Find("bo nong").transform.Find("Group002").gameObject);
        listModelAnim.Add(ModelAnim.transform.Find("Group004").gameObject);
        listModelAnim.Add(ModelAnim.transform.Find("Object004").gameObject);


        listModel.Add(Model.transform.Find("Bang dan ").gameObject);
        listModel.Add(Model.transform.Find("Que_thong_nong").gameObject);
        listModel.Add(Model.transform.Find("Vo_than_tren").gameObject);
        listModel.Add(Model.transform.Find("Bo_giat_lai").gameObject);
        listModel.Add(Model.transform.Find("bo nong").gameObject);
        listModel.Add(Model.transform.Find("bo nong").transform.Find("Group002").gameObject);
        listModel.Add(Model.transform.Find("Group004").gameObject);
        listModel.Add(Model.transform.Find("Object004").gameObject);

        listNamePartAKGun.Add(ModelAnim.transform.Find("Bang dan ").gameObject.name);
        listNamePartAKGun.Add(ModelAnim.transform.Find("Que_thong_nong").gameObject.name);
        listNamePartAKGun.Add(ModelAnim.transform.Find("Vo_than_tren").gameObject.name);
        listNamePartAKGun.Add(ModelAnim.transform.Find("Bo_giat_lai").gameObject.name);
        listNamePartAKGun.Add(ModelAnim.transform.Find("bo nong").gameObject.name);
        listNamePartAKGun.Add(ModelAnim.transform.Find("bo nong").transform.Find("Group002").gameObject.name);
        listNamePartAKGun.Add(ModelAnim.transform.Find("Group004").gameObject.name);

        searchPartAKGun = transform.Find("PracticeSearchPartAKGun").GetComponent<PracticeSearchPartAKGun>();
        practiceAnimator = ModelAnim.GetComponent<PracticeAnimator>();
        uIThucHanh = GameObject.Find("ThucHanh").GetComponent<UIThucHanh>();
    }
    #endregion
}
