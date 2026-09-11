using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playBTN;
    [SerializeField] private Button leaveBTN;

    private const string INTRO_SCENE_NAME = "Intro";
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";

    public void OnPlayClicked()
    {
        SceneLoader.Instance.StartCoroutine(SceneLoader.Instance.AddScene(INTRO_SCENE_NAME));
        SceneLoader.Instance.StartCoroutine(SceneLoader.Instance.UnloadScene(MAIN_MENU_SCENE_NAME));
    }
    public void OnLeaveClicked()
    {
        Application.Quit();
    }
}
