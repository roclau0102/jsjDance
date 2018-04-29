using UnityEngine;
using System.Collections;

public class Money : MonoBehaviour {
    public tk2dTextMesh text;
    bool isFirstTime = true;
	// Use this for initialization
	void Start () {        
        DataUtils.ChangeMoneyCallback = (v) =>
        {
            if (isFirstTime)
            {
                text.text = v.ToString();
                isFirstTime = false;
            }
            else
            {
                int now = int.Parse(text.text);
                iTween.ValueTo(gameObject, iTween.Hash("from", now, "to", v, "time", .5f, "onupdate", "OnUpdate"));
            }            
        };
        DataUtils.AddMoney(0);
	}

    void OnDisable()
    {
        DataUtils.ChangeMoneyCallback = null;
    }

    void OnDestroy()
    {
        iTween.Stop();
    }

    void OnUpdate(int v)
    {
        text.text = v.ToString();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
