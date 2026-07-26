using UnityEngine;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private Button quitButton;
    void Awake()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }
    void QuitGame()
    {
        Application.Quit();
    }
}
