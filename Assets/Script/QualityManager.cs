using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityManager : MonoBehaviour
{
    public int targetFrameRate = 60;
    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
