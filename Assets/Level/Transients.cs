using UnityEngine;

public static class Transients
{
    public static void Clear()
    {
        Sweep<Bullet>();
        Sweep<AphorismBullet>();
        Sweep<BangPopBullet>();
        Sweep<ShotgunBullet>();
        Sweep<LaserBullet>();
        Sweep<WaterGunBullet>();
        Sweep<PokemonBullet>();
        Sweep<DiamondDustShot>();
        Sweep<FuckYouBulletMovement>();
        Sweep<ImFreeBulletMovement>();

        Sweep<WaterBlob>();
        Sweep<PoisonGasCloud>();
        Sweep<NukeExplosion>();
        Sweep<FlareStrike>();
        Sweep<FlareStrikeWarning>();
        Sweep<PopSign>();
        Sweep<BlockPiece>();
        Sweep<SelfDestruct>();

        Sweep<KeyPromptPopup>();
        Sweep<KeyPromptSequencePopup>();
        Sweep<FFBulletUI>();
        Sweep<PopupBulletActions>();

        AudioManager.StopBus(AudioBus.VoiceLine, 0.15f);
    }

    public static bool HazardsLive()
    {
        return Any<NukeExplosion>() || Any<FlareStrike>() || Any<FlareStrikeWarning>() || Any<PoisonGasCloud>();
    }

    static bool Any<T>() where T : Component
    {
        return Object.FindAnyObjectByType<T>(FindObjectsInactive.Exclude);
    }

    static void Sweep<T>() where T : Component
    {
        var found = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var component in found)
        {
            if (component) Object.Destroy(component.gameObject);
        }
    }
}
