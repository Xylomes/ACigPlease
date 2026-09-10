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

    public IEnumerator LoadScene(string pSceneName)
    {
        AsyncOperation lAsycOP = SceneManager.LoadSceneAsync(pSceneName, LoadSceneMode.Single);
        loadedScenes.Add(pSceneName);
        yield return lAsycOP;
    }
    public IEnumerator AddScene(string pSceneName)
    {
        AsyncOperation lAsycOP = SceneManager.LoadSceneAsync(pSceneName, LoadSceneMode.Additive);
        loadedScenes.Add(pSceneName);
        yield return lAsycOP;
    }

    public IEnumerator UnloadScene(string pSceneName)
    {
        AsyncOperation lAsycOP = SceneManager.UnloadSceneAsync(pSceneName);
        loadedScenes.Remove(pSceneName);
        yield return lAsycOP;
    }

}
