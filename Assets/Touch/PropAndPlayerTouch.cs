/*
 *Author by roc. All rights reserved. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropAndPlayerTouch : MonoBehaviour {

    PropScene ps;

    Button backBtn;
    Button nextBtn;

	// Use this for initialization
	void Start () {

        ps = GameObject.Find("Main").transform.GetComponent<PropScene>();
        backBtn = transform.Find("Button_Back").GetComponent<Button>();
        nextBtn = transform.Find("Button_Next").GetComponent<Button>();

        backBtn.onClick.AddListener(() => {
            ps.BackToSongList();
        });

        nextBtn.onClick.AddListener(()=> {
            ps.EnterGame();
        });

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
