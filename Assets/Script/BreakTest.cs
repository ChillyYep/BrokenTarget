using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
