using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Effect Clips")]
    [SerializeField] private AudioClip placeObjectClip;
    [SerializeField] private AudioClip scaleGridClip;
    [SerializeField] private AudioClip balanceGoneClip;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    private const string SfxVolumeKey = "SFXVolume";
    private const string MusicVolumeKey = "MusicVolume";

    public float SfxVolume
    {
        get => sfxVolume;
        set => SetSfxVolume(value);
    }

    public float MusicVolume
    {
        get => musicVolume;
        set => SetMusicVolume(value);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveAudioSources();
        LoadVolumeSettings();
    }

    private void Start()
    {
        ApplyMusicVolume();

        if (playMusicOnStart)
            PlayMusic();
    }

    private void OnValidate()
    {
        ApplyMusicVolume();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey(SfxVolumeKey))
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);

        if (PlayerPrefs.HasKey(MusicVolumeKey))
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume);
    }

    public void PlayPlaceObjectSound()
    {
        PlaySfx(placeObjectClip);
    }

    public void PlayScaleGridSound()
    {
        PlaySfx(scaleGridClip);
    }

    public void PlayBalanceGoneSound()
    {
        PlaySfx(balanceGoneClip);
    }

    public void PlayMusic()
    {
        if (musicClip == null || musicSource == null)
            return;

        if (musicSource.clip != musicClip)
            musicSource.clip = musicClip;

        musicSource.loop = true;
        ApplyMusicVolume();

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    private void ResolveAudioSources()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();

        if (sfxSource == null && audioSources.Length > 0)
            sfxSource = audioSources[0];

        if (musicSource == null && audioSources.Length > 1)
            musicSource = audioSources[1];

        if (sfxSource != null)
            sfxSource.playOnAwake = false;

        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
