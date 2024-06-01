using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : Singleton<LoadingScreen>
{
    [SerializeField] Image _loadingBar;
    [SerializeField] string _startSceen;
    [SerializeField] float _loadDelay;
    [SerializeField] float _delayPercentage = 0.75f;
    protected override void SingletonAwakened()
    {
        base.SingletonAwakened();
        this.AddEvent(EventKey.ON_FINISH_LOADING_SCREEN, OnFinishLoadingScreen);
        this.AddEvent(EventKey.ON_FINISH_LOADING_SCREEN, OnFinishLoadingScreenO);
    }
    protected override void SingletonStarted()
    {
        base.SingletonStarted();
        StartLoad(_startSceen);
    }
    protected override void SingletonDestroyed()
    {
        base.SingletonDestroyed();
        this.DropEvent(EventKey.ON_FINISH_LOADING_SCREEN, OnFinishLoadingScreen);
        this.DropEvent(EventKey.ON_FINISH_LOADING_SCREEN, OnFinishLoadingScreenO);
    }
    public void StartLoad(string targetLevel)
    {
        gameObject.SetActive(true);
        StartCoroutine(LoadLevel(targetLevel));
    }
    IEnumerator LoadLevel(string targetLevel)
    {
        float timeSinceLoad = 0;
        float sceenLoadPercentage = 1 - _delayPercentage;
        while (timeSinceLoad < _loadDelay)
        {
            _loadingBar.fillAmount = Mathf.Clamp01(timeSinceLoad / _loadDelay) * _delayPercentage;
            timeSinceLoad += Time.deltaTime;
            yield return Yielder.WaitForEndOfFrame;
        }
        //Start doing load
        AsyncOperation loadSceen = SceneManager.LoadSceneAsync(targetLevel, LoadSceneMode.Single);
        while (!loadSceen.isDone)
        {
            _loadingBar.fillAmount = _delayPercentage + Mathf.Clamp01(loadSceen.progress / .9f) * sceenLoadPercentage;
            yield return Yielder.WaitForEndOfFrame;
        }
        yield return Yielder.WaitForSeconds(0.1f);
        gameObject.SetActive(false);
        this.DispatchEvent(EventKey.ON_FINISH_LOADING_SCREEN);
        this.DispatchEvent(EventKey.ON_FINISH_LOADING_SCREEN, 1000);
    }
    public void OnFinishLoadingScreen()
    {
        Debug.Log("Finish Loading");
    }
    public void OnFinishLoadingScreenO(object o)
    {
        Debug.Log($"Finish Loading {o}");
    }
}