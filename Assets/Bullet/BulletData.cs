using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Bullet Data")]
public class BulletData : ScriptableObject
{
    public string Name;
    public GameObject Prefab;
}
