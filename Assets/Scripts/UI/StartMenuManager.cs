using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("黑场过渡持续时间")]
    public float blackFadeDuration = 1.0f;

    [Tooltip("Logo淡入持续时间")]
    public float logoFadeDuration = 1.5f;

    [Tooltip("Play按钮淡入持续时间")]
    public float buttonFadeDuration = 1.0f;

    [Tooltip("Logo淡入延迟时间")]
    public float logoDelay = 0.5f;

    [Tooltip("Play按钮淡入延迟时间")]
    public float buttonDelay = 1.0f;

    [Header("UI References")]
    public Image backgroundImage;
    public Image logoImage;
    public Button playButton;

    private CanvasGroup backgroundCanvasGroup;
    private CanvasGroup logoCanvasGroup;
    private CanvasGroup buttonCanvasGroup;

    private void Awake()
    {
        // 确保所有元素都有CanvasGroup组件
        backgroundCanvasGroup = backgroundImage.GetComponent<CanvasGroup>();
        if (backgroundCanvasGroup == null)
            backgroundCanvasGroup = backgroundImage.gameObject.AddComponent<CanvasGroup>();

        logoCanvasGroup = logoImage.GetComponent<CanvasGroup>();
        if (logoCanvasGroup == null)
            logoCanvasGroup = logoImage.gameObject.AddComponent<CanvasGroup>();

        buttonCanvasGroup = playButton.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null)
            buttonCanvasGroup = playButton.gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        // 初始状态设置
        backgroundCanvasGroup.alpha = 0f;
        logoCanvasGroup.alpha = 0f;
        buttonCanvasGroup.alpha = 0f;
        playButton.interactable = false;

        // 开始过渡序列
        StartCoroutine(TransitionSequence());
    }

    private System.Collections.IEnumerator TransitionSequence()
    {
        // 背景淡入（从黑场过渡）
        float timer = 0f;
        while (timer < blackFadeDuration)
        {
            timer += Time.deltaTime;
            backgroundCanvasGroup.alpha = timer / blackFadeDuration;
            yield return null;
        }
        backgroundCanvasGroup.alpha = 1f;

        // 等待Logo延迟
        yield return new WaitForSeconds(logoDelay);

        // Logo淡入
        timer = 0f;
        while (timer < logoFadeDuration)
        {
            timer += Time.deltaTime;
            logoCanvasGroup.alpha = timer / logoFadeDuration;
            yield return null;
        }
        logoCanvasGroup.alpha = 1f;

        // 等待按钮延迟
        yield return new WaitForSeconds(buttonDelay);

        // 按钮淡入
        timer = 0f;
        while (timer < buttonFadeDuration)
        {
            timer += Time.deltaTime;
            buttonCanvasGroup.alpha = timer / buttonFadeDuration;
            yield return null;
        }
        buttonCanvasGroup.alpha = 1f;
        playButton.interactable = true;
    }

    // 按钮点击事件
    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("GameScene");
    }
}