using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KeyboardSetting  {

    /// <summary>
    /// p1挥动
    /// </summary>
    public enum P1_ACTION_TO_KEYBOARD
    {
        WAVE = KeyCode.Space,
        WAVE_LEFT = KeyCode.N,
        WAVE_RIGHT = KeyCode.M
    };

    /// <summary>
    /// p2挥动
    /// </summary>
    public enum P2_ACTION_TO_KEYBOARD
    {
        WAVE = KeyCode.Keypad0,
        WAVE_LEFT = KeyCode.Keypad7,
        WAVE_RIGHT = KeyCode.Keypad8
    };

    /// <summary>
    /// P1模拟手柄对应回手键盘
    /// </summary>
    static public Dictionary<JoystickManager.JOYSTICK_KEY, KeyCode> P1_JOYSTICK_KEY_TO_KEYBOARD = new Dictionary<JoystickManager.JOYSTICK_KEY, KeyCode>
    {
        {JoystickManager.JOYSTICK_KEY.KEY_UP, KeyCode.W},
        {JoystickManager.JOYSTICK_KEY.KEY_DOWN, KeyCode.S},
        {JoystickManager.JOYSTICK_KEY.KEY_LEFT, KeyCode.A},
        {JoystickManager.JOYSTICK_KEY.KEY_RIGHT, KeyCode.D},        
        {JoystickManager.JOYSTICK_KEY.KEY_OK, KeyCode.U},
        {JoystickManager.JOYSTICK_KEY.KEY_D, KeyCode.I},
        {JoystickManager.JOYSTICK_KEY.KEY_A, KeyCode.O},
        {JoystickManager.JOYSTICK_KEY.KEY_B, KeyCode.J},
        {JoystickManager.JOYSTICK_KEY.KEY_X, KeyCode.K},
        {JoystickManager.JOYSTICK_KEY.KEY_Y, KeyCode.L},
        {JoystickManager.JOYSTICK_KEY.KEY_BACK, KeyCode.Escape}
    };

    /// <summary>
    /// P2模拟手柄对应回手键盘
    /// </summary>
    static public Dictionary<JoystickManager.JOYSTICK_KEY, KeyCode> P2_JOYSTICK_KEY_TO_KEYBOARD = new Dictionary<JoystickManager.JOYSTICK_KEY, KeyCode>
    {
        {JoystickManager.JOYSTICK_KEY.KEY_UP, KeyCode.UpArrow},
        {JoystickManager.JOYSTICK_KEY.KEY_DOWN, KeyCode.DownArrow},
        {JoystickManager.JOYSTICK_KEY.KEY_LEFT, KeyCode.LeftArrow},
        {JoystickManager.JOYSTICK_KEY.KEY_RIGHT, KeyCode.RightArrow},        
        {JoystickManager.JOYSTICK_KEY.KEY_C, KeyCode.Keypad1},
        {JoystickManager.JOYSTICK_KEY.KEY_D, KeyCode.Keypad2},
        {JoystickManager.JOYSTICK_KEY.KEY_A, KeyCode.Keypad3},
        {JoystickManager.JOYSTICK_KEY.KEY_B, KeyCode.Keypad4},
        {JoystickManager.JOYSTICK_KEY.KEY_X, KeyCode.Keypad5},
        {JoystickManager.JOYSTICK_KEY.KEY_Y, KeyCode.Keypad6},
        {JoystickManager.JOYSTICK_KEY.KEY_BACK, KeyCode.Keypad9}
    };

    /// <summary>
    /// 跳舞毯模拟回键盘
    /// </summary>
    static public Dictionary<JoystickManager.BLANKET_NUMBER_KEY, KeyCode> BLANKET_TO_KEYBOARD = new Dictionary<JoystickManager.BLANKET_NUMBER_KEY, KeyCode>
    {
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_1, KeyCode.Q},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_3, KeyCode.A},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_5, KeyCode.Z},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_6, KeyCode.W},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_80, KeyCode.X},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_81, KeyCode.X},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_10, KeyCode.S},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_11, KeyCode.E},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_13, KeyCode.D},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_15, KeyCode.C},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_16, KeyCode.Keypad7},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_18, KeyCode.Keypad4},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_20, KeyCode.Keypad1},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_21, KeyCode.Keypad8},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_230, KeyCode.Keypad5},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_231, KeyCode.Keypad5},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_25, KeyCode.Keypad2},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_26, KeyCode.Keypad9},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_28, KeyCode.Keypad6},
        {JoystickManager.BLANKET_NUMBER_KEY.KEY_30, KeyCode.Keypad3}
    };


    

}
