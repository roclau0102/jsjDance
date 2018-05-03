/*
 *Author by roc. All rights reserved. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongListTouch : MonoBehaviour {

    Button backBtn;
    Button nextBtn;
    Button easyBtn;
    Button mideumBtn;
    Button hardBtn;

	// Use this for initialization
	void Start () {

        backBtn = transform.Find("Button_Back").GetComponent<Button>();
        nextBtn = transform.Find("Button_Next").GetComponent<Button>();
        easyBtn = transform.Find("Button_Easy").GetComponent<Button>();
        mideumBtn = transform.Find("Button_Medium").GetComponent<Button>();
        hardBtn = transform.Find("Button_Hard").GetComponent<Button>();

        backBtn.onClick.AddListener(()=> {
            SongListMain.instance.BackToMain();
        });

        nextBtn.onClick.AddListener(()=> {
            SongListMain.instance.GoNext();
        });

        easyBtn.onClick.AddListener(() => {
            SongListMain.instance.SelectDifficult(0);
        });

        mideumBtn.onClick.AddListener(() => {
            SongListMain.instance.SelectDifficult(1);

        });

        hardBtn.onClick.AddListener(() => {
            SongListMain.instance.SelectDifficult(2);

        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
