using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _gameWinUI;
    [SerializeField] private GameObject _pauseUI;

    private bool _isGameOver = false;
    private bool _isGameWin = false;
    private bool _isPaused = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PauseGame()
    {
        if (_pauseUI == null) return;
        if (_isGameOver || _isGameWin) return;

        _isPaused = !_isPaused;

        if (_isPaused)
        {
            Time.timeScale = 0f;
            _pauseUI.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            _pauseUI.SetActive(false);
        }
    }

    // 플레이어 사망 시 호출
    public void GameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        Invoke(nameof(ShowGameOverUI), 3f);
    }

    private void ShowGameOverUI()
    {
        Time.timeScale = 0f;

        if (_gameOverUI != null)
            _gameOverUI.SetActive(true);
    }


    // GoblinBaseManager 에서 건물 다 파괴시 호출
    public void GameWin()
    {
        if (_isGameWin) return;
        _isGameWin = true;

        Invoke(nameof(ShowGameWinUI), 3f);
    }

    private void ShowGameWinUI()
    {
        Time.timeScale = 0f;

        if (_gameWinUI != null)
            _gameWinUI.SetActive(true);
    }

    // 게임 재시작
    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    /// 타이틀 씬으로 이동
    public void SceneChange(int sceneNum)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneNum);
    }

    /// 게임 완전 종료
    public void ExitGame()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}