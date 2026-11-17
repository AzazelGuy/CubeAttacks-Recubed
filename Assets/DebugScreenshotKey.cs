using System;
using UnityEngine;

public class DebugScreenshotKey : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
            Debug.Log("A screenshot was taken!");
        }
    }
}
