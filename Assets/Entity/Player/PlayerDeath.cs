using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    class DetachedPart
    {
        public Transform Transform;
        public Transform Parent;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public int Layer;
        public Rigidbody Body;
        public Collider Collider;
    }

    [SerializeField] Player player;
    [SerializeField] PlayerMovement movement;
    [SerializeField] PlayerLook look;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform gunModel;

    [SerializeField] float cameraRadius = 0.24f;
    [SerializeField] float gunRadius = 0.16f;
    [SerializeField] float bounciness = 0.2f;
    [SerializeField] float detachedFriction = 0.7f;
    [SerializeField] int detachedLayer = 2;

    [SerializeField] float cameraMass = 16f;
    [SerializeField] float gunMass = 7f;
    [SerializeField] float gravityScale = 2.4f;
    [SerializeField] float linearDamping = 0.25f;
    [SerializeField] float angularDamping = 1.6f;

    [SerializeField] float cameraUpKick = 0.9f;
    [SerializeField] float cameraBackKick = 1.1f;
    [SerializeField] float cameraSpin = 2.2f;
    [SerializeField] float gunUpKick = 1.4f;
    [SerializeField] float gunForwardKick = 1.3f;
    [SerializeField] float gunSpin = 5f;
    [SerializeField] float inheritedVelocity = 0.4f;

    [SerializeField] float hitStopScale = 0.12f;
    [SerializeField] float hitStopDuration = 0.11f;
    [SerializeField] float hitStopRecovery = 0.45f;

    readonly List<DetachedPart> detached = new();
    readonly List<Behaviour> suspended = new();

    PhysicsMaterial bounceMaterial;
    AudioHandle tinnitus;
    AudioHandle drone;
    Rigidbody playerBody;
    float hitStopElapsed = -1f;
    bool active;

    void Reset()
    {
        TryGetComponent(out player);
        TryGetComponent(out movement);
        TryGetComponent(out look);
    }

    void Awake()
    {
        if (!player) TryGetComponent(out player);
        if (!movement) TryGetComponent(out movement);
        if (!look) TryGetComponent(out look);
        if (!cameraTransform)
        {
            var found = GetComponentInChildren<Camera>(true);
            if (found) cameraTransform = found.transform;
        }
        if (!gunModel)
        {
            var sway = GetComponentInChildren<GunSway>(true);
            if (sway && sway.transform.childCount > 0) gunModel = sway.transform.GetChild(0);
        }

        TryGetComponent(out playerBody);

        bounceMaterial = new PhysicsMaterial("Death Debris")
        {
            bounciness = bounciness,
            dynamicFriction = detachedFriction,
            staticFriction = detachedFriction,
            bounceCombine = PhysicsMaterialCombine.Maximum,
            frictionCombine = PhysicsMaterialCombine.Average,
        };
    }


    void Update()
    {
        if (hitStopElapsed < 0f) return;

        hitStopElapsed += Time.unscaledDeltaTime;
        if (hitStopElapsed < hitStopDuration) return;

        float recovery = Mathf.Clamp01((hitStopElapsed - hitStopDuration) / Mathf.Max(0.0001f, hitStopRecovery));
        Time.timeScale = Mathf.Lerp(hitStopScale, 1f, recovery * recovery);

        if (recovery >= 1f)
        {
            Time.timeScale = 1f;
            hitStopElapsed = -1f;
        }
    }

    public void Play()
    {
        if (active) return;
        active = true;

        Vector3 inherited = playerBody ? playerBody.linearVelocity * inheritedVelocity : Vector3.zero;

        Suspend(movement);
        Suspend(look);
        if (player && player.Gun) player.Gun.InputLocked = true;

        if (playerBody)
        {
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
            playerBody.isKinematic = true;
        }

        SuspendGunRig();

        if (gunModel)
        {
            Vector3 forward = gunModel.forward;
            Detach(gunModel, gunRadius, gunMass,
                inherited + forward * gunForwardKick + Vector3.up * gunUpKick + Random.insideUnitSphere * 0.35f,
                Random.onUnitSphere * gunSpin, false);
        }

        if (cameraTransform)
        {
            Vector3 back = -cameraTransform.forward;
            Detach(cameraTransform, cameraRadius, cameraMass,
                inherited + back * cameraBackKick + Vector3.up * cameraUpKick + Random.insideUnitSphere * 0.25f,
                Random.onUnitSphere * cameraSpin, true);
        }

        AudioManager.PlayEventAt(SfxEvent.Death, transform.position);
        AudioManager.DuckBus(AudioBus.World, 0.12f, 1.6f, 1.2f);
        AudioManager.DuckBus(AudioBus.Enemy, 0.1f, 1.6f, 1.2f);
        tinnitus = AudioManager.Loop(Sfx.Tinnitus, null, 0.55f);
        drone = AudioManager.Loop(Sfx.Drone, null, 0.8f);

        hitStopElapsed = 0f;
        Time.timeScale = hitStopScale;
    }

    void SuspendGunRig()
    {
        foreach (var sway in GetComponentsInChildren<GunSway>(true)) Suspend(sway);
        foreach (var recoil in GetComponentsInChildren<GunRecoil>(true)) Suspend(recoil);
        foreach (var chamber in GetComponentsInChildren<GunChamber>(true)) Suspend(chamber);
    }

    public void Restore()
    {
        if (!active) return;
        active = false;

        hitStopElapsed = -1f;
        Time.timeScale = 1f;

        AudioManager.Stop(tinnitus, 0.35f);
        AudioManager.Stop(drone, 0.5f);

        foreach (var part in detached)
        {
            if (part.Transform && part.Transform.TryGetComponent(out DeathDebris debris)) Destroy(debris);
        }

        foreach (var part in detached)
        {
            if (part.Body) Destroy(part.Body);
            if (part.Collider) Destroy(part.Collider);

            part.Transform.SetParent(part.Parent, false);
            part.Transform.SetLocalPositionAndRotation(part.LocalPosition, part.LocalRotation);
            SetLayerRecursive(part.Transform, part.Layer);
        }
        detached.Clear();

        foreach (var behaviour in suspended)
        {
            if (behaviour) behaviour.enabled = true;
        }
        suspended.Clear();

        if (playerBody)
        {
            playerBody.isKinematic = false;
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
        }

        if (player && player.Gun) player.Gun.InputLocked = false;
    }

    void Detach(Transform target, float radius, float mass, Vector3 velocity, Vector3 spin, bool levelHorizon)
    {
        var part = new DetachedPart
        {
            Transform = target,
            Parent = target.parent,
            LocalPosition = target.localPosition,
            LocalRotation = target.localRotation,
            Layer = target.gameObject.layer,
        };

        target.SetParent(null, true);
        SetLayerRecursive(target, detachedLayer);

        var collider = target.gameObject.AddComponent<SphereCollider>();
        collider.radius = radius;
        collider.material = bounceMaterial;
        collider.hideFlags = HideFlags.DontSave;

        var body = target.gameObject.AddComponent<Rigidbody>();
        body.mass = mass;
        body.linearDamping = linearDamping;
        body.angularDamping = angularDamping;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.linearVelocity = velocity;
        body.angularVelocity = spin;
        body.hideFlags = HideFlags.DontSave;

        var debris = target.gameObject.AddComponent<DeathDebris>();
        debris.Configure(gravityScale, levelHorizon);
        debris.hideFlags = HideFlags.DontSave;

        part.Body = body;
        part.Collider = collider;
        detached.Add(part);
    }

    void Suspend(Behaviour behaviour)
    {
        if (!behaviour || !behaviour.enabled) return;
        behaviour.enabled = false;
        suspended.Add(behaviour);
    }

    static void SetLayerRecursive(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; ++i)
        {
            SetLayerRecursive(root.GetChild(i), layer);
        }
    }
}
