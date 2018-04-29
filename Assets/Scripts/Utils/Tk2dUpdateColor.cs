using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Tk2dUpdateColor : MonoBehaviour {

    public Color color = Color.white;
    private tk2dSprite s;

	// Use this for initialization
	void Start () {
        s = GetComponent<tk2dSprite>();
	}
	
	// Update is called once per frame
	void Update () {
        if (color != s.color)
        {
            s.color = color;
        }
	}
}
