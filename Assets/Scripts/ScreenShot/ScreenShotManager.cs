using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenShotManager : MonoBehaviour
{
    public string gameName = "Stolen Future";


    public RawImage showImg;
    public byte[] currentTexture;
    public string currentFilePath;

    public GameObject screenShotPanel;
    public GameObject capturePanel;
    public GameObject saveImagePanel;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string ScreenShotName()
    {
        return $"{gameName}_{System.DateTime.Now:yy-MMM-dd ddd}.png";
    }

    public void Capture()
    {
        StartCoroutine(TakeScreenShot());
    }
    
    private IEnumerator TakeScreenShot()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
        screenshot.Apply();

        currentFilePath = Path.Combine(Application.temporaryCachePath, "Temp_img.jpg");
        currentTexture = screenshot.EncodeToJPG();
        File.WriteAllBytes(currentFilePath,currentTexture);
        
        ShowImage();
        EnableUICapture(true);
        //Destroy to avoid memory leak
        Destroy(screenshot);
        
    }

    public void EnableUICapture(bool isactive)
    {
        capturePanel.SetActive(isactive);
    }

    public void ShowImage()
    {
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.LoadImage(currentTexture);
        showImg.material.mainTexture = tex;
        screenShotPanel.SetActive(true);
    }

    public void SharedImg()
    {
        new NativeShare().AddFile(currentFilePath).SetSubject("Subject goes here").SetText("Hello World").SetUrl("https://github.com/yasirkula/UnityNativeShare").SetCallback((result, shareTarget) => Debug.Log($"Shared result: {result}, selected app {shareTarget}")).Share();

    }

    public void SaveToGallery()
    {
        NativeGallery.Permission permission =
            NativeGallery.SaveImageToGallery(currentFilePath, gameName, ScreenShotName(), (success, path) =>
                {
                    Debug.Log("Media save result: " + success + " " + path);
                    if (success)
                    {
                        saveImagePanel.SetActive(true);
#if UNITY_EDITOR
                        string editorFilePath = Path.Combine(Application.persistentDataPath, ScreenShotName());
                        File.WriteAllBytes(editorFilePath, currentTexture);
#endif
                    }
                }
            );
        Debug.Log("Permission result" + permission);
    }
}
