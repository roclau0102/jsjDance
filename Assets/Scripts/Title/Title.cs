using UnityEngine;
using System.Collections;

public class Title : BaseScene, ICW_AttachScene
{

    public GameObject model;
    public GameObject options;
    public GameObject startText;
    public GameObject loading;
    public GameObject loadingBar;
    bool inited = false;
    public GameObject p1Eff;
    public GameObject p2Eff;
    public GameObject p1SelectEff;
    public GameObject p2SelectEff;
    public Exit exit;

    public int state = 4;


    public Texture2D textureWX;


    // Use this for initialization
    void Start()
    {
        startText.SetActive(false);
        loading.SetActive(true);
        if (Global.IsOverlayMode())
        {
            Global.CallAndroidStatic("StaticHideVideo");
        }

        Global.init(() =>
        {
            loadingBar.GetComponent<Animation>().Stop();
            loadingBar.transform.localScale = Vector3.one;
            Invoke("Init", 0.5f);

        });
        p2SelectEff.SetActive(false);


        WXLoginInfoPanel.instance.addGameMoneyCallback = (count) =>
        {
            DataUtils.AddMoney(count);
        };

        Texture2D texture = null;

        texture = textureWX;


        if (Version.currentPlatform.ToString().IndexOf("XRDS") != -1)
        {
            JoystickManager.instance.blanketType = JoystickManager.BLANKET_TYPE.东升旭日;
            JoystickManager.instance.joystickType = JoystickManager.JOYSTICK_TYPE.INFARED_RAY;
            JoystickManager.instance.GetInfaredRay().checkEventEnter = false;
            JoystickManager.instance.autoCheckJoyStickType = false;
        }
        else
        {
            if (CheckHand.instance == null)
            {
                CheckHand.Init(
                 CheckHand.TYPE.BLANKET, texture,
                Version.IsWX()
             );
            }
        }

        pressTime = Time.time;

    }

    bool canEnter = true;
    bool canCancle = true;
    static int loadedTime = 0;
    AndroidJavaObject activity;

    void Update()
    {
        if (Version.currentPlatform == Version.PLAFTFORM_ENUM.SkyWorth_Dis_NoReg)
        {
            if ((Time.time - pressTime) > 15f)
            {
                //if (Application.platform == RuntimePlatform.Android)
                //{
                //    activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                //   .GetStatic<AndroidJavaObject>("currentActivity");
                //    state = activity.Get<int>("usbhid_state");
                //}

                ///
                //if (state != 4)
                //{
                //CW.Instance.SetState(CW.STATE.自动演示中);
                //}


                ///在主菜单界面无操作15s后，自动演示.若未接入跳舞毯，演示30s后，返回主菜单；若已接入跳舞毯，则一直演示下去。

                if (CW.Instance != null)
                {
                    CW.Instance.autoPlayTimer.endable = true;
                    CW.Instance.autoPlayTimer.SaveTime();

                    CW.Instance.SetState(CW.STATE.自动演示中);
                    Debug.Log("自动演示");                    
                }

                pressTime = Time.time;
                DataUtils.isAutoMode = true;
                DataUtils.runingAutoMode = true;
                LoadLevel("SongList");

                Debug.Log("load songlisg");
            }
        }
    }


    protected override void CameraFadeIn(bool isBlackBg = true)
    {
        base.CameraFadeIn(loadedTime != 0);
        loadedTime++;
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        keyTime = Time.time;
    }


    void Init()
    {
        startText.SetActive(true);
        loading.SetActive(false);
        inited = true;

        if (Version.currentPlatform != Version.PLAFTFORM_ENUM.SkyWorth_Dis_NoReg)
        {
            if (Guide.instance != null)
            {
                Guide.instance.Show();
            }
        }
        else
        {
            if (Guide.instance != null) Guide.instance.Free();
        }


    }


    public void AddEvent()
    {
        OnEnable();
    }




    float keyTime = 0;
    int pIndex = 1;


    protected override void Move(int x, int y, BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (keyState != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;
        if ((Time.time - keyTime) < 0.3f) return;
        if (WXLoginInfoPanel.instance != null && WXLoginInfoPanel.instance.loginUI != null && WXLoginInfoPanel.instance.loginUI.activeSelf) return;
        if (!inited) return;
        if (canEnter == false) return;

        keyTime = Time.time;
        if (exit.gameObject.activeSelf)
        {
            if (x != 0) exit.Move(x);
            return;
        }
        else if (model.activeSelf)
        {
            Sounder.instance.Play("按键音效");
            pIndex++;
            if (pIndex > 2)
            {
                pIndex = 1;
            }
            else if (pIndex < 1)
            {
                pIndex = 2;
            }
            p1SelectEff.SetActive(pIndex == 1);
            p2SelectEff.SetActive(pIndex == 2);

            options.GetComponent<Animation>().Play("Select" + pIndex + "P");
            GetComponent<Animation>().Play("Select" + pIndex + "PCharacter");
        }
        else
        {
            if (!WXLoginInfoPanel.instance.loginUI.activeSelf)
            {
                if (y == -1)
                {
                    WXLoginInfoPanel.instance.ShowLogin();
                    Sounder.instance.Play("按键音效");
                }
            }
        }
    }

    protected override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (!inited) return;
        if ((Time.time - keyTime) < 0.3f) return;
        keyTime = Time.time;
        if (WXLoginInfoPanel.instance != null && WXLoginInfoPanel.instance.loginUI != null && WXLoginInfoPanel.instance.loginUI.activeSelf) return;


        if (exit.gameObject.activeSelf)
        {
            exit.Press();
            return;
        }
        else if (model.activeSelf == false)
        {
            if (canEnter)
            {
                Sounder.instance.Play("模式选中按键音");
                model.SetActive(true);
            }
            return;
        }

        RemoveKeyEvent();
        DataUtils.mode = (Global.MODE)(pIndex - 1);

        switch (DataUtils.mode)
        {
            case Global.MODE.MODE_1P:
                p1Eff.SetActive(true);
                break;
            case Global.MODE.MODE_2P:
                p2Eff.SetActive(true);
                break;
        }
        Sounder.instance.Play("模式选中按键音");
        Invoke("GoNext", 0.5f);
    }

    void GoNext()
    {
        LoadLevel("SongList");
    }

    protected override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (!canCancle) return;
        if (WXLoginInfoPanel.instance != null && WXLoginInfoPanel.instance.loginUI != null && WXLoginInfoPanel.instance.loginUI.activeSelf) return;
        if ((Time.time - keyTime) < 0.3f) return;
        keyTime = Time.time;
        if (model.activeSelf)
        {
            model.SetActive(false);
            Sounder.instance.Play("返回按键");
        }
        else
        {
            if (!exit.gameObject.activeSelf)
            {

                exit.Show(() =>
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        (new AndroidJavaObject("java.lang.System")).CallStatic("exit", 0);
                    }
                    else
                    {
                        Application.Quit();
                    }
                });
            }
            else
            {
                exit.gameObject.SetActive(false);
            }
        }
    }

    public void SetCanEnter(bool b)
    {
        canEnter = b;
    }

    public void SetCanCancle(bool b)
    {
        canCancle = b;
    }

    public bool CanShowEMW()
    {
        return exit && exit.gameObject.activeSelf == false;
    }
}
