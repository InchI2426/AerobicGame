using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static void GoToMainMenu()
        => SceneManager.LoadScene("MainMenu");

    public static void GoToStageSelect()
        => SceneManager.LoadScene("StageSelect");

    public static void GoToGameplay(int stage)
    {
        PlayerPrefs.SetInt("SelectedStage", stage);
        SceneManager.LoadScene("Gameplay");
    }

    public static void GoToResult(int score, int stage)
    {
        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetInt("LastStage", stage);
        SceneManager.LoadScene("ResultScreen");
    }

    public static void ReplayCurrentStage()
    {
        int stage = PlayerPrefs.GetInt("LastStage", 1);
        GoToGameplay(stage);
    }
}