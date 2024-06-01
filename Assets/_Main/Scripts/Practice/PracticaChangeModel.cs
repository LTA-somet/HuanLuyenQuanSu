using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PracticaChangeModel : CustomMonoBehaviour
{
    [SerializeField]
    private ControllerPracticeAKGun controllerPracticeAKGun;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ChangeModel();
    }
    //tắt bộ phận của model chứa anim và mở model không chứa anim và ngược lại để sửa lỗi animation 
    private void ChangeModel()
    {

        if (controllerPracticeAKGun.isRun)
        {
            var listModel = controllerPracticeAKGun.ListModel;
            var numberModel = controllerPracticeAKGun.NumberModel;
            if (controllerPracticeAKGun.UIThucHanh.IsThao)
            {
                numberModel = controllerPracticeAKGun.NumberModel;
            }
            else
            {
                numberModel = controllerPracticeAKGun.ListNamePartAKGun.Count - (controllerPracticeAKGun.NumberModel + 1);
            }
            var listModelAnim = controllerPracticeAKGun.ListModelAnim;
            if (!controllerPracticeAKGun.PracticeAnimator.IsChangeModel || numberModel < 0 || listModel[numberModel].name == "bo nong") return;

            if (listModelAnim[numberModel].name == "Group002")
            {

                listModel[numberModel].transform.parent.gameObject.SetActive(!listModel[numberModel].activeInHierarchy);
                listModelAnim[numberModel].transform.parent.gameObject.SetActive(!listModelAnim[numberModel].activeInHierarchy);

                controllerPracticeAKGun.PracticeAnimator.ChangeModel();

            }
            else if (listModelAnim[numberModel].name == "Bo_giat_lai")
            {
                listModel[numberModel].gameObject.SetActive(!listModel[numberModel].activeInHierarchy);
                listModelAnim[numberModel].gameObject.SetActive(!listModelAnim[numberModel].activeInHierarchy);
                listModel[listModel.Count - 1].gameObject.SetActive(!listModel[listModel.Count - 1].activeInHierarchy);
                listModelAnim[listModel.Count - 1].gameObject.SetActive(!listModelAnim[listModel.Count - 1].activeInHierarchy);
                controllerPracticeAKGun.PracticeAnimator.ChangeModel();
            }
            else
            {
                listModel[numberModel].SetActive(!listModel[numberModel].activeInHierarchy);
                listModelAnim[numberModel].SetActive(!listModelAnim[numberModel].activeInHierarchy);
                controllerPracticeAKGun.PracticeAnimator.ChangeModel();
            }
        }

    }
    //chạy khi nhấn nút reset trên editor không liên quan đến game chỉ hỗ trợ kéo thả
    #region editor
    protected override void LoadComponent()
    {
        base.LoadComponent();
        controllerPracticeAKGun = transform.parent.GetComponent<ControllerPracticeAKGun>();
    }
    #endregion
}
