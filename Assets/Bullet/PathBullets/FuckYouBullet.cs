using System.Collections;
using UnityEngine;

public class FuckYouBulletMovement : MonoBehaviour
{
    [SerializeField] Bullet bullet;
    [SerializeField] float speed = 30.0f;
    [SerializeField] float turnAroundDelay = 2.0f;
    [SerializeField] float rotateDuration = 0.5f;
    [SerializeField] float returnSpeed = 18.0f;

    [SerializeField] GameObject signPrefab;
    [SerializeField] float signHeight = 0.45f;

    float spawnFixedTime;
    bool turnedAround;

    void Reset()
    {
        TryGetComponent(out bullet);
    }

    void Start()
    {
        spawnFixedTime = Time.fixedTime;
        bullet.RB.linearVelocity = transform.forward * speed;
    }

    void FixedUpdate()
    {
        if (!turnedAround && Time.fixedTime > spawnFixedTime + turnAroundDelay)
        {
            turnedAround = true;
            StartCoroutine(TurnAroundCoro());
        }
    }

    IEnumerator TurnAroundCoro()
    {
        var body = bullet.RB;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;

        SpawnSign();

        Quaternion start = transform.rotation;
        Quaternion target = start;
        float elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;

            target = Quaternion.LookRotation(AimDirection());
            transform.rotation = Quaternion.Slerp(start, target, Mathf.SmoothStep(0f, 1f, elapsed / rotateDuration));
            yield return null;
        }

        transform.rotation = target;

        body.isKinematic = false;
        body.linearVelocity = transform.forward * returnSpeed;
    }

    void SpawnSign()
    {
        var prefab = signPrefab ? signPrefab : Resources.Load<GameObject>("FuckYouSign");
        if (!prefab) return;

        var sign = Instantiate(prefab, transform.position + Vector3.up * signHeight, Quaternion.identity);
        if (sign.TryGetComponent(out PopSign pop)) pop.Attach(transform, signHeight);
    }

    Vector3 AimDirection()
    {
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        Transform aim = player ? player.Center : null;
        if (!aim) return transform.forward;

        Vector3 toPlayer = aim.position - transform.position;
        return toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : transform.forward;
    }
}
