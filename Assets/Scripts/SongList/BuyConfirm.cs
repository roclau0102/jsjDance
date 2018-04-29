using UnityEngine;
using System.Collections;
using System;

public class BuyConfirm : MonoBehaviour {
    public tk2dButton ok;
    public tk2dButton cancel;
    public int index=0;
    public Action callback;
    public TextMesh text;
    

	// Use this for initialization
	void Start () {
	    
	}

    public void Show(Action callback, string text)
    {
        this.callback = callback;
        gameObject.SetActive(true);
        this.text.text = text;
        index = 1;
        CheckBtn();        
        Sounder.instance.Play("对话框弹出音效");        
    }

    void CheckBtn()
    {
        ok.transform.GetChild(0).gameObject.SetActive(index==1);
        cancel.transform.GetChild(0).gameObject.SetActive(index==0);
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
        Sounder.instance.Play("按键音效");
    }

	
	// Update is called once per frame
	void Update () {
	
	}
}
