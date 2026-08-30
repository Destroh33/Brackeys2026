public readonly struct SfxLayer
{
    public readonly string Cue;
    public readonly float Delay;
    public readonly float Volume;
    public readonly float Jitter;

    public SfxLayer(string cue, float delay, float volume, float jitter)
    {
        Cue = cue;
        Delay = delay;
        Volume = volume;
        Jitter = jitter;
    }
}

public static class SfxEvent
{
    const float None = 0f;
    const float Light = 0.035f;
    const float Wide = 0.11f;

    static SfxLayer L(string cue, float delay = 0f, float volume = 1f, float jitter = Light)
        => new(cue, delay, volume, jitter);

    // ------------------------------------------------------------- revolver

    public static readonly SfxLayer[] Fire =
    {
        L(Sfx.CylinderTick, 0.04f, 0.55f, Wide),
    };

    public static readonly SfxLayer[] Eject =
    {
        L(Sfx.CylinderTick, 0f, 0.8f, Wide),
        L(Sfx.Eject, 0.07f, 0.9f, Light),
    };

    public static readonly SfxLayer[] DryFire =
    {
        L(Sfx.DryFire, 0f, 1f, Light),
        L(Sfx.CylinderTick, 0.05f, 0.5f, Wide),
    };

    public static readonly SfxLayer[] Reload =
    {
        L(Sfx.Reload, 0f, 1f, None),
        L(Sfx.CylinderTick, 0.62f, 0.5f, Wide),
    };

    // --------------------------------------------------------------- impact

    public static readonly SfxLayer[] HitConcrete = { L(Sfx.ImpactConcrete, 0f, 1f, Wide) };
    public static readonly SfxLayer[] HitWater = { L(Sfx.ImpactWater, 0f, 1f, Wide) };
    public static readonly SfxLayer[] HitPellet = { L(Sfx.ImpactConcrete, 0f, 0.5f, Wide) };
    public static readonly SfxLayer[] HitDebris = { L(Sfx.ImpactDebris, 0f, 0.6f, Wide) };

    public static readonly SfxLayer[] HitFlesh =
    {
        L(Sfx.EnemyHit, 0f, 0.9f, Wide),
    };

    // -------------------------------------------------------------- bullets

    public static readonly SfxLayer[] GasLaunch = { L(Sfx.GasLaunch, 0f, 1f, Light) };

    public static readonly SfxLayer[] GasBurst =
    {
        L(Sfx.GasBurst, 0f, 1f, Light),
    };

    public static readonly SfxLayer[] FlareIgnite =
    {
        L(Sfx.GasBurst, 0f, 0.7f, Light),
        L(Sfx.ImpactDebris, 0.06f, 0.45f, Wide),
    };

    public static readonly SfxLayer[] FlareStrike =
    {
        L(Sfx.NukeWhistle, 0f, 0.55f, Wide),
    };

    public static readonly SfxLayer[] DiamondDust =
    {
        L(Sfx.FirePiercing, 0f, 0.1f, Light),
    };

    public static readonly SfxLayer[] NukeLaunch =
    {
        L(Sfx.NukeLaunch, 0f, 1f, None),
        L(Sfx.NukeSub, 0f, 0.35f, None),
    };

    public static readonly SfxLayer[] NukeDetonate =
    {
        L(Sfx.NukeBlast, 0f, 1f, None),
        L(Sfx.NukeSub, 0.02f, 0.9f, None),
        L(Sfx.NukeRumble, 0.3f, 0.75f, None),
        L(Sfx.ImpactDebris, 1.0f, 0.5f, Wide),
        L(Sfx.ImpactDebris, 1.5f, 0.35f, Wide),
    };

    public static readonly SfxLayer[] BangPop =
    {
        L(Sfx.Pop, 0f, 1f, Light),
        L(Sfx.Confetti, 0.06f, 0.7f, Light),
    };

    public static readonly SfxLayer[] BangBounce = { L(Sfx.ImpactDebris, 0f, 0.55f, Wide) };

    // scratch, then the sting lands after it clears, then the return whoosh last
    public static readonly SfxLayer[] FuckYou =
    {
        L(Sfx.Scratch, 0f, 1f, None),
        L(Sfx.Sting, 0.58f, 0.9f, None),
    };

    public static string ReportFor(string bullet)
    {
        return bullet switch
        {
            "ShotgunBulletData" => Sfx.FireShotgun,
            "PiercingBulletData" => Sfx.FirePiercing,
            "LaserBulletData" => Sfx.FireLaser,
            "PoisonGasBulletData" => Sfx.GasLaunch,
            "NukeBulletData" => Sfx.NukeLaunch,
            "FlareBulletData" => Sfx.GasLaunch,
            "WaterGunBulletData" => "",
            "BangGunBulletData" => "",
            _ => Sfx.Fire9mm,
        };
    }

    public static readonly SfxLayer[] Whizz = { L(Sfx.Whoosh, 0f, 0.5f, Wide) };

    // -------------------------------------------------------------- enemies

    public static readonly SfxLayer[] EnemyAlert = { L(Sfx.EnemyAlert, 0f, 1f, Light) };

    public static readonly SfxLayer[] MeleeWindUp =
    {
        L(Sfx.EnemyWindUp, 0f, 1f, Light),
        L(Sfx.WhooshReverse, 0.08f, 0.45f, Light),
    };

    public static readonly SfxLayer[] MeleeSwing = { L(Sfx.EnemySwing, 0f, 1f, Light) };

    public static readonly SfxLayer[] MeleeHit =
    {
        L(Sfx.EnemyHit, 0f, 1f, Light),
        L(Sfx.BodyDrop, 0.03f, 0.5f, Light),
    };

    public static readonly SfxLayer[] MeleeMiss = { L(Sfx.EnemySwing, 0f, 0.6f, Wide) };

    public static readonly SfxLayer[] EnemyAim = { L(Sfx.WhooshReverse, 0f, 0.5f, Light) };
    public static readonly SfxLayer[] EnemyFire = { L(Sfx.EnemyGun, 0f, 1f, Light) };

    public static readonly SfxLayer[] EnemyShatter =
    {
        L(Sfx.EnemyShatter, 0f, 1f, Light),
        L(Sfx.ImpactDebris, 0.14f, 0.5f, Wide),
        L(Sfx.ImpactDebris, 0.32f, 0.35f, Wide),
    };

    public static readonly SfxLayer[] EnemyStep = { L(Sfx.StepConcrete, 0f, 0.6f, Wide) };

    // --------------------------------------------------------------- player

    public static readonly SfxLayer[] StepConcrete = { L(Sfx.StepConcrete, 0f, 1f, Wide) };
    public static readonly SfxLayer[] StepMetal = { L(Sfx.StepMetal, 0f, 1f, Wide) };
    public static readonly SfxLayer[] Jump = { L(Sfx.Jump, 0f, 0.9f, Wide) };
    public static readonly SfxLayer[] LandSoft = { L(Sfx.Land, 0f, 0.8f, Wide) };

    public static readonly SfxLayer[] LandHard =
    {
        L(Sfx.Land, 0f, 1f, Light),
        L(Sfx.BodyDrop, 0.02f, 0.4f, Light),
    };

    public static readonly SfxLayer[] Death =
    {
        L(Sfx.PlayerDeath, 0f, 1f, None),
        L(Sfx.NukeSub, 0f, 0.3f, None),
    };

    public static readonly SfxLayer[] BodyBounce = { L(Sfx.BodyDrop, 0f, 0.7f, Wide) };
    public static readonly SfxLayer[] GunClatter = { L(Sfx.GunClatter, 0f, 0.8f, Light) };

    // ---------------------------------------------------------------- world

    public static readonly SfxLayer[] DoorOpen = { L(Sfx.DoorOpen, 0f, 1f, None) };

    // ------------------------------------------------------------------- ui

    public static readonly SfxLayer[] UiHover = { L(Sfx.CylinderTick, 0f, 0.35f, Wide) };

    public static readonly SfxLayer[] UiClick =
    {
        L(Sfx.Eject, 0f, 0.7f, Light),
    };

    public static readonly SfxLayer[] UiStart =
    {
        L(Sfx.Reload, 0f, 0.9f, None),
    };

    public static readonly SfxLayer[] UiBack = { L(Sfx.DryFire, 0f, 0.6f, Light) };

    public static readonly SfxLayer[] UiScreen = { L(Sfx.Whoosh, 0f, 0.45f, Light) };

    public static readonly SfxLayer[] UiCopy =
    {
        L(Sfx.CylinderTick, 0f, 0.6f, Light),
        L(Sfx.Eject, 0.05f, 0.35f, Light),
    };
}
