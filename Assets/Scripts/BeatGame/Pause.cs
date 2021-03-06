using UnityEngine;
using System.Collections;
using System;

public class Pause : MonoBehaviour {
    public System.Action contiuneCall;
    public tk2dSprite[] btns;
    int index = 0;

    public GameObject pauseTouch;


    private void OnEnable()
    {
        pauseTouch.SetActive(true);
    }

    private void OnDisable()
    {
        pauseTouch.SetActive(false);
    }

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
                OnButtonChangeSong();
                break;
            case 1:
                OnButtonRestart();
                break;
            case 2:
                OnButtonContinue();
                break;
            case 3:
                OnButtonQuit();
                break;
        }
        gameObject.SetActive(false);
    }

    public void OnButtonQuit()
    {
        JoystickManager.instance.ExitGame();
        gameObject.SetActive(false);
    }

    public void OnButtonContinue()
    {
        contiuneCall();
        gameObject.SetActive(false);
    }

    public void OnButtonRestart()
    {
        loadLevel("Video", false, true);
        gameObject.SetActive(false);
    }

    public void OnButtonChangeSong()
    {
        loadLevel("SongList", false, true);
        gameObject.SetActive(false);
    }
}
