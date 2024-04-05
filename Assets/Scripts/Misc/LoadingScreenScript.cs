using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class LoadingScreenScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _loadingText;
    private AsyncOperation _loadingNext;
    private bool _break = false;
    private EasyTimer _textFade;
    private bool _loadingDone = false;

    // Start is called before the first frame update
    void Start()
    {
        loadNext();
        _textFade = new EasyTimer(0.5f, false);
        GameManager.Instance.InLoadingScreen = false;
    }

    void Update()
    {
        if (_break) return;

        if (_loadingNext.isDone && !_loadingDone)
        {
            _loadingDone = true;
            foreach (var gObject in UnitySceneManager.GetActiveScene().GetRootGameObjects())
            {
                gObject.SetActive(false);
            }
            _loadingText.gameObject.SetActive(true);
            _textFade.Reset();
            GameManager.Instance.UnloadLastScene();
        }

        if (_loadingDone && !_textFade.Done)
        {
            _loadingText.color = new Color(_loadingText.color.r, _loadingText.color.g, _loadingText.color.b, 1 - GameManager.Instance.UnloadProgress);
        }

        if (_loadingDone && _textFade.Done)
            _break = true;
    }

    //private IEnumerator loadNext()
    //{
    //    _loadingNext = UnitySceneManager.LoadSceneAsync(GameManager.Instance.NextScene, LoadSceneMode.Additive);
    //    while (!_loadingNext.isDone)
    //    {
    //        yield return null;
    //    }
    //    Scene nextScene = UnitySceneManager.GetSceneByName(GameManager.Instance.NextScene);
    //    var result = UnitySceneManager.SetActiveScene(nextScene);
    //}

    private void loadNext()
    {
        _loadingNext = UnitySceneManager.LoadSceneAsync(GameManager.Instance.NextScene);
    }
}
