using UnityEngine;
using System.Collections;
using UnityEditor;

#if UNITY_EDITOR
[CanEditMultipleObjects()]
[CustomEditor(typeof(DataUtils), true)]
public class DataUtilsExtension : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("加钱", GUILayout.Height(50)))
        {
            DataUtils.AddMoney(1000);
        }

        if (GUILayout.Button("加100000纠正道具", GUILayout.Height(50)))
        {
            DataUtils.correntPropCount += 100000;
        }

        if (GUILayout.Button("无限血量", GUILayout.Height(50)))
        {
            for (int i = 0; i < BeatGame.instance.players.Length; i++)
            {
                BeatGame.instance.players[i].SetMaxHP(100000);
                BeatGame.instance.players[i].AddHP(100000);
            }
        }

        if (GUILayout.Button("清空用户数据", GUILayout.Height(50)))
        {
            DataUtils.CleanUserData();
        }
    }
}
#endif