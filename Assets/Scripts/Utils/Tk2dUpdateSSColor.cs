using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Tk2dUpdateSSColor : MonoBehaviour
{

    public Color color = Color.white;
    private tk2dSlicedSprite s;

	// Use this for initialization
	void Start () {
        s = GetComponent<tk2dSlicedSprite>();
	}
	
	// Update is called once per frame
	void Update () {
        if (color != s.color)
        {
            s.color = color;
        }
	}
}
