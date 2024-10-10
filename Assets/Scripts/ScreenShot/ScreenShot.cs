using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScreenShot : MonoBehaviour
{
    // Start is  called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator TakeScreenShotAndShared()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
        screenshot.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "Shared_img.jpg");
        File.WriteAllBytes(filePath,screenshot.EncodeToJPG());
        
        //Destroy to avoid memory leak
        Destroy(screenshot);
        
        new NativeShare().AddFile(filePath).SetSubject("Subject goes here").SetText("Hello World").SetUrl("https://github.com/yasirkula/UnityNativeShare").SetCallback((result, shareTarget) => Debug.Log($"Shared result: {result}, selected app {shareTarget}")).Share();
    }
}
