using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Sahne Ayarları")]
    [Tooltip("Yüklenecek sahnenin build index değeri.")]
    public int sceneIndex = 1;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    public void StartGame()
    {
        StartCoroutine(StartAnim());
    }

    public void StartUIAnimation()
    {
        animator.enabled = true;
    }

    private IEnumerator StartAnim()
    {
        StartUIAnimation();
        
        yield return null;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
        LoadLevel(sceneIndex);
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
