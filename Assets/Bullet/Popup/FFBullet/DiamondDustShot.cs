using UnityEngine;
using System.Collections;

public class DiamondDustShot : MonoBehaviour
{
    [SerializeField] int shotCount = 10;
    [SerializeField] float shotInterval = 0.2f;
    [SerializeField] float dispersionfactor = 0.2f;
    [SerializeField] GameObject projectile;
    PlayerGun gun;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gun = GameManager.Instance.Player.Gun;
        StartCoroutine(DiamondDust());
    }

    IEnumerator DiamondDust()
    {
        for (int i = 0; i < shotCount; ++i)
        {
            GameObject proj = gun.SpawnAimedBullet(projectile);
            if (proj) proj.transform.Rotate(Random.insideUnitSphere * Random.Range(0, dispersionfactor));
            AudioManager.PlayEventOn(SfxEvent.DiamondDust, transform);
            yield return new WaitForSeconds(shotInterval);
        }
        Destroy(gameObject);
    }
}
