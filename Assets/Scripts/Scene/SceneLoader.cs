using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private float _fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadWithTransition(sceneName));
    }

    private IEnumerator LoadWithTransition(string sceneName)
    {
        // 1. Load loading screen
        Scene previousScene = SceneManager.GetActiveScene();
        yield return SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);

        // 2. Get the transition controller from it
        LoadingScreen loadingScreen = FindFirstObjectByType<LoadingScreen>();
        if (loadingScreen == null)
        {
            Debug.LogError("LoadingScreen not found. Make sure the 'Loading' scene is added to the Build Settings/Profile.");
            yield break;
        }
        yield return loadingScreen.FadeIn(_fadeDuration);

        // 3. Load target scene additively so the Loading scene stays alive for FadeOut
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f) yield return null;

        op.allowSceneActivation = true;
        yield return op;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        yield return SceneManager.UnloadSceneAsync(previousScene);

        // 4. Fade out then unload loading scene
        yield return loadingScreen.FadeOut(_fadeDuration);
        SceneManager.UnloadSceneAsync("Loading");
    }
}
