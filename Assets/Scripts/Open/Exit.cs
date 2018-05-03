using UnityEngine;
using System.Collections;
using System;

public class Exit : MonoBehaviour {
    public GameObject ok;
    public GameObject cancel;
    public int index=0;
    public Action callback;
       

	// Use this for initialization
	void Start () {
        CheckBtn();
	}

    public void OnDisable()
    {
        //旭日东升跳舞毯的左键为330为按钮冲突了~
        //if (Version.currentPlatform.ToString().IndexOf("XRDS") != -1)
        //{
        //    //JoystickManager.instance.GetInfaredRay().checkEventEnter = false;
        //}
    }

    public void OnEnable()
    {
        //if (Version.currentPlatform.ToString().IndexOf("XRDS") != -1)
        //{
        //    //JoystickManager.instance.GetInfaredRay().checkEventEnter = true;
        //}
    }

    

    public void Show(Action callback)
    {
        this.callback = callback;
        gameObject.SetActive(true);
        index =1;
        CheckBtn();
    }

    void CheckBtn()
    {
        ok.SetActive(index==1);
        cancel.SetActive(index==0);
    }

    public void Move(int x)
    {
        index += x;
        if (index > 1)
        {
            index = 0;
        }
        else if (index < 0)
        {
            index = 1;
        }
        CheckBtn();
    }

    public void Press()
    {
        if (index == 1)
        {
            if (callback!=null) callback();
        }
        gameObject.SetActive(false);
    }

	
	// Update is called once per frame
	void Update () {
	
	}
}
