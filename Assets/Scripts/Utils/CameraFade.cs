using UnityEngine;
using System.Collections;

public class CameraFade : MonoBehaviour {

    public Tk2dUpdateColor setting;
    public tk2dSprite sprite;

    void Awake()
    {
        setting.transform.localPosition = new Vector3(0, 0, 1);
    }


    public void In(bool isBlack=true)
    {
        setting.transform.localPosition = new Vector3(0, 0, 1);
        setting.color = Color.white;
        sprite.color = setting.color;
        sprite.SetSprite(isBlack?"blackbg":"whitebg");
        GetComponent<Animation>().Stop();
        GetComponent<Animation>().Play("CameraFadeIn");
        Invoke("AnimComplete", 0.2f);
    }

    public void AnimComplete()
    {
        Debug.Log("Camera  AnimComplete~~");
        setting.transform.localPosition = new Vector3(0, 599, 1);
    }


    public void Out(bool isBlack=true)
    {
        setting.transform.localPosition = new Vector3(0, 0, 1);
        Color c = setting.color;
        c = Color.white;
        c.a = 0;
        setting.color = c;
        sprite.color = setting.color;
        GetComponent<Animation>().Stop();
        sprite.SetSprite(isBlack ? "blackbg" : "whitebg");
        GetComponent<Animation>().Play("CameraFadeOut");
    }

}
