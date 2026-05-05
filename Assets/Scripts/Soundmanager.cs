using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private float footstepVolume = 0.3f;
    [SerializeField] private float weaponVolume = 0.7f;

    [Header("Generated Clips")]
    public AudioClip gunshot;
    public AudioClip pistolShot;
    public AudioClip reload;
    public AudioClip footstep1;
    public AudioClip footstep2;
    public AudioClip footstep3;
    public AudioClip hitMarker;
    public AudioClip headshot;
    public AudioClip death;
    public AudioClip jump;
    public AudioClip land;
    public AudioClip dash;
    public AudioClip smokeThrow;
    public AudioClip smokeExplode;
    public AudioClip knifeSwing;
    public AudioClip knifeHit;
    public AudioClip uiClick;
    public AudioClip lowHealth;

    // Audio source pool
    private List<AudioSource> audioPool = new List<AudioSource>();
    private int poolSize = 10;

    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GenerateAllSounds();
            CreateAudioPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateAudioPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioPool.Add(source);
        }
    }

    AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in audioPool)
        {
            if (!source.isPlaying)
                return source;
        }
        // If all busy, use first one
        return audioPool[0];
    }

    void GenerateAllSounds()
    {
        // Weapon sounds
        gunshot = GenerateGunshot(0.3f, 800f, 100f);
        pistolShot = GeneratePistolShot(0.2f, 1000f, 150f);
        reload = GenerateReload(0.8f);

        // Movement sounds
        footstep1 = GenerateFootstep(0.1f, 100f);
        footstep2 = GenerateFootstep(0.1f, 120f);
        footstep3 = GenerateFootstep(0.1f, 90f);
        jump = GenerateJump(0.15f);
        land = GenerateLand(0.2f);
        dash = GenerateDash(0.25f);

        // Combat sounds
        hitMarker = GenerateHitMarker(0.1f);
        headshot = GenerateHeadshot(0.15f);
        death = GenerateDeath(0.4f);

        // Ability sounds
        smokeThrow = GenerateSmokeThrow(0.2f);
        smokeExplode = GenerateSmokeExplode(0.3f);

        // Knife sounds
        knifeSwing = GenerateKnifeSwing(0.15f);
        knifeHit = GenerateKnifeHit(0.1f);

        // UI sounds
        uiClick = GenerateUIClick(0.05f);
        lowHealth = GenerateLowHealth(0.5f);

        Debug.Log("All sounds generated!");
    }

    #region Sound Generators

    AudioClip GenerateGunshot(float duration, float frequency, float dropoff)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * dropoff);

            // Mix of noise and low frequency punch
            float noise = Random.Range(-1f, 1f) * envelope;
            float punch = Mathf.Sin(2 * Mathf.PI * frequency * t * Mathf.Exp(-t * 50)) * envelope * 2f;
            float bass = Mathf.Sin(2 * Mathf.PI * 60 * t) * envelope * 1.5f;

            data[i] = Mathf.Clamp((noise * 0.4f + punch * 0.4f + bass * 0.2f), -1f, 1f);
        }

        return CreateClip("Gunshot", data, sampleRate);
    }

    AudioClip GeneratePistolShot(float duration, float frequency, float dropoff)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * dropoff);

            // Higher pitched, snappier
            float noise = Random.Range(-1f, 1f) * envelope * 0.5f;
            float snap = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope;

            data[i] = Mathf.Clamp(noise + snap * 0.8f, -1f, 1f);
        }

        return CreateClip("PistolShot", data, sampleRate);
    }

    AudioClip GenerateReload(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;

            // Click sounds at different points
            float click1 = (progress > 0.1f && progress < 0.15f) ? Mathf.Sin(t * 3000) * 0.5f : 0;
            float click2 = (progress > 0.4f && progress < 0.45f) ? Mathf.Sin(t * 2000) * 0.4f : 0;
            float click3 = (progress > 0.7f && progress < 0.75f) ? Mathf.Sin(t * 4000) * 0.6f : 0;

            // Metal sliding sound
            float slide = 0;
            if (progress > 0.2f && progress < 0.6f)
            {
                float slideT = (progress - 0.2f) / 0.4f;
                slide = Random.Range(-0.1f, 0.1f) * (1 - slideT);
            }

            data[i] = Mathf.Clamp(click1 + click2 + click3 + slide, -1f, 1f);
        }

        return CreateClip("Reload", data, sampleRate);
    }

    AudioClip GenerateFootstep(float duration, float frequency)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 40);

            // Low thud with some noise
            float thud = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope;
            float noise = Random.Range(-1f, 1f) * envelope * 0.3f;

            data[i] = Mathf.Clamp(thud * 0.7f + noise, -1f, 1f);
        }

        return CreateClip("Footstep", data, sampleRate);
    }

    AudioClip GenerateJump(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 20);

            // Rising pitch
            float freq = 150 + t * 500;
            float swoosh = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.3f;
            float noise = Random.Range(-1f, 1f) * envelope * 0.2f;

            data[i] = Mathf.Clamp(swoosh + noise, -1f, 1f);
        }

        return CreateClip("Jump", data, sampleRate);
    }

    AudioClip GenerateLand(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 30);

            // Heavy thud
            float thud = Mathf.Sin(2 * Mathf.PI * 80 * t) * envelope;
            float impact = Mathf.Sin(2 * Mathf.PI * 40 * t) * envelope * 0.5f;
            float noise = Random.Range(-1f, 1f) * envelope * 0.4f;

            data[i] = Mathf.Clamp(thud + impact + noise, -1f, 1f);
        }

        return CreateClip("Land", data, sampleRate);
    }

    AudioClip GenerateDash(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float envelope = Mathf.Sin(progress * Mathf.PI);

            // Whoosh sound
            float whoosh = Random.Range(-1f, 1f) * envelope;
            float wind = Mathf.Sin(2 * Mathf.PI * (200 + progress * 400) * t) * envelope * 0.3f;

            data[i] = Mathf.Clamp(whoosh * 0.5f + wind, -1f, 1f);
        }

        return CreateClip("Dash", data, sampleRate);
    }

    AudioClip GenerateHitMarker(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 50);

            // Sharp ding
            float ding = Mathf.Sin(2 * Mathf.PI * 1200 * t) * envelope;
            float ding2 = Mathf.Sin(2 * Mathf.PI * 1800 * t) * envelope * 0.5f;

            data[i] = Mathf.Clamp(ding + ding2, -1f, 1f);
        }

        return CreateClip("HitMarker", data, sampleRate);
    }

    AudioClip GenerateHeadshot(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 30);

            // More satisfying ding
            float ding1 = Mathf.Sin(2 * Mathf.PI * 1500 * t) * envelope;
            float ding2 = Mathf.Sin(2 * Mathf.PI * 2000 * t) * envelope * 0.7f;
            float ding3 = Mathf.Sin(2 * Mathf.PI * 2500 * t) * envelope * 0.4f;

            data[i] = Mathf.Clamp(ding1 + ding2 + ding3, -1f, 1f);
        }

        return CreateClip("Headshot", data, sampleRate);
    }

    AudioClip GenerateDeath(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 5);

            // Low rumble
            float rumble = Mathf.Sin(2 * Mathf.PI * 50 * t) * envelope;
            float noise = Random.Range(-1f, 1f) * envelope * 0.3f;
            float drop = Mathf.Sin(2 * Mathf.PI * (200 - t * 300) * t) * envelope * 0.5f;

            data[i] = Mathf.Clamp(rumble + noise + drop, -1f, 1f);
        }

        return CreateClip("Death", data, sampleRate);
    }

    AudioClip GenerateSmokeThrow(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 15);

            // Swoosh
            float swoosh = Random.Range(-1f, 1f) * envelope * 0.4f;
            float wind = Mathf.Sin(2 * Mathf.PI * 300 * t) * envelope * 0.2f;

            data[i] = Mathf.Clamp(swoosh + wind, -1f, 1f);
        }

        return CreateClip("SmokeThrow", data, sampleRate);
    }

    AudioClip GenerateSmokeExplode(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 10);

            // Poof sound
            float poof = Random.Range(-1f, 1f) * envelope;
            float bass = Mathf.Sin(2 * Mathf.PI * 100 * t) * envelope * 0.5f;

            data[i] = Mathf.Clamp(poof * 0.6f + bass, -1f, 1f);
        }

        return CreateClip("SmokeExplode", data, sampleRate);
    }

    AudioClip GenerateKnifeSwing(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float envelope = Mathf.Sin(progress * Mathf.PI);

            // Swoosh
            float freq = 400 + progress * 600;
            float swoosh = Random.Range(-1f, 1f) * envelope * 0.5f;
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.2f;

            data[i] = Mathf.Clamp(swoosh + tone, -1f, 1f);
        }

        return CreateClip("KnifeSwing", data, sampleRate);
    }

    AudioClip GenerateKnifeHit(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 60);

            // Thunk
            float thunk = Mathf.Sin(2 * Mathf.PI * 200 * t) * envelope;
            float impact = Random.Range(-1f, 1f) * envelope * 0.5f;

            data[i] = Mathf.Clamp(thunk + impact, -1f, 1f);
        }

        return CreateClip("KnifeHit", data, sampleRate);
    }

    AudioClip GenerateUIClick(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 100);

            // Short click
            float click = Mathf.Sin(2 * Mathf.PI * 800 * t) * envelope;

            data[i] = Mathf.Clamp(click, -1f, 1f);
        }

        return CreateClip("UIClick", data, sampleRate);
    }

    AudioClip GenerateLowHealth(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;

            // Heartbeat
            float beat1 = (progress < 0.1f) ? Mathf.Sin(progress / 0.1f * Mathf.PI) : 0;
            float beat2 = (progress > 0.15f && progress < 0.25f) ? Mathf.Sin((progress - 0.15f) / 0.1f * Mathf.PI) * 0.7f : 0;

            float pulse = (beat1 + beat2) * Mathf.Sin(2 * Mathf.PI * 60 * t);

            data[i] = Mathf.Clamp(pulse, -1f, 1f);
        }

        return CreateClip("LowHealth", data, sampleRate);
    }

    #endregion

    AudioClip CreateClip(string name, float[] data, int sampleRate)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    #region Public Play Methods

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = volume * sfxVolume * masterVolume;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();
    }

    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * sfxVolume * masterVolume;
        source.spatialBlend = 1f; // 3D sound
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();

        Destroy(tempGO, clip.length + 0.1f);
    }

    // Convenience methods
    public void PlayGunshot() => PlaySound(gunshot, weaponVolume);
    public void PlayPistolShot() => PlaySound(pistolShot, weaponVolume);
    public void PlayReload() => PlaySound(reload, weaponVolume);
    public void PlayFootstep()
    {
        AudioClip[] steps = { footstep1, footstep2, footstep3 };
        PlaySound(steps[Random.Range(0, steps.Length)], footstepVolume);
    }
    public void PlayJump() => PlaySound(jump, sfxVolume);
    public void PlayLand() => PlaySound(land, sfxVolume);
    public void PlayDash() => PlaySound(dash, sfxVolume);
    public void PlayHitMarker() => PlaySound(hitMarker, sfxVolume);
    public void PlayHeadshot() => PlaySound(headshot, sfxVolume);
    public void PlayDeath() => PlaySound(death, sfxVolume);
    public void PlaySmokeThrow() => PlaySound(smokeThrow, sfxVolume);
    public void PlaySmokeExplode() => PlaySound(smokeExplode, sfxVolume);
    public void PlayKnifeSwing() => PlaySound(knifeSwing, sfxVolume);
    public void PlayKnifeHit() => PlaySound(knifeHit, sfxVolume);
    public void PlayUIClick() => PlaySound(uiClick, sfxVolume * 0.5f);

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public float GetMasterVolume() => masterVolume;
    public float GetSFXVolume() => sfxVolume;

    #endregion
}