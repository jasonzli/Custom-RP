using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        ScreenCapture.CaptureScreenshot(filename + endingStrings[ending], scale);
    }
}
