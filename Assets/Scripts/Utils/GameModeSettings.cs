using UnityEngine;
using System.Collections;

public class GameModeSettings : ScriptableObject
{
    public TYPE type;
    public enum TYPE
    {
        正常模式,
        叠加安卓视频模式
    }
}
