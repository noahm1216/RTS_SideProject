using UnityEngine;
using System.IO; // Required for path operations
using System.Collections;

public class ScreenshotHandler : MonoBehaviour
{
    public KeyCode screenshotKey = KeyCode.P; // Key to trigger screenshot
    public string folderName = "Screenshots"; // Name of the folder to save screenshots

    void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }

       
    }

    void TakeScreenshot()
    {
        // Construct the full path to the desired folder within the project
        string folderPath = Path.Combine(Application.dataPath, folderName);

        // Create the directory if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate a unique filename using a timestamp
        string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string fullPath = Path.Combine(folderPath, fileName);

        // Capture the screenshot and save it to the specified path
        ScreenCapture.CaptureScreenshot(fullPath);

        Debug.Log("Screenshot saved to: " + fullPath);
    }

   
}
