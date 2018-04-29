using UnityEngine;
using System.Collections;

public class TimeDisable : MonoBehaviour {
    public float liveTIme=1;

    float startTime;
	// Use this for initialization
	void Start () {
	
	}

    void OnEnable()
    {
        startTime = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.time - startTime > liveTIme)
        {
            gameObject.SetActive(false);
        }
	}
}
