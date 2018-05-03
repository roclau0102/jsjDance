using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class RegistScene : MonoBehaviour {
    public GameObject checking;
    public GameObject failed;

    AndroidJavaObject currentActivity;

	// Use this for initialization
	void Start () {

        if (Application.isEditor)
            //Application.LoadLevel("Open");
            SceneManager.LoadScene("Open");
        

        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = activity.GetStatic<AndroidJavaObject>("currentActivity");
            //if (Version.IsUSB())
            //{
            //    //JoystickManager.instance.CallJavaFunction("InitUsb");
            //}
        }
        waitTime = Time.time;
	}

    bool initUSBComplete = false;
    float waitTime = 0;

    /// <summary>
    /// USB授权完了就进入游戏咯
    /// </summary>
    /// <returns></returns>
    void CheckingPremissionDone()
    {
        if (Time.time - waitTime < 1)
        {
            return;
        }
        if (initUSBComplete) return;
        int state = currentActivity.Get<int>("request_state");
        int request_flag = currentActivity.Get<int>("request_flag");
        Debug.Log("state:" + state + "," + request_flag);
        if (state != 2)
        {
            return;
        }
        if (request_flag == 1)
        {
            initUSBComplete = true;
            Application.LoadLevel("AutoUpgrade");
            return;
        }
        else
        {
            return;
        }
    }


    bool done = false;

	// Update is called once per frame
	void Update () {
        if (Application.platform != RuntimePlatform.Android) return;

        //if (Version.IsUSB())
        //{
        //    CheckingPremissionDone();
        //    return;
        //}


        if (done)
        {            
            if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Menu))
            {
                Debug.Log("exitgame");
                Global.ExitGame();
            }
            return;
        }
        string state = currentActivity.Get<string>("registed");
        switch (state)
        {
            case "failed":
                Debug.Log("regist failed");
                checking.SetActive(false);
                failed.SetActive(true);
                done = true;
                break;
            case "success":
                Debug.Log("regist success");
                Application.LoadLevel("Open");
                done = true;
                break;
        }
	}
}
