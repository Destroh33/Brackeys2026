using System.Collections;
using UnityEngine;

public class CrtScreen : MonoBehaviour
{
    const int Width = 320;
    const int Height = 200;
    const int Samples = 1024;
    const int GrainSize = 4096;

    [SerializeField] Renderer surface;
    [SerializeField] Light glow;
    [SerializeField] AudioSource source;
    [SerializeField] Color phosphor = new(0.4f, 1f, 0.64f);
    [SerializeField] float traceHeight = 0.4f;
    [SerializeField] float gain = 4.5f;
    [SerializeField] float waveLength = 40f;
    [SerializeField] float attackRate = 26f;
    [SerializeField] float releaseRate = 4.5f;
    [SerializeField] float speed = 1f;
    [SerializeField] float spread = 0.3f;
    [SerializeField] float jitter = 0.3f;
    [SerializeField] float lineWidth = 2.2f;
    [SerializeField] float glowFloor = 1.1f;
    [SerializeField] float glowResponse = 0.7f;
    [SerializeField] float grainAmount = 0.012f;
    [SerializeField] bool startPowered;

    public float Level { get; private set; }

    Texture2D texture;
    Color32[] pixels;
    float[] field;

    readonly float[] samples = new float[Samples];
    readonly float[] fadeX = new float[Width];
    readonly float[] fadeY = new float[Height];
    readonly float[] window = new float[Width];
    readonly float[] grain = new float[GrainSize];

    Material material;

    bool on;
    float power;
    float warmth = 1f;
    float collapse;
    float envelope;
    float punch;
    float clock;
    int drift;

    void Reset()
    {
        surface = GetComponent<Renderer>();
    }

    void Awake()
    {
        if (!surface) surface = GetComponent<Renderer>();

        if (!surface)
        {
            enabled = false;
            return;
        }

        material = surface.material;
        field = new float[Width * Height];
        pixels = new Color32[Width * Height];

        texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        material.mainTexture = texture;
        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_MainTex", texture);

        float edge = Mathf.Clamp(spread, 0.01f, 0.49f);

        for (int x = 0; x < Width; ++x)
        {
            float u = x / (Width - 1f);
            float d = Mathf.Abs(u * 2f - 1f);
            fadeX[x] = Mathf.Clamp01(1f - d * d * 0.5f);
            window[x] = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - d) / edge));
        }

        for (int y = 0; y < Height; ++y)
        {
            float d = Mathf.Abs(y / (Height - 1f) * 2f - 1f);
            fadeY[y] = Mathf.Clamp01(1f - d * d * 0.6f);
        }

        for (int i = 0; i < GrainSize; ++i) grain[i] = Random.value;

        if (startPowered) TurnOn();
    }

    public void TurnOn()
    {
        on = true;
        power = 0f;
        warmth = 1f;
        collapse = 0f;
    }

    public IEnumerator TurnOff(float duration)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.01f, duration))
        {
            collapse = Mathf.Clamp01(t);
            yield return null;
        }

        collapse = 1f;
        on = false;
        power = 0f;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (on)
        {
            power = Mathf.MoveTowards(power, 1f, dt / 0.3f);
            warmth = Mathf.MoveTowards(warmth, 0f, dt / 0.7f);
        }

        drift = (drift + 7919) & (GrainSize - 1);
        clock += dt * Mathf.Max(0f, speed);

        Listen(dt);
        Compose();
        Push();
    }

    void Listen(float dt)
    {
        bool live = source && source.isPlaying;
        if (live) source.GetOutputData(samples, 0);

        float sum = 0f;
        for (int i = 0; i < Samples; ++i) sum += live ? samples[i] * samples[i] : 0f;

        float loud = Safe(Mathf.Sqrt(sum / Samples));
        float target = live ? Mathf.Clamp01(loud * Mathf.Max(0.1f, gain)) : 0f;

        float rate = target > envelope ? Mathf.Max(1f, attackRate) : Mathf.Max(0.2f, releaseRate);
        float settled = Mathf.Lerp(envelope, target, 1f - Mathf.Exp(-rate * dt));

        punch = Mathf.Max(punch * Mathf.Exp(-dt * 7f), Mathf.Max(0f, settled - envelope) * 6f);
        envelope = Safe(settled);

        Level = Mathf.Lerp(Level, Mathf.Clamp01(loud * 3.2f), 0.25f);
    }

    float Deflection(int x)
    {
        float u = x / (Width - 1f);
        float cycles = Width / Mathf.Max(6f, waveLength);
        float p = u * Mathf.PI * 2f * cycles;

        float shape = Mathf.Sin(p + clock * 3.1f) * 0.55f +
                      Mathf.Sin(p * 2.13f - clock * 4.7f) * 0.28f +
                      Mathf.Sin(p * 3.71f + clock * 2.3f) * 0.17f;

        float spike = punch * Mathf.Sin(p * 5.3f + clock * 11f) * 0.5f;
        float fuzz = (grain[(x * 7 + drift) & (GrainSize - 1)] - 0.5f) * jitter;

        float raw = window[x] * envelope * (shape + fuzz + spike);

        return Mathf.Clamp(Safe(raw / (1f + Mathf.Abs(raw) * 0.45f)), -1f, 1f);
    }

    static float Safe(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
    }

    void Compose()
    {
        float squeeze = Squeeze();
        int center = Height / 2;
        float open = center * squeeze;
        float wash = 0.05f * power;
        float dust = (Mathf.Max(0f, grainAmount) + warmth * 0.45f) * power;

        for (int y = 0; y < Height; ++y)
        {
            int row = y * Width;

            if (Mathf.Abs(y - center) > open)
            {
                System.Array.Clear(field, row, Width);
                continue;
            }

            float scan = (y & 1) == 0 ? 1f : 0.35f;
            float rule = y % 25 == 0 ? 0.05f : 0f;
            float body = fadeY[y] * scan * wash;

            for (int x = 0; x < Width; ++x)
            {
                float grid = rule + (x % 32 == 0 ? 0.045f : 0f);
                field[row + x] = (body + grid * power) * fadeX[x] +
                                 grain[(row + x + drift) & (GrainSize - 1)] * dust;
            }
        }

        if (squeeze > 0.02f) Trace(center, squeeze);
        if (collapse > 0f) Pinch(center);
    }

    void Trace(int center, float squeeze)
    {
        float beam = Mathf.Clamp01(power);
        if (beam <= 0.001f) return;

        float amp = Height * Mathf.Max(0.05f, traceHeight) * squeeze;
        float previous = center + Deflection(0) * amp;

        for (int x = 0; x < Width; ++x)
        {
            float current = center + Deflection(x) * amp;
            Stroke(x, previous, current, beam);
            previous = current;
        }
    }

    void Stroke(int x, float a, float b, float beam)
    {
        if (b < a) (a, b) = (b, a);

        float half = Mathf.Max(0.5f, lineWidth) * 0.5f;
        float halo = half + 2.5f;

        int lo = Mathf.Clamp((int)(a - halo), 0, Height - 1);
        int hi = Mathf.Clamp((int)(b + halo) + 1, 0, Height - 1);

        for (int y = lo; y <= hi; ++y)
        {
            float d = y < a ? a - y : (y > b ? y - b : 0f);
            float core = Mathf.Clamp01(half + 0.5f - d);
            float bleed = Mathf.Clamp01(1f - d / halo);

            field[y * Width + x] += (core + bleed * bleed * 0.2f) * beam;
        }
    }

    void Pinch(int center)
    {
        float t = Mathf.InverseLerp(0.35f, 1f, collapse);
        if (t <= 0f) return;

        float shrink = t < 0.55f ? 1f : Mathf.Pow(1f - (t - 0.55f) / 0.45f, 0.6f);
        float bright = 1.8f * Mathf.Min(1f, t * 3f) * shrink;
        float half = Width * 0.5f * shrink;

        int from = Mathf.Clamp(Mathf.RoundToInt(Width * 0.5f - half), 0, Width);
        int to = Mathf.Clamp(Mathf.RoundToInt(Width * 0.5f + half), 0, Width);

        for (int x = from; x < to; ++x)
        {
            field[center * Width + x] += bright;
            field[(center - 1) * Width + x] += bright * 0.5f;
            field[(center + 1) * Width + x] += bright * 0.5f;
        }
    }

    void Push()
    {
        float r = phosphor.r;
        float g = phosphor.g;
        float b = phosphor.b;

        for (int i = 0; i < field.Length; ++i)
        {
            float v = field[i];
            if (v > 1f) v = 1f;
            else if (!(v > 0f)) v = 0f;

            pixels[i].r = (byte)(v * r * 255f);
            pixels[i].g = (byte)(v * g * 255f);
            pixels[i].b = (byte)(v * b * 255f);
            pixels[i].a = 255;
        }

        texture.SetPixels32(pixels);
        texture.Apply(false);

        material.SetColor("_BaseColor", new Color(1.3f, 1.3f, 1.3f, 1f));
        material.SetColor("_Color", new Color(1.3f, 1.3f, 1.3f, 1f));

        if (!glow) return;

        float wanted = (glowFloor + Level * glowResponse) * power * (1f - collapse);
        glow.intensity = Mathf.Lerp(glow.intensity, Safe(wanted), 0.12f);
    }

    float Squeeze()
    {
        return collapse <= 0f ? 1f : Mathf.Max(0f, 1f - collapse / 0.5f);
    }

    void OnDestroy()
    {
        if (texture) Destroy(texture);
    }
}
