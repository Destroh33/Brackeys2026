using UnityEngine;
using System.Collections;

public class DiamondDustShot : MonoBehaviour
{
    [SerializeField] int shotCount = 10;
    [SerializeField] float shotInterval = 0.2f;
    [SerializeField] float dispersionfactor = 0.2f;
    [SerializeField] GameObject projectile;
    Transform muzzle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        muzzle = GameManager.Instance.Player.GetComponent<PlayerGun>().muzzle;
        StartCoroutine(DiamondDust());
    }

    IEnumerator DiamondDust()
    {
        for (int i = 0; i < shotCount; ++i)
        {
            GameObject proj = Instantiate(projectile, muzzle.position, muzzle.rotation);
            proj.transform.Rotate(Random.insideUnitSphere * Random.Range(0, dispersionfactor));
            AudioManager.PlayEventOn(SfxEvent.DiamondDust, transform);
            yield return new WaitForSeconds(shotInterval);
        }
        Destroy(gameObject);
    }
}
