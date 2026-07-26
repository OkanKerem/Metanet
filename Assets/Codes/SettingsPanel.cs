using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Controls")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button closeButton;


    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private SoundManager soundManager;
    private bool isUpdatingSliders;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        soundManager = SoundManager.Instance;

        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();

    

        if (panelRoot != null)
            panelRoot.SetActive(false);

        WireControls();
    }

    private void Start()
    {
        SyncSlidersFromSoundManager();
    }

    public void OpenPanel()
    {
        if (panelRoot == null)
            return;

        previousTimeScale = Time.timeScale;
        panelRoot.SetActive(true);
        SyncSlidersFromSoundManager();
        Time.timeScale = 0f;
    }

    public void ClosePanel()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(false);
        Time.timeScale = previousTimeScale;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           OpenPanel();
        }
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void WireControls()
    {


        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
    }

    private void SyncSlidersFromSoundManager()
    {
        if (soundManager == null)
            return;

        isUpdatingSliders = true;

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(soundManager.SfxVolume);

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(soundManager.MusicVolume);

        isUpdatingSliders = false;
    }

    private void OnSfxSliderChanged(float value)
    {
        if (isUpdatingSliders || soundManager == null)
            return;

        soundManager.SetSfxVolume(value);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (isUpdatingSliders || soundManager == null)
            return;

        soundManager.SetMusicVolume(value);
    }

    private void OnDestroy()
    {
      
 

        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(GoToMainMenu);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
    }

}