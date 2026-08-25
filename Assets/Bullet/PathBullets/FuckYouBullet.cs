using System;
using System.Collections;
using UnityEngine;

public class FuckYouBulletMovement : MonoBehaviour
{
    [SerializeField] Bullet bullet;
    [SerializeField] float speed = 30.0f;
    [SerializeField] float turnAroundDelay = 2.0f;
    [SerializeField] float rotateDuration = 0.5f;
    [SerializeField] float rotateLerpMult = 30.0f;

    float spawnFixedTime;
    bool turnedAround = false;

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
        bullet.RB.linearVelocity = Vector3.zero;
        bullet.RB.isKinematic = true;

        float startTime = Time.time;
        while(Time.time < startTime + rotateDuration)
        {
            Quaternion targetRot = Quaternion.LookRotation(
                GameManager.Instance.Player.Center.position - bullet.RB.transform.position
            );
            bullet.RB.MoveRotation(Quaternion.Lerp(bullet.RB.rotation, targetRot, Time.deltaTime * rotateLerpMult));
            yield return null;
        }

        bullet.RB.isKinematic = false;
        bullet.RB.linearVelocity = transform.forward * speed;
    }
}
