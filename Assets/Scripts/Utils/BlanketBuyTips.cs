using UnityEngine;
using System.Collections;

public class BlanketBuyTips : BaseScene {

    public Texture2D wxTexture;
    public Texture2D osTexture;
    public System.Action successCallback;
    public System.Action failedCallback;
    public bool forceClose = false;

    static TIPS_CHECK_STEP step = TIPS_CHECK_STEP.UNCHECK;
    enum TIPS_CHECK_STEP
    {
        UNCHECK,
        CHECKING,
        DONE
    }

    tk2dSpriteFromTexture sprite;

    protected override void LateUpdate()
    {
        if (forceClose)
        {
            if (successCallback != null) successCallback();
            RemoveKeyEvent();
            Destroy(gameObject);
        }
    }

	// Use this for initialization

    bool startCheck = false;

    internal void StartCheck()
    {
        startCheck = true;        

        //if (Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_SHOW_NO_REGIST ||
        //    Version.currentPlatform== Version.PLAFTFORM_ENUM.TEL ||
        //    Version.currentPlatform == Version.PLAFTFORM_ENUM.OS_SHOW ||
        //    Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_SHOW ||
        //    Version.IsOS() || Version.currentPlatform == Version.PLAFTFORM_ENUM.USB
        //    )
        //{
        //    if (successCallback!=null) successCallback();
        //    RemoveKeyEvent();
        //    return;
        //}

        if (step == TIPS_CHECK_STEP.DONE)
        {
            step = TIPS_CHECK_STEP.DONE;
            if (successCallback != null) successCallback();
            RemoveKeyEvent();
            Destroy(gameObject);
        }
        else
        {
            gameObject.transform.position = new Vector3(9.6f, 5.4f, -14.5f);
            sprite = GetComponent<tk2dSpriteFromTexture>();
            if (sprite == null)
            {
                sprite = gameObject.AddComponent<tk2dSpriteFromTexture>();
            }
            Check(INPUT_TYPE.JOYSTICK, false, false);
        }        
    }

    void OnDestroy()
    {
        wxTexture = osTexture = null;
        if (sprite!=null) sprite.texture = null;
    }


    void Check(BaseScene.INPUT_TYPE type, bool enter, bool cancel)
    {
        Debug.Log("1");
        if (!startCheck) return;
        Debug.Log("2");
        switch (step)
        {
            case TIPS_CHECK_STEP.UNCHECK:
                Debug.Log("3");
                if (BaseScene.blanketPress)
                {
                    Debug.Log("4");
                    step = TIPS_CHECK_STEP.DONE;                    
                }
                else
                {
                    Debug.Log("5");
                    //if (Version.IsOS())
                    //{
                    //    sprite.texture = osTexture;
                    //}
                    //else if (Version.IsWX())
                    //{
                    //    sprite.texture = wxTexture;
                    //}
                    step = TIPS_CHECK_STEP.CHECKING;
                    sprite.ForceBuild();
                    sprite.gameObject.SetActive(true);
                }
                break;
            case TIPS_CHECK_STEP.CHECKING:
                Debug.Log("6  " + type);
                if (type == INPUT_TYPE.BLANKET)
                {
                    Debug.Log("7");
                    step = TIPS_CHECK_STEP.DONE;
                }
                else
                {
                    Debug.Log("8");
                    if (enter)
                    {
                        Debug.Log("9");
                        Application.OpenURL("http://detail.tmall.com/item.htm?id=15097926519");
                    }
                    else if (cancel)
                    {
                        Debug.Log("10");
                        if (failedCallback != null) failedCallback();
                        RemoveKeyEvent();
                    }
                }
                break;
        }

        if (step == TIPS_CHECK_STEP.DONE)
        {
            Debug.Log("11");
            RemoveKeyEvent();
            if (successCallback != null) successCallback();
            Destroy(gameObject);
        }        
    }


    public override void Move(int x, int y, BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        Debug.Log("press " + x + "," + y);
        if (keyState == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN)
        {
            Check(type, false, false);
        }
        return;
    }

    public override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        Debug.Log("press Enter");
        if (keyState == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN)
        {
            Check(type, true, false);
        }        
    }

    public override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        Debug.Log("press cancel");
        if (keyState == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN && type == INPUT_TYPE.JOYSTICK)
        {
            Check(type, false, true);
        }        
    }

    
}
