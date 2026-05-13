using UnityEngine;
using TMPro;

public class ResultScreenUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // ดึงคะแนนที่ส่งมาจาก Gameplay
        int score = PlayerPrefs.GetInt("LastScore", 0);
        int stage = PlayerPrefs.GetInt("LastStage", 1);
        scoreText.text = $"Stage {stage}\nScore: {score}";
    }

    public void OnReplayPressed() => SceneLoader.ReplayCurrentStage();
    public void OnStageSelectPressed() => SceneLoader.GoToStageSelect();
}