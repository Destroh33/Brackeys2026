using System.Collections;
using UnityEngine;

public class WaterGunBullet : MonoBehaviour
{
    [SerializeField] GameObject blobPrefab;
    [SerializeField] ParticleSystem mist;
    [SerializeField] int blobCount = 26;
    [SerializeField] float streamDuration = 1.1f;
    [SerializeField] float speed = 17.0f;
    [SerializeField] float speedJitter = 2.5f;
    [SerializeField] float spreadDegrees = 3.0f;
    [SerializeField] float lingerAfterStream = 1.5f;
    [SerializeField] bool followMuzzle = true;

    void Start()
    {
        if (followMuzzle) AttachToMuzzle();
        StartCoroutine(SprayCoro());
    }

    void AttachToMuzzle()
    {
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        var muzzle = player && player.Gun ? player.Gun.muzzle : null;
        if (muzzle) transform.SetParent(muzzle, true);
    }

    IEnumerator SprayCoro()
    {
        float interval = streamDuration / Mathf.Max(1, blobCount);

        for (int i = 0; i < blobCount; ++i)
        {
            Vector3 direction = transform.rotation * Util.GetRandomDirectionInCone(spreadDegrees * Mathf.Deg2Rad);
            var blob = Instantiate(blobPrefab, transform.position, Quaternion.LookRotation(direction));
            if (blob.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = direction * (speed + Random.Range(-speedJitter, speedJitter));
            }
            yield return new WaitForSeconds(interval);
        }

        if (mist) mist.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(gameObject, lingerAfterStream);
    }
}
