using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//Somekind of YieldInstruction pooling  
public class Yielder : Singleton<Yielder>
{
    Dictionary<float, WaitForSeconds> _yieldSecondsPool = new Dictionary<float, WaitForSeconds>();
    WaitForFixedUpdate _yieldFixedFrame = new WaitForFixedUpdate();
    WaitForEndOfFrame _yieldEndOfFrame = new WaitForEndOfFrame();

    public static WaitForFixedUpdate WaitForFixedUpdate => Instance ? Instance._yieldFixedFrame : new WaitForFixedUpdate();
    public static WaitForEndOfFrame WaitForEndOfFrame => Instance ? Instance._yieldEndOfFrame : new WaitForEndOfFrame();
    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!Instance) return new WaitForSeconds(seconds);
        var pool = Instance._yieldSecondsPool;
        if (!pool.ContainsKey(seconds))
        {
            pool.Add(seconds, new WaitForSeconds(seconds));
        }
        return pool[seconds];
    }
}