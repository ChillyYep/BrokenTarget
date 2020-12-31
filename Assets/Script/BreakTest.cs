using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BrokenSys;

public class BreakTest : MonoBehaviour
{
    public void StartBrokenManager()
    {
        BrokenManager.Instance.StartOver();
    }
    public void StopBrokenManager()
    {
        BrokenManager.Instance.Stop();
    }
}
