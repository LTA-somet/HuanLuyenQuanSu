using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationState
{
    private static bool _isQuitting = false;
    public static bool IsQuitting => _isQuitting;
    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        Application.quitting += Quit;
    }
    static void Quit()
    {
        _isQuitting = true;
    }
}