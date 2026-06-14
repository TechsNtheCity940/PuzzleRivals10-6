using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ButtonMashingPuzzle : BasePuzzle
{
    [Header("Button Mashing")]
    public Button mainButton;
    public TMP_Text counterText;
    public int minClicks = 5;
    public int maxClicks = 15;

    [Header("Art")]
    public Sprite[] buttonFrames; // 0: idle, 1: pressed/click animation frames

    private int _targetClicks;
    private int _currentClicks;

    protected override void BuildBoard()
    {
        _targetClicks = rng.Next(minClicks + difficulty, maxClicks + difficulty * 2);
        _currentClicks = 0;
        UpdateUI();

        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(OnButtonClick);
            
            // Set up animator if frames exist
            UISpriteSheetAnimator anim = mainButton.image.GetComponent<UISpriteSheetAnimator>();
            if (anim == null) anim = mainButton.image.gameObject.AddComponent<UISpriteSheetAnimator>();
            anim.loop = false;
            anim.playOnEnable = false;
            if (buttonFrames != null && buttonFrames.Length > 0)
                mainButton.image.sprite = buttonFrames[0];
        }
    }

    private void OnButtonClick()
    {
        if (!CanAcceptInput()) return;

        _currentClicks++;
        UpdateUI();

        // Visual feedback: Scale pulse
        StartCoroutine(PulseButton());

        // Visual feedback: Sprite animation
        UISpriteSheetAnimator anim = mainButton.image.GetComponent<UISpriteSheetAnimator>();
        if (anim != null && buttonFrames != null && buttonFrames.Length > 1)
        {
            anim.PlaySet(buttonFrames, 20f, false);
        }

        if (_currentClicks >= _targetClicks)
        {
            Solve();
        }
    }

    private System.Collections.IEnumerator PulseButton()
    {
        Vector3 startScale = Vector3.one;
        Vector3 peakScale = Vector3.one * 1.2f;
        float duration = 0.1f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainButton.transform.localScale = Vector3.Lerp(startScale, peakScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainButton.transform.localScale = Vector3.Lerp(peakScale, startScale, elapsed / duration);
            yield return null;
        }
        mainButton.transform.localScale = startScale;
    }

    private void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = (_targetClicks - _currentClicks).ToString();
        }
    }

    public override void AutoSolveChunk()
    {
        OnButtonClick();
    }
}