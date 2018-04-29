using UnityEngine;
using System.Collections;

public class AutoPlayVideo : MonoBehaviour {    
    public string videoName="idle";

    // Use this for initialization
    void Start()
    {
        Sounder.instance.StopBg();
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log("播放待机动画, 成功并返回");
            Application.LoadLevel("Title");
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            Global.InitVidoes(() =>
            {
                Debug.Log("初始化完成");

                if (Application.platform == RuntimePlatform.Android)
                {
                    Debug.Log("初始化完成,播放视频StaticPlayVideo:" + Global.AppContentPath() + videoName);
                    Global.CallAndroidStatic("StaticPlayVideo", Global.AppContentPath() + videoName);
                }
                time = Time.time;
                JoystickManager.instance.KeyEvent += instance_KeyEvent;
            }, videoName);
        }
    }

    void instance_KeyEvent(JoystickManager.PLAYER_INDEX player, JoystickManager.JOYSTICK_KEY key, JoystickManager.JOYSTICK_KEY_STATE state, JoystickManager.JOYSTICK_TYPE joysickType)
    {
        if (state == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN)
        {
            if (key == JoystickManager.JOYSTICK_KEY.KEY_BACK)
            {
                Load();
            }
        }
    }


    float time = -1;
    bool done = false;

    void Update()
    {
        if (done) return;
        if ((time != -1 && (Time.time - time) > 14.8f) || Input.GetKey(KeyCode.Escape))
        {            
            Invoke("Load", 0.5f);
            done = true;
        }
    }

    void Load()
    {
        JoystickManager.instance.KeyEvent -= instance_KeyEvent;
        Global.CallAndroidStatic("StaticHideVideo");
        Sounder.instance.Play("背景音乐", true);
        Application.LoadLevel("Title");
    }

}
