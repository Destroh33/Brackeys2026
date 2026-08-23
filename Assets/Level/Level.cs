using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [System.NonSerialized]
    public List<Enemy> Enemies;

    virtual public void StartLevel() {}
}
