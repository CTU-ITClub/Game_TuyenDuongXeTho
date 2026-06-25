using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("UI")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;

    // PlayerPrefs keys
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string MUSIC_MUTE_KEY = "MusicMute";
    private const string SFX_MUTE_KEY = "SFXMute";

    // Volume range (dB)
    private const float MIN_DB = -70f;
    private const float MUSIC_MAX_DB = -10f;
    private const float SFX_MAX_DB = 0f;

    private void Awake()
    {
        // Giá trị mặc định lần đầu chơi
        if (!PlayerPrefs.HasKey(MUSIC_MUTE_KEY))
            PlayerPrefs.SetInt(MUSIC_MUTE_KEY, 0);

        if (!PlayerPrefs.HasKey(SFX_MUTE_KEY))
            PlayerPrefs.SetInt(SFX_MUTE_KEY, 0);

        if (!PlayerPrefs.HasKey(MUSIC_KEY))
            PlayerPrefs.SetFloat(MUSIC_KEY, 0.85f);

        if (!PlayerPrefs.HasKey(SFX_KEY))
            PlayerPrefs.SetFloat(SFX_KEY, 0.85f);

        PlayerPrefs.Save();
    }

    private void Start()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 0.5f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_KEY, 0.5f);

        bool musicMute = PlayerPrefs.GetInt(MUSIC_MUTE_KEY, 0) == 1;
        bool sfxMute = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;

        // Set UI
        if (musicSlider != null)
            musicSlider.value = musicVol;

        if (sfxSlider != null)
            sfxSlider.value = sfxVol;

        if (musicToggle != null)
            musicToggle.SetIsOnWithoutNotify(!musicMute);

        if (sfxToggle != null)
            sfxToggle.SetIsOnWithoutNotify(!sfxMute);

        ApplyMusic();
        ApplySFX();
    }

    // ================= MUSIC =================

    public void ToggleMusic()
    {
        if (musicToggle == null)
            return;

        PlayerPrefs.SetInt(MUSIC_MUTE_KEY, musicToggle.isOn ? 0 : 1);
        PlayerPrefs.Save();

        ApplyMusic();
    }

    public void SetMusic()
    {
        if (musicSlider != null)
        {
            PlayerPrefs.SetFloat(MUSIC_KEY, musicSlider.value);
            PlayerPrefs.Save();
        }

        ApplyMusic();
    }

    private void ApplyMusic()
    {
        bool isMusicOn = musicToggle == null || musicToggle.isOn;

        if (musicSlider != null)
            musicSlider.interactable = isMusicOn;

        if (!isMusicOn)
        {
            audioMixer.SetFloat("MusicVolume", -80f);
            return;
        }

        float volume = musicSlider != null
            ? musicSlider.value
            : PlayerPrefs.GetFloat(MUSIC_KEY, 0.5f);

        float db = Mathf.Lerp(MIN_DB, MUSIC_MAX_DB, volume);
        audioMixer.SetFloat("MusicVolume", db);
    }

    // ================= SFX =================

    public void ToggleSFX()
    {
        if (sfxToggle == null)
            return;

        PlayerPrefs.SetInt(SFX_MUTE_KEY, sfxToggle.isOn ? 0 : 1);
        PlayerPrefs.Save();

        ApplySFX();
    }

    public void SetSFX()
    {
        if (sfxSlider != null)
        {
            PlayerPrefs.SetFloat(SFX_KEY, sfxSlider.value);
            PlayerPrefs.Save();
        }

        ApplySFX();
    }

    private void ApplySFX()
    {
        bool isSFXOn = sfxToggle == null || sfxToggle.isOn;

        if (sfxSlider != null)
            sfxSlider.interactable = isSFXOn;

        if (!isSFXOn)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
            return;
        }

        float volume = sfxSlider != null
            ? sfxSlider.value
            : PlayerPrefs.GetFloat(SFX_KEY, 0.5f);

        float db = Mathf.Lerp(MIN_DB, SFX_MAX_DB, volume);
        audioMixer.SetFloat("SFXVolume", db);
    }
}