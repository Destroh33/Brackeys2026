using UnityEngine;
using System.Collections.Generic;

public class PokemonBullet : MonoBehaviour
{
    [SerializeField] List<GameObject> bulletPrefabs;

    Transform muzzle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MusicManager.Push(this, Sfx.MusicPika);
        muzzle = GameManager.Instance.Player.GetComponent<PlayerGun>().muzzle;
    }

    void OnDestroy()
    {
        MusicManager.Pop(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShootBullet(int type)
    {
        if (type < 0 || type >= bulletPrefabs.Count)
        {
            return;
        }

        Instantiate(bulletPrefabs[type], muzzle.position, muzzle.rotation);
    }
}
