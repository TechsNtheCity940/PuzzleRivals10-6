using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ModeSelectController : MonoBehaviour
{
    [Header("Loading Screen View")]
    public GameObject loadingScreenRoot;
    public Slider loadingProgressBar;
    public float loadingDuration = 2.0f;

    [Header("Mode Menu View")]
    public CanvasGroup modeMenuRoot;
    public Button ffaButton;
    public Button brainVsBrainButton;
    public Button profileButton;
    public Button storeButton;
    public Button quitButton;

    [Header("Validation Popup Dialog")]
    public GameObject validationPopupRoot;
    public Button popupGoToProfileButton;
    public Button popupCloseButton;
    public TMP_Text popupMessageText;

    [Header("Audio")]
    public AudioClip clickSfx;
    public AudioClip hoverSfx;
    public AudioClip errorSfx;

    public string matchSceneName = "Match";
    public string mainMenuSceneName = "Main";
    public string profileSceneName = "Profile";
    public string storeSceneName = "Store";

    private bool isLoading = true;

    private void Start()
    {
        // Set initial UI states
        if (loadingScreenRoot != null) loadingScreenRoot.SetActive(true);
        if (loadingProgressBar != null) loadingProgressBar.value = 0f;

        if (modeMenuRoot != null)
        {
            modeMenuRoot.alpha = 0f;
            modeMenuRoot.interactable = false;
            modeMenuRoot.blocksRaycasts = false;
        }

        if (validationPopupRoot != null)
        {
            validationPopupRoot.SetActive(false);
        }

        // Setup Button Listeners
        SetupButton(ffaButton, () => TryStartMatch(GameState.Mode.FFA));
        SetupButton(brainVsBrainButton, () => TryStartMatch(GameState.Mode.BVB));
        SetupButton(profileButton, () => LoadScene(profileSceneName));
        SetupButton(storeButton, () => LoadScene(storeSceneName));
        SetupButton(quitButton, BackToMenu);

        // Setup Popup Listeners
        if (popupGoToProfileButton != null)
        {
            popupGoToProfileButton.onClick.AddListener(() =>
            {
                PlayClickSound();
                LoadScene(profileSceneName);
            });
        }
        if (popupCloseButton != null)
        {
            popupCloseButton.onClick.AddListener(() =>
            {
                PlayClickSound();
                if (validationPopupRoot != null) validationPopupRoot.SetActive(false);
            });
        }

        // Play menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        // Start loading sequence
        StartCoroutine(LoadingSequenceRoutine());
    }

    private void SetupButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        if (button == null) return;
        button.onClick.AddListener(() =>
        {
            PlayClickSound();
            callback();
        });

        // Add hover triggers
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entry);
    }

    private IEnumerator LoadingSequenceRoutine()
    {
        isLoading = true;
        float elapsed = 0f;

        while (elapsed < loadingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / loadingDuration);
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
            }
            yield return null;
        }

        // Transition out loading screen
        if (loadingScreenRoot != null)
        {
            loadingScreenRoot.SetActive(false);
        }

        // Fade in menu
        float fadeDuration = 0.5f;
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            if (modeMenuRoot != null)
            {
                modeMenuRoot.alpha = alpha;
            }
            yield return null;
        }

        if (modeMenuRoot != null)
        {
            modeMenuRoot.alpha = 1f;
            modeMenuRoot.interactable = true;
            modeMenuRoot.blocksRaycasts = true;
        }

        isLoading = false;
    }

    private void TryStartMatch(GameState.Mode mode)
    {
        if (isLoading) return;

        // PROFILE VALIDATION CHECK
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (p == null || p.playerName == "New Rival" || string.IsNullOrWhiteSpace(p.playerName))
        {
            // Block player and show popup
            PlayErrorSound();
            if (validationPopupRoot != null)
            {
                validationPopupRoot.SetActive(true);
                if (popupMessageText != null)
                {
                    popupMessageText.text = "PROFILE INCOMPLETE!\n\nBefore entering matchmaking, you must set up a CUSTOM USERNAME and select an AVATAR on your profile!";
                }
            }
            return;
        }

        // Normal flow
        if (GameState.Instance != null)
        {
            GameState.Instance.currentMode = mode;
        }

        LoadScene(matchSceneName);
    }

    private void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void BackToMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    private void PlayClickSound()
    {
        if (clickSfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(clickSfx, 0.8f);
        }
    }

    private void PlayHoverSound()
    {
        if (hoverSfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(hoverSfx, 0.5f);
        }
    }

    private void PlayErrorSound()
    {
        if (errorSfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(errorSfx, 0.9f);
        }
    }
}
