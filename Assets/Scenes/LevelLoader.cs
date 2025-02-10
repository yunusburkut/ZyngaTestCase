using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void StartGame()
    {
        StartCoroutine(StartAnim());
    }

    private IEnumerator StartAnim()
    {
        
        yield return null;
    }
    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
        
        asyncOp.allowSceneActivation = false;
        
        while (!asyncOp.isDone)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            Debug.Log("Loading progress: " + (progress * 100f).ToString("F0") + "%");

            if (asyncOp.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f);
                asyncOp.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
