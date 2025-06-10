using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndSceneManager : MonoBehaviour
{
    [Header("视频设置")]
    [SerializeField] private VideoPlayer endFilm; // 结束视频播放器
    [SerializeField] private AudioSource endSound; // 结束背景音乐（独立于视频）

    [Header("UI元素")]
    [SerializeField] private Image endBG; // 结束背景图片
    [SerializeField] private Text victoryText; // 胜利文本
    [SerializeField] private Text statsText; // 统计信息文本
    [SerializeField] private Button backButton; // 返回按钮

    [Header("过渡设置")]
    [SerializeField] private float fadeDuration = 1.0f; // 淡入淡出时间
    [SerializeField] private float delayBetweenElements = 0.5f; // UI元素之间的显示延迟

    private void Start()
    {
        // 初始化时隐藏所有UI元素
        SetInitialUIState();

        // 播放视频和音乐（音乐会继续播放直到自然结束）
        PlayMedia();

        // 视频播放完成后的事件
        endFilm.loopPointReached += OnVideoFinished;

        // 设置按钮点击事件
        backButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void SetInitialUIState()
    {
        statsText.text = Extensions.distance.ToString();
        // 设置所有UI元素为透明
        SetAlpha(endBG, 0f);
        SetAlpha(victoryText, 0f);
        SetAlpha(statsText, 0f);

        // 隐藏按钮
        backButton.gameObject.SetActive(false);
    }

    private void PlayMedia()
    {
        // 播放视频（不循环）
        endFilm.playOnAwake = false;
        endFilm.isLooping = false;
        endFilm.Play();

        // 播放音乐（不循环）
        endSound.playOnAwake = false;
        endSound.loop = false;
        endSound.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // 视频播放完成后开始UI显示流程
        // 注意：音乐继续播放不受影响
        StartCoroutine(ShowUIElements());
    }

    private System.Collections.IEnumerator ShowUIElements()
    {
        // 显示背景图片
        yield return StartCoroutine(FadeInUI(endBG));

        // 等待一段时间
        yield return new WaitForSeconds(delayBetweenElements);

        // 显示胜利文本
        yield return StartCoroutine(FadeInUI(victoryText));

        // 等待一段时间
        yield return new WaitForSeconds(delayBetweenElements);

        // 显示统计信息文本
        yield return StartCoroutine(FadeInUI(statsText));

        // 等待一段时间
        yield return new WaitForSeconds(delayBetweenElements);

        // 显示返回按钮
        backButton.gameObject.SetActive(true);
        yield return StartCoroutine(FadeInUI(backButton.GetComponent<Image>()));
    }

    private System.Collections.IEnumerator FadeInUI(Graphic uiElement)
    {
        float elapsedTime = 0f;
        Color color = uiElement.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            uiElement.color = color;
            yield return null;
        }

        color.a = 1f;
        uiElement.color = color;
    }

    private void SetAlpha(Graphic uiElement, float alpha)
    {
        Color color = uiElement.color;
        color.a = alpha;
        uiElement.color = color;
    }

    public void ReturnToMainMenu()
    {
        // 停止音乐（如果还在播放）
        if (endSound.isPlaying)
        {
            endSound.Stop();
        }
        SceneManager.LoadScene("StartScene");
    }

    private void OnDestroy()
    {
        // 清理事件监听
        if (endFilm != null)
        {
            endFilm.loopPointReached -= OnVideoFinished;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(ReturnToMainMenu);
        }
    }
}