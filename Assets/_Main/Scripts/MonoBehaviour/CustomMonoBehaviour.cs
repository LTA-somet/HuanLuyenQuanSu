using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomMonoBehaviour : MonoBehaviour
{
    private void Reset()
    {
        LoadComponent();
        ResetValue();
    }
    protected virtual void LoadComponent() { }
    protected virtual void ResetValue() { }
}
