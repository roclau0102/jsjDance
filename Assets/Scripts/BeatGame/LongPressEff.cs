using UnityEngine;
using System.Collections;

public class LongPressEff : MonoBehaviour {
    public GameObject prefab;

    public void Restore()
    {
        PrefabPool.restore(gameObject, prefab);
    }
}
