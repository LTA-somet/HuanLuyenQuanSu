using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeAnimator : CustomMonoBehaviour
{
    [SerializeField]
    private Animator animator;
    public Animator Animator { get { return animator; } }
    [SerializeField]
    private ControllerPracticeAKGun controllerPracticeAKGun;
    public ControllerPracticeAKGun ControllerPracticeAKGun { get { return controllerPracticeAKGun; } }
    private bool isExitState = false;
    public bool IsExitState { get { return isExitState; } }

    private string currentState;
    private bool isChangeModel = false;
    public bool IsChangeModel { get { return isChangeModel; } }

    //[Header("test")]
    //public int layer = 1;
    //public float time = 0;

    //chuyển sang animation tiếp theo
    public void ChangeCurrentState(string newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        animator.Play(currentState);
    }

    private void Update()
    {

        //if (Input.GetKey(KeyCode.Space))
        //{
        //    time += Time.deltaTime;
        //time = Mathf.Clamp01(time += Time.deltaTime);
        //animator.Play("Take 009", layer, time);
        //}
    }

    //reset trạng thái mặc định khi thoát và bắt đầu màn hình thực hành tháo lắp
    public void Init()
    {
        isExitState = false; isChangeModel = false;
        currentState = "";
    }
    //chạy khi nhấn nút reset trên editor không liên quan đến game chỉ hỗ trợ kéo thả
    #region editor
    protected override void LoadComponent()
    {
        base.LoadComponent();
        animator = GetComponent<Animator>();
        controllerPracticeAKGun = transform.parent.GetComponent<ControllerPracticeAKGun>();
    }
    #endregion
    //chay cả trên code runtime và trên even của animation đề check next animation và check thay đổi model
    #region Even Animation
    public void CheckState()
    {
        if (!controllerPracticeAKGun.UIThucHanh.IsThao) return;
        isExitState = !isExitState;
    }
    public void CheckStateLap()
    {
        if (controllerPracticeAKGun.UIThucHanh.IsThao) return;
        isExitState = !isExitState;
    }
    public void ChangeModel()
    {
        isChangeModel = !isChangeModel;
    }
    #endregion
}
