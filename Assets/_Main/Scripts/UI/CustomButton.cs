using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomButton : CustomMonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textTopic;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void MouseEnter()
    {
        _textTopic.fontStyle |= TMPro.FontStyles.Underline;
    }
    public void MouseExit()
    {
        _textTopic.fontStyle = _textTopic.fontStyle & ~TMPro.FontStyles.Underline;
    }
    protected override void LoadComponent()
    {
        base.LoadComponent();
        _textTopic = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }
}
