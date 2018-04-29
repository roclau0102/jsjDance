using UnityEngine;
using System.Collections;

public class PlayerScoreData  {

    public int[] juadeType2Count = new int[5]{0,0,0,0,0};
    public int comboCount = 0;
    public int score=0;

    public int GetWinMoney()
    {
        return (int)score / 100;
    }

    /// <summary>
    /// 得到级别
    /// </summary>
    /// <returns></returns>
     public Global.WIN_GRADE GetGrade()
    {
        float a = this.accurate;
        if (a <= 1 && a > 0.95f)
        {
            return Global.WIN_GRADE.S;
        }
        else if (a <= .95f && a > 0.9f)
        {
            return Global.WIN_GRADE.A;
        }
        else if (a <= 0.9f && a > 0.8f)
        {
            return Global.WIN_GRADE.B;
        }

        else if (a <= 0.8f && a > 0.7f)
        {
            return Global.WIN_GRADE.C;
        }
        return Global.WIN_GRADE.D;
    }



    /// <summary>
    /// 同步率
    /// </summary>
    public float accurate
    {
        get
        {
            float result=0;
            float max = 0;
            for (int i = 0; i < juadeType2Count.Length; i++)
            {
                max += juadeType2Count[i];
            }
            result = (juadeType2Count[0] + juadeType2Count[1]) / max;            
            return result;
        }
    }

    public void AddJuade(SongData data, Global.TIMING_JUAGE_TYPE type)
    {
        juadeType2Count[(int)type]++;        
    }

    /// <summary>
    /// 计算同步率
    /// </summary>
    /// <param name="data"></param>
    public void CountRightPercent(SongData data)
    {
        float p = DataUtils.GetMusicRightPercent(data, DataUtils.difficult);
        if ((accurate*100) > p)
        {
            DataUtils.SaveMusicRightPercent(data, DataUtils.difficult, accurate);
        }        
    }


    internal void SetMissCount(int p)
    {
        juadeType2Count[(int)Global.TIMING_JUAGE_TYPE.MISS] += p;
    }
}
