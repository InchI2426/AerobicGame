using UnityEngine;

public class StageSelectUI : MonoBehaviour
{
    public void OnStageSelected(int stage)
        => SceneLoader.GoToGameplay(stage);

    public void OnBackPressed()
        => SceneLoader.GoToMainMenu();
}