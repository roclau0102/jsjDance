using UnityEngine;
using System.Collections;

public class ITweenTextValue: MonoBehaviour  {
    static ITweenTextValue _instance;
    public static ITweenTextValue instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go =(new GameObject());
                _instance = go.AddComponent<ITweenTextValue>();
                _instance.gameObject.name = "ITweenTextValue";
            }
            return _instance;
        }
    }

    tk2dTextMesh text;

    public void SetTo(tk2dTextMesh text, int from, int to, float delay, float time)
    {
        this.text = text;
        iTween.ValueTo(gameObject,
            iTween.Hash("from", from, "to", to, "time", time, "delay", delay,  "onupdate", "onUpdate", "oncomplete", "OnComplete"));
    }

    void OnComplete()
    {
        DestroyImmediate(this);
    }

    void onUpdate(int v)
    {
        text.text = v.ToString();
    }



    public void SetTextIntTween(tk2dTextMesh t, int toValue, float delay=0, float time=1.5f)
    {
        ITweenTextValue tween = t.gameObject.AddComponent<ITweenTextValue>();
        tween.SetTo(t, int.Parse(t.text), toValue, delay, time);        
    }
}
