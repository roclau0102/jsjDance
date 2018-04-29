using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public  class BaseScene : MonoBehaviour
{
    string nextLevelName;
    protected bool initedCamera = false;

    public BasePropItem okBtn;
    public GameObject returnBtn;

    public bool canPressUp = true;
    public bool canPressDown = true;
    public bool canPressLeft = true;
    public bool canPressRight = true;
    public bool canPressEnter = true;
    public bool canPressCancel = true;


    protected virtual  void LateUpdate()
    {
        if (!initedCamera) CameraFadeIn();
    }

    protected virtual void CameraFadeIn(bool isBlackBg=true)
    {
        if (Camera.main == null) return;
        Camera.main.GetComponent<CameraFade>().In(isBlackBg);
        initedCamera = true;
    }


    protected virtual void LoadLevel(string name, bool goFront=true, bool needFadeOut=true)
    {
        if (goFront)
        {
            if (okBtn != null)
            {
                okBtn.GetComponent<Animation>().Play();
                okBtn = null;
            }
        }
        else
        {
            if (returnBtn != null)
            {
                returnBtn.GetComponent<Animation>().Play();
                returnBtn = null;
            }
        }
        RemoveKeyEvent();
        Resources.UnloadUnusedAssets();
        
        if (needFadeOut)
        {
            Camera.main.GetComponent<CameraFade>().Out();
            nextLevelName = name;
            Invoke("LoadLevel1", 0.5f);
        }
        else
        {
            Application.LoadLevel(name);
        }        
    }

    void LoadLevel1()
    {
        Application.LoadLevel(nextLevelName);
    }

    protected void RemoveKeyEvent()
    {
        JoystickManager.instance.KeyEvent -= instance_KeyEvent;
        JoystickManager.instance.BlanketEvent -= instance_BlanketEvent;
    }


    protected virtual void OnEnable()
    {
        JoystickManager.instance.isUseBlanket = true;
        JoystickManager.instance.isTestKeyboard4Blanket = true;
        JoystickManager.instance.KeyEvent += instance_KeyEvent;
        JoystickManager.instance.BlanketEvent += instance_BlanketEvent;
    }

    protected abstract void Move(int x, int y, INPUT_TYPE type,JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player);
    protected abstract void PressEnter(INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player);
    protected abstract void Cancel(INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player);

    public enum INPUT_TYPE
    {
        JOYSTICK,
        BLANKET
    }

    float enterPressTime = 0;
    static protected float pressTime = 0;

    void instance_KeyEvent(JoystickManager.PLAYER_INDEX player, JoystickManager.JOYSTICK_KEY key, JoystickManager.JOYSTICK_KEY_STATE state, JoystickManager.JOYSTICK_TYPE joysickType)
    {
        if (DataUtils.runingAutoMode)
        {
            Global.CancelAutoMode();
            return;
        }
        pressTime = Time.time;
        switch (key)
        {            
            case JoystickManager.JOYSTICK_KEY.KEY_UP:
                if (Version.currentPlatform.ToString().IndexOf("WX_XRDS")!=-1) return;
                if(canPressUp)Move(0, -1, INPUT_TYPE.JOYSTICK, state, player);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_DOWN:
                if (Version.currentPlatform.ToString().IndexOf("WX_XRDS") != -1) return;
                if (canPressDown) Move(0, 1, INPUT_TYPE.JOYSTICK, state, player);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_LEFT:
                if (Version.currentPlatform.ToString().IndexOf("WX_XRDS") != -1) return;
                if (canPressLeft) Move(-1, 0, INPUT_TYPE.JOYSTICK, state, player);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_RIGHT:
                if (Version.currentPlatform.ToString().IndexOf("WX_XRDS") != -1) return;
                if (canPressRight) Move(1, 0, INPUT_TYPE.JOYSTICK, state, player);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_OK:
                if (state == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN && (Time.time - enterPressTime) > 0.3f)
                {
                    if (canPressEnter) PressEnter(INPUT_TYPE.JOYSTICK, state, player);
                    //enterPressTime = Time.time;
                }
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_BACK:
                if (canPressCancel)
                {
                    if (state == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) Cancel(INPUT_TYPE.JOYSTICK, state, player);
                }                
                break;
        }

        if (Version.currentPlatform == Version.PLAFTFORM_ENUM.OS_SHOW || Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_SHOW)
        {
            if (state == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN &&
            key == JoystickManager.JOYSTICK_KEY.KEY_Y)
            {
                /*
                if (JoystickManager.instance.GetKey(JoystickManager.PLAYER_INDEX.P1, JoystickManager.JOYSTICK_KEY.KEY_A, true) &&
                    JoystickManager.instance.GetKey(JoystickManager.PLAYER_INDEX.P1, JoystickManager.JOYSTICK_KEY.KEY_B, true) &&
                    JoystickManager.instance.GetKey(JoystickManager.PLAYER_INDEX.P1, JoystickManager.JOYSTICK_KEY.KEY_X, true))
                {
                    DataUtils.AddMoney(1000);
                    Global.CallAndroidStatic("StaticAlert", "加金钱1000");
                }*/
            }
        }        
    }

    public static bool blanketPress = false;    

    void instance_BlanketEvent(JoystickManager.BLANKET_NUMBER_KEY key, JoystickManager.JOYSTICK_KEY_STATE state)
    {
        if (DataUtils.runingAutoMode)
        {
            Global.CancelAutoMode();
            return;
        }
        blanketPress = true;
        pressTime = Time.time;

        switch (key)
        {
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_6:
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_21:
                if (canPressUp)
                {
                    Move(0, -1, INPUT_TYPE.BLANKET, state,
                    key == JoystickManager.BLANKET_NUMBER_KEY.KEY_6 ?
                    JoystickManager.PLAYER_INDEX.P1 : JoystickManager.PLAYER_INDEX.P2);
                }                
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_10:
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_25:
                if (canPressDown)
                {
                    Move(0, 1, INPUT_TYPE.BLANKET, state,
                    key == JoystickManager.BLANKET_NUMBER_KEY.KEY_10 ?
                    JoystickManager.PLAYER_INDEX.P1 : JoystickManager.PLAYER_INDEX.P2);
                }                
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_3:
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_18:
                if (canPressLeft)
                {
                    Move(-1, 0, INPUT_TYPE.BLANKET, state,
                    key == JoystickManager.BLANKET_NUMBER_KEY.KEY_3 ?
                    JoystickManager.PLAYER_INDEX.P1 : JoystickManager.PLAYER_INDEX.P2);
                }                
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_13:
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_28:
                if (canPressRight)
                {
                    Move(1, 0, INPUT_TYPE.BLANKET, state,
                    key == JoystickManager.BLANKET_NUMBER_KEY.KEY_13 ?
                    JoystickManager.PLAYER_INDEX.P1 : JoystickManager.PLAYER_INDEX.P2);
                }                
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_11:
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_26:
                if (canPressEnter)
                {
                    if (state == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN)
                        PressEnter(INPUT_TYPE.BLANKET, state,
                            key == JoystickManager.BLANKET_NUMBER_KEY.KEY_11 ?
                            JoystickManager.PLAYER_INDEX.P1 : JoystickManager.PLAYER_INDEX.P2);
                }                
                break;                
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_1:
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_16:
                if (Application.platform != RuntimePlatform.Android || Version.currentPlatform.ToString().IndexOf("WX_XRDS") != -1)
                {
                    if (canPressCancel)
                    {
                        if (state == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN)
                            Cancel(INPUT_TYPE.BLANKET, state,
                                key == JoystickManager.BLANKET_NUMBER_KEY.KEY_1 ?
                                JoystickManager.PLAYER_INDEX.P1 : JoystickManager.PLAYER_INDEX.P2);
                    }                    
                }                
                break;
        }
        
        if (Application.platform != RuntimePlatform.Android)
        {
            if (state == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN &&
                key == JoystickManager.BLANKET_NUMBER_KEY.KEY_15)
            {
                DataUtils.AddMoney(1000);
            }
        }
    }
}
