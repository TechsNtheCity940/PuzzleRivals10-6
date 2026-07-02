using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignalSyncPuzzle : BasePuzzle
{
    [Header("Wave Rendering")]
    public UILineRenderer targetWave;
    public UILineRenderer userWave;
    public int points = 100;

    [Header("Sliders")]
    public Slider freqSlider;
    public Slider ampSlider;
    public Slider phaseSlider;

    private float targetFreq, targetAmp, targetPhase;
    private float userFreq, userAmp, userPhase;

    protected override void BuildBoard()
    {
        targetFreq = (float)rng.NextDouble() * 2f + 1f;
        targetAmp = (float)rng.NextDouble() * 0.5f + 0.5f;
        targetPhase = (float)rng.NextDouble() * Mathf.PI;

        userFreq = 1f;
        userAmp = 1f;
        userPhase = 0f;

        if (freqSlider != null) freqSlider.value = 0.5f;
        if (ampSlider != null) ampSlider.value = 0.5f;
        if (phaseSlider != null) phaseSlider.value = 0f;

        DrawWaves();
    }

    protected override void Update()
    {
        base.Update();
        if (!isRunning || isSolved) return;

        userFreq = freqSlider.value * 4f + 0.5f;
        userAmp = ampSlider.value * 1.5f;
        userPhase = phaseSlider.value * Mathf.PI * 2f;

        DrawWaves();
        CheckWin();
    }

    private void DrawWaves()
    {
        List<Vector2> targetPoints = new List<Vector2>();
        List<Vector2> userPoints = new List<Vector2>();

        float width = 800f;
        float height = 400f;

        for (int i = 0; i < points; i++)
        {
            float x = (float)i / points;
            float tx = x * Mathf.PI * 2f * targetFreq + targetPhase + Time.time * 2f;
            float ux = x * Mathf.PI * 2f * userFreq + userPhase + Time.time * 2f;

            targetPoints.Add(new Vector2(x * width - width/2, Mathf.Sin(tx) * targetAmp * height/2));
            userPoints.Add(new Vector2(x * width - width/2, Mathf.Sin(ux) * userAmp * height/2));
        }

        if (targetWave != null) targetWave.points = targetPoints.ToArray();
        if (userWave != null) userWave.points = userPoints.ToArray();
    }

    private void CheckWin()
    {
        float df = Mathf.Abs(userFreq - targetFreq);
        float da = Mathf.Abs(userAmp - targetAmp);
        float dp = Mathf.Abs(Mathf.DeltaAngle(userPhase * Mathf.Rad2Deg, targetPhase * Mathf.Rad2Deg));

        if (df < 0.1f && da < 0.1f && dp < 10f)
        {
            Solve();
        }
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;
        freqSlider.value = Mathf.MoveTowards(freqSlider.value, (targetFreq - 0.5f) / 4f, 0.1f);
        ampSlider.value = Mathf.MoveTowards(ampSlider.value, targetAmp / 1.5f, 0.1f);
        phaseSlider.value = Mathf.MoveTowards(phaseSlider.value, targetPhase / (Mathf.PI * 2f), 0.1f);
    }
}
