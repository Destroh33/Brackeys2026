using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bullet Pool", menuName = "Bullet Pool")]
public class BulletPool : ScriptableObject
{
    public List<BulletData> Bullets;
}
