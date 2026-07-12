using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI completionRateText;
    public Button[] stageButtons;
    public GameObject[] completedBadges;

    void Start()
    {
        string name = PlayerPrefs.GetString("PlayerName", "Player");
        playerNameText.text = "Welcome, " + name;

        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
        if (playerId == -1) return;

        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stage = i + 1;
            int best = DatabaseManager.Instance.GetHighScore(playerId, stage);

            TextMeshProUGUI label = stageButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (best > 0)
                label.text = "Stage " + stage + "\nBest: " + best;
            else
                label.text = "Stage " + stage;

            if (completedBadges != null && i < completedBadges.Length && completedBadges[i] != null)
            {
                bool completed = DatabaseManager.Instance.IsStageCompleted(playerId, stage);
                completedBadges[i].SetActive(completed);
            }
        }

        if (completionRateText != null)
        {
            float rate = DatabaseManager.Instance.GetCompletionRate(playerId, stageButtons.Length);
            completionRateText.text = "Completion Rate: " + rate.ToString("0.0") + "%";
        }
    }

    public void OnStageSelected(int stage) => SceneLoader.GoToGameplay(stage);
    public void OnBackPressed() => SceneLoader.GoToMainMenu();
}