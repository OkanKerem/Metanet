using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text roundText;

    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GameObject comboGameobject;
    [SerializeField] private GameObject gameLostPanel;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Points")]
    [SerializeField] private int[] pointsByValue = new int[5];

    [Header("Combo Animation")]
    [SerializeField] private float comboPopScale = 1.25f;
    [SerializeField] private float comboPopDuration = 0.2f;
    [SerializeField] private float comboFadeDuration = 0.35f;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private int totalScore;
    private int combo;

    private CanvasGroup comboCanvasGroup;
    private Vector3 comboOriginalScale;
    private Coroutine comboAnimationCoroutine;

    public int TotalScore => totalScore;
    public int Combo => combo;

    private void Awake()
    {
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(Retry);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(MainMenu);

        if (comboGameobject != null)
        {
            comboCanvasGroup = comboGameobject.GetComponent<CanvasGroup>();

            if (comboCanvasGroup == null)
                comboCanvasGroup = comboGameobject.AddComponent<CanvasGroup>();

            comboOriginalScale = comboGameobject.transform.localScale;
            comboCanvasGroup.alpha = 0f;
            comboGameobject.SetActive(false);
        }

        if (gameLostPanel != null)
            gameLostPanel.SetActive(false);

        UpdateUI();
    }

    public void RegisterPlacement(BalanceObject placedObject)
    {
        if (placedObject == null)
            return;

        int previousCombo = combo;

        UpdateCombo(placedObject.Weight);

        int weightValue = GetPointsByWeight(placedObject.Weight);
        int pointsEarned = weightValue * (combo + 1);

        totalScore += pointsEarned;

        UpdateUI();
        AnimateCombo(previousCombo);
    }

    public void ResetScore()
    {
        totalScore = 0;
        combo = 0;

        UpdateUI();

        if (comboGameobject != null)
        {
            if (comboAnimationCoroutine != null)
                StopCoroutine(comboAnimationCoroutine);

            comboGameobject.transform.localScale = comboOriginalScale;
            comboCanvasGroup.alpha = 0f;
            comboGameobject.SetActive(false);
        }
    }

    private void UpdateCombo(Weight weight)
    {
        if (weight == Weight.Heavy || weight == Weight.VeryHeavy)
            combo++;
        else
            combo = 0;
    }

    private void AnimateCombo(int previousCombo)
    {
        if (comboGameobject == null || comboCanvasGroup == null)
            return;

        if (comboAnimationCoroutine != null)
            StopCoroutine(comboAnimationCoroutine);

        if (combo > previousCombo)
            comboAnimationCoroutine = StartCoroutine(ComboPopAnimation());
        else if (previousCombo > 0 && combo == 0)
            comboAnimationCoroutine = StartCoroutine(ComboFadeAnimation());
    }

    private IEnumerator ComboPopAnimation()
    {
        comboGameobject.SetActive(true);
        comboCanvasGroup.alpha = 1f;

        Vector3 popScale = comboOriginalScale * comboPopScale;
        float elapsedTime = 0f;

        while (elapsedTime < comboPopDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / (comboPopDuration * 0.5f));
            t = EaseOutBack(t);

            comboGameobject.transform.localScale = Vector3.LerpUnclamped(comboOriginalScale, popScale, t);

            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < comboPopDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / (comboPopDuration * 0.5f));
            t = t * t * (3f - 2f * t);

            comboGameobject.transform.localScale = Vector3.Lerp(popScale, comboOriginalScale, t);

            yield return null;
        }

        comboGameobject.transform.localScale = comboOriginalScale;
        comboAnimationCoroutine = null;
    }

    private IEnumerator ComboFadeAnimation()
    {
        float startAlpha = comboCanvasGroup.alpha;
        Vector3 startScale = comboGameobject.transform.localScale;
        Vector3 targetScale = comboOriginalScale * 0.9f;

        float elapsedTime = 0f;

        while (elapsedTime < comboFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / comboFadeDuration);
            t = t * t * (3f - 2f * t);

            comboCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            comboGameobject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        comboCanvasGroup.alpha = 0f;
        comboGameobject.transform.localScale = comboOriginalScale;
        comboGameobject.SetActive(false);

        comboAnimationCoroutine = null;
    }

    private float EaseOutBack(float t)
    {
        const float overshoot = 1.70158f;

        t -= 1f;

        return 1f + (overshoot + 1f) * t * t * t + overshoot * t * t;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"{totalScore}";

        if (comboText != null)
            comboText.text = combo > 0 ? $"x{combo}" : string.Empty;

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Score: {totalScore}";

        if (turnManager != null && roundText != null)
            roundText.text = $"Round: {turnManager.roundNumber}";
    }

    private int GetPointsByWeight(Weight weight)
    {
        if (pointsByValue == null || pointsByValue.Length < 5)
        {
            Debug.LogError("Points By Value dizisi en az 5 eleman içermeli.", this);
            return 0;
        }

        switch (weight)
        {
            case Weight.VeryLight:
                return pointsByValue[0];

            case Weight.Light:
                return pointsByValue[1];

            case Weight.Medium:
                return pointsByValue[2];

            case Weight.Heavy:
                return pointsByValue[3];

            case Weight.VeryHeavy:
                return pointsByValue[4];

            default:
                return 0;
        }
    }

    public void GameLost()
    {
        if (gameLostPanel != null)
            gameLostPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        if (tryAgainButton != null)
            tryAgainButton.onClick.RemoveListener(Retry);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(MainMenu);
    }
}