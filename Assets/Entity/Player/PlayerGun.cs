using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [SerializeField]
    int NumChambers = 6;

    // Circular buffer of bulletCapacity elements
    BulletData[] chambers;
    int chamberIndex = 0;
    public int ChamberIndex => chamberIndex;

    /// <summary>
    /// The bullet data in the current chamber, or null if the chamber is empty
    /// </summary>
    public BulletData Chamber => chambers[chamberIndex];
    public IReadOnlyList<BulletData> Chambers => chambers;

    public Transform muzzle;
    public Transform cameraTransform;
    public float range = 200f;
    public LayerMask aimMask = ~0;

    public GunRecoil gunRecoil;
    public GunChamber gunChamber;
    public PlayerLook playerLook;

    public float fireRate = 6f;
    public float cameraPitchRecoil = -1.4f;
    public float cameraYawRecoil = 0.35f;

    InputSystem_Actions actions;
    float nextFireTime;

    void Awake()
    {
        actions = new InputSystem_Actions();
        gunChamber.degreesPerShot = 360f / NumChambers;
    }

    void Start()
    {
        // Default init to null
        chambers = new BulletData[NumChambers];
    }

    void OnEnable()
    {
        actions.Player.Enable();
    }

    void OnDisable()
    {
        actions.Player.Disable();
    }

    void OnDestroy()
    {
        actions.Dispose();
    }

    void Update()
    {
        if (actions.Player.Attack.WasPressedThisFrame() && Time.time >= nextFireTime) Shoot();
    }

    void Shoot()
    {
        nextFireTime = Time.time + 1f / fireRate;

        BulletData data = PopChamber();
        if (data != null)
        {
            SpawnBullet(data);
            gunRecoil.Kick();
            playerLook.AddRecoil(cameraPitchRecoil, Random.Range(-cameraYawRecoil, cameraYawRecoil));
        }

        gunChamber.Advance();
    }

    void SpawnBullet(BulletData data)
    {
        Vector3 aimPoint = cameraTransform.position + cameraTransform.forward * range;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, range, aimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        Vector3 direction = (aimPoint - muzzle.position).normalized;
        Instantiate(data.Prefab, muzzle.position, Quaternion.LookRotation(direction));
    }

    public void SkipChamber()
    {
        chamberIndex = (chamberIndex + 1) % NumChambers;
    }

    public BulletData PopChamber()
    {
        var data = Chamber;
        chambers[chamberIndex] = null;
        SkipChamber();
        return data;
    }

    public void SetBullets(IReadOnlyList<BulletData> bullets)
    {
        if (bullets.Count > NumChambers)
        {
            throw new System.InvalidOperationException("Tried to set bullets with wrong number of bullets");
        }
        for (int i = 0; i < NumChambers; ++i)
        {
            chambers[i] = i < bullets.Count ? bullets[i] : null;
        }
        chambers.Shuffle();
    }
}
