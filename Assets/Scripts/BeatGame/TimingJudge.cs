using UnityEngine;
using System.Collections;

public class TimingJudge : MonoBehaviour {

    public PlayParticle[] judgeEffects;

    public void Awake()
    {
        AnimationComplete();
    }

    public void AnimationComplete()
    {
        for (int i = 0; i < judgeEffects.Length; i++)
        {
            judgeEffects[i].gameObject.SetActive(false);
        } 
    }

    

    public void Show(Global.TIMING_JUAGE_TYPE type)
    {
        for (int i = 0; i < judgeEffects.Length; i++)
        {
            judgeEffects[i].gameObject.SetActive(i == (int)type);
            judgeEffects[i].PlayParticles();
        }
        gameObject.GetComponent<Animation>().Stop();
        gameObject.GetComponent<Animation>().Play(PlayMode.StopAll);        
    }

}
