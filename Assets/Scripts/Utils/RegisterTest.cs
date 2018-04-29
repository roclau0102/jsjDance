using UnityEngine;
using System.Collections;

public class RegisterTest : MonoBehaviour {

    string result = null;



    bool getResultComplete = false;

    public Texture2D splashImage;
    void Start() {
        if (Application.platform != RuntimePlatform.Android)
            return;
        #if UNITY_ANDROID
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.Call("StartActivity");
        #endif
    }

    void OnGUI() {
        if (splashImage!=null)GUI.DrawTexture(new Rect(0.5f * (Screen.width - splashImage.width), (Screen.height - splashImage.height) * 0.5f, splashImage.width, splashImage.height), splashImage);

        if (Input.GetKey(KeyCode.Escape))
        {
            if (Application.platform == RuntimePlatform.Android){              
                Application.Quit();
            }
        }

        if (getResultComplete) return;
        switch (result)
        {
            case "1":
                getResultComplete = true;
                Debug.Log("注册通过");
                Application.LoadLevel("Open");
                break;
            case "0":
                Application.Quit();
                Debug.Log("注册不通过");
                getResultComplete = true;
                break;
        }      
    }

    void messgae(string str) {
        result = str;        
    }
}

