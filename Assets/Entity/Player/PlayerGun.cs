using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [SerializeField]
    int numChambers = 6;

    // Circular buffer of bulletCapacity elements
    BulletData[] chambers;
    int chamberIndex = 0;

    /// <summary>
    /// The bullet data in the current chamber, or null if the chamber is empty
    /// </summary>
    public BulletData Chamber => chambers[chamberIndex];

    void Start()
    {
        // Default init to null
        chambers = new BulletData[numChambers];
    }

    public void SkipChamber()
    {
        chamberIndex = (chamberIndex + 1) % numChambers;
    }

    public BulletData PopChamber()
    {
        var data = Chamber;
        chambers[chamberIndex] = null;
        SkipChamber();
        return data;
    }
}
