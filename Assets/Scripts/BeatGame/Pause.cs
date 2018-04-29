using UnityEngine;
using System.Collections;
using System;

public class Pause : MonoBehaviour {
    public System.Action contiuneCall;
    public tk2dSprite[] btns;
    int index = 0;


    public void Show()
    {
       gameObject.SetActive(true);
        index = 2;  
        CheckBtn();
    }

    void CheckBtn()
    {
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].transform.GetChild(0).gameObject.SetActive(index == i);
        }            
    }

    public void Move(int x)
    {
        index += x;
        if (index >= btns.Length)
        {
            index = 0;
        }
        else if (index < 0)
        {
            index = btns.Length-1;
        }
        CheckBtn();
    }

    public Action<string, bool,bool> loadLevel;

    public void Press()
    {
        switch(index){
            case 0:
                loadLevel("SongList",false,true);
                break;
            case 1:
                loadLevel("Video", false, true);
                break;           
            case 2:
                contiuneCall();
                break;
            case 3:
                JoystickManager.instance.ExitGame();
                break;
        }
        gameObject.SetActive(false);
    }
}
