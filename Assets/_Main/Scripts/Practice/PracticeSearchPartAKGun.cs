using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeSearchPartAKGun : CustomMonoBehaviour
{
    [SerializeField]
    Camera mainCamera;
    [SerializeField]
    LayerMask layerMask;
    [SerializeField]
    private ControllerPracticeAKGun controllerPracticeAKGun;


    private string namePartAKGun;
    public string NamePartAKGun { get { return namePartAKGun; } }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        FindGameObject();
    }
    private void FixedUpdate()
    {
        //FindGameObject();
    }

    private void FindGameObject()
    {

        if (controllerPracticeAKGun.isRun)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Vector3 mousePos = Input.mousePosition;
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    Debug.Log(hit.transform.name);
                    //controllerPracticeAKGun.Animator.Play(hit.transform.name);
                    if (controllerPracticeAKGun.PracticeAnimator.IsExitState) return;
                    namePartAKGun = hit.transform.name;
                    controllerPracticeAKGun.isRunAnim = true;
                    controllerPracticeAKGun.CheckNameRunState(namePartAKGun);

                }
                //namePartAKGun = null;
            }
        }
        else
        {
            controllerPracticeAKGun.isRunAnim = false;
        }

    }
    #region editor
    protected override void LoadComponent()
    {
        base.LoadComponent();
        mainCamera = Camera.main;
        layerMask = LayerMask.GetMask("AK Gun");
        controllerPracticeAKGun = transform.parent.GetComponent<ControllerPracticeAKGun>();
    }
    #endregion
}
