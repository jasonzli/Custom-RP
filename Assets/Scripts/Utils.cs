using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;


public class Utils : MonoBehaviour
{
    public enum FileTypes
    {
        JPG, PNG, GIF
    }
    Dictionary<FileTypes, string> endingStrings = new Dictionary<FileTypes, string>()
    {
        { FileTypes.JPG, ".JPG"},
        { FileTypes.PNG, ".png"},
        { FileTypes.GIF, ".gif"}
    };
    public string filename;
    [Range(1, 5)]
    public int scale = 1;
    public FileTypes ending;
    public void Snapshot()
    {
        if (!Directory.Exists("Screenshots/"))
        {
            Directory.CreateDirectory("Screenshots/");
        }
        ScreenCapture.CaptureScreenshot($"Screenshots/{filename}{endingStrings[ending]}", scale);
        Debug.Log("Screenshot taken");
    }
}
