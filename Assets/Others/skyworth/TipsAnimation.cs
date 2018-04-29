using UnityEngine;
using System.Collections;

public class TipsAnimation : MonoBehaviour
{
    public float alpha = 1;
    Color c = Color.white;

    void Update()
    {
        if(c.a==alpha)return;
        c.a=alpha;
        GetComponent<Renderer>().material.SetColor("_Color", c);
    }

    void OnDisable()
    {
        c.a = 0;
        GetComponent<Renderer>().material.SetColor("_Color", c);        
    }
}