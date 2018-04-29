using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WXPanelBasic : MonoBehaviour {
    protected int focusIndex = 0;
    protected int maxIndex = 4;
    public Action<bool> loadingCall;
    public Action<float, bool> showKeyboardCall;

	// Use this for initialization
	void Start () {
	
	}


    virtual public void KeyboardInput(string value)
    {

    }

    virtual public void Init(DANCE_DATA data)
    {

    }

    void OnEnable()
    {
        Show();
    }

    virtual protected void InShow()
    {
        Move(0,0);
    }

	
	// Update is called once per frame
	void Update () {
	
	}

    virtual public void Show()
    {
        focusIndex = 0;
        Invoke("InShow", 0.1f);
    }

    virtual public void Hide()
    {

    }

    virtual public void Move(int x, int y)
    {
        focusIndex += y;
        if (focusIndex >= maxIndex)
        {
            focusIndex = 0;
        }
        else if (focusIndex < 0)
        {
            focusIndex = maxIndex - 1;
        }
    }

    public virtual void Enter()
    {

    }

    public virtual void Cancel()
    {

    }
}
