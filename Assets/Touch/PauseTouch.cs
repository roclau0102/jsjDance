using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseTouch : MonoBehaviour {

    Button changeSongBtn;
    Button restartBtn;
    Button continueBtn;
    Button quitBtn;

	// Use this for initialization
	void Start () {

        changeSongBtn = transform.Find("Button_ChangeSong").GetComponent<Button>();
        restartBtn = transform.Find("Button_Restart").GetComponent<Button>();
        continueBtn = transform.Find("Button_Continue").GetComponent<Button>();
        quitBtn = transform.Find("Button_Quit").GetComponent<Button>();

        changeSongBtn.onClick.AddListener(()=> {
            BeatGame.instance.pauseUI.OnButtonChangeSong();
        });

        restartBtn.onClick.AddListener(()=> {
            BeatGame.instance.pauseUI.OnButtonRestart();
        });

        continueBtn.onClick.AddListener(()=> {
            BeatGame.instance.pauseUI.OnButtonContinue();
        });

        quitBtn.onClick.AddListener(()=> {
            BeatGame.instance.pauseUI.OnButtonQuit();
        });
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
