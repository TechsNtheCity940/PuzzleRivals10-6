using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Start Screen View")]
    public GameObject startScreenRoot;
    public Button startScreenTriggerButton;

    [Header("Explosion Effect")]
    public Image explosionImage;
    public AudioClip explosionSfx;

    [Header("Main Menu View")]
    public CanvasGroup mainMenuRoot;
    public Button btnPlay;
    public Button btnQuit;
    public Button btnProfile;
    public Button btnStore;
    public Button btnSeasonPass;

    [Header("Audio")]
    public AudioClip clickSfx;
    public AudioClip hoverSfx;

    private bool isTransitioning = false;

    private void Start()
    {
        // Initialize state
        if (startScreenRoot != null) startScreenRoot.SetActive(true);
        if (explosionImage != null)
        {
            explosionImage.gameObject.SetActive(false);
            explosionImage.transform.localScale = Vector3.zero;
        }
        if (mainMenuRoot != null)
        {
            mainMenuRoot.alpha = 0f;
            mainMenuRoot.interactable = false;
            mainMenuRoot.blocksRaycasts = false;
        }

        // Setup triggers
        if (startScreenTriggerButton != null)
        {
            startScreenTriggerButton.onClick.AddListener(TriggerExplosionTransition);
        }

        // Setup main menu buttons
        SetupButton(btnPlay, PlayGame);
        SetupButton(btnQuit, QuitGame);
        SetupButton(btnProfile, () => GoToScene("Profile"));
        SetupButton(btnStore, () => GoToScene("Store"));
        SetupButton(btnSeasonPass, () => GoToScene("SeasonPass"));

        // Play menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        // Set default mode
        if (GameState.Instance != null)
        {
            GameState.Instance.currentMode = GameState.Mode.FFA;
        }
    }

    private void SetupButton(Button button, UnityEngine.Events.UnityAction callback)
    {
        if (button == null) return;
        button.onClick.AddListener(() =>
        {
            PlayClickSound();
            callback();
        });

        // Add hover sound if EventTrigger or pointer enter is handled
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

    private void TriggerExplosionTransition()
    {
        if (isTransitioning) return;
        StartCoroutine(ExplosionTransitionRoutine());
    }

    private IEnumerator ExplosionTransitionRoutine()
    {
        isTransitioning = true;

        // Play explosion SFX
        if (explosionSfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(explosionSfx, 1f);
        }

        // Activate and animate explosion image
        if (explosionImage != null)
        {
            explosionImage.gameObject.SetActive(true);
            explosionImage.transform.localScale = Vector3.one * 0.1f;
            explosionImage.color = Color.white;
        }

        float duration = 0.8f;
        float elapsed = 0f;

        CanvasGroup startScreenGroup = startScreenRoot != null ? startScreenRoot.GetComponent<CanvasGroup>() : null;
        if (startScreenGroup == null && startScreenRoot != null)
        {
            startScreenGroup = startScreenRoot.AddComponent<CanvasGroup>();
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Animate explosion scale and fade
            if (explosionImage != null)
            {
                // Ease out scale
                float scaleT = 1f - Mathf.Pow(1f - t, 3f);
                explosionImage.transform.localScale = Vector3.Lerp(Vector3.one * 0.1f, Vector3.one * 3f, scaleT);

                // Fade out explosion at the end
                if (t > 0.5f)
                {
                    float alphaT = (t - 0.5f) / 0.5f;
                    explosionImage.color = new Color(1f, 1f, 1f, 1f - alphaT);
                }
            }

            // Cross-fade screens
            if (startScreenGroup != null)
            {
                startScreenGroup.alpha = Mathf.Clamp01(1f - t * 2f); // Fades out faster
            }

            if (mainMenuRoot != null)
            {
                mainMenuRoot.alpha = Mathf.Clamp01(t);
            }

            yield return null;
        }

        // Clean up start screen
        if (startScreenRoot != null) startScreenRoot.SetActive(false);
        if (explosionImage != null) explosionImage.gameObject.SetActive(false);

        // Enable main menu interaction
        if (mainMenuRoot != null)
        {
            mainMenuRoot.alpha = 1f;
            mainMenuRoot.interactable = true;
            mainMenuRoot.blocksRaycasts = true;
        }

        isTransitioning = false;
    }

    private void PlayGame()
    {
        if (GameState.Instance != null)
        {
            Debug.Log("Opening mode select.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("ModeSelect");
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void GoToScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
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
}
