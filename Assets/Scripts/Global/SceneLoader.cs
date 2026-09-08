using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject sceneHolder;

    private const string MAIN_MENU_SCENE_NAME = "MainMenu";

    private List<string> loadedScenes = new List<string>();

    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        StartCoroutine(AddScene(MAIN_MENU_SCENE_NAME));
    }

    public IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation AsycOP = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        loadedScenes.Add(sceneName);
        yield return AsycOP;
    }
    public IEnumerator AddScene(string sceneName)
    {
        AsyncOperation AsycOP = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadedScenes.Add(sceneName);
        yield return AsycOP;
    }

    public IEnumerator UnloadScene(string sceneName)
    {
        AsyncOperation AsycOP = SceneManager.UnloadSceneAsync(sceneName);
        loadedScenes.Remove(sceneName);
        yield return AsycOP;
    }

}
