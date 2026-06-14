using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CircuitScramble : PuzzleBase
{
    [Header("Assets")]
    public GameObject terminalPrefab;
    public GameObject wirePrefab;
    public Sprite terminalSprite;

    // Core-charge source sheet (2172x724) does NOT divide into a clean 8-frame strip,
    // so it is used as a single STATIC glow sprite overlaid on the board centre.
    [Header("Core Charge FX")]
    public Sprite coreChargeSprite;
    public float coreChargeScale = 1.5f;

    private SpriteRenderer coreGlow;

    private int wireCount = 4;
    private List<CircuitTerminal> sources = new List<CircuitTerminal>();
    private List<CircuitTerminal> sinks = new List<CircuitTerminal>();
    private int[] sinkColorIndex;
    private Dictionary<int, WireData> activeWires = new Dictionary<int, WireData>();
    private int pendingSourceIdx = -1;
    private LineRenderer pendingLine;

    private Color[] WIRE_COLORS = {
        new Color(1.0f, 0.18f, 0.78f),  // magenta
        new Color(0.0f, 0.90f, 1.0f),   // cyan
        new Color(0.61f, 1.0f, 0.18f),  // lime
        new Color(1.0f, 0.54f, 0.12f),  // orange
        new Color(0.78f, 0.35f, 1.0f),  // purple
        new Color(1.0f, 0.92f, 0.18f)   // yellow
    };

    private class WireData
    {
        public int sinkIdx;
        public bool correct;
        public LineRenderer line;
    }

    public override void Setup(int d)
    {
        base.Setup(d);
        wireCount = Mathf.Clamp(3 + d - 1, 3, WIRE_COLORS.Length);
        BuildBoard();
    }

    private void BuildBoard()
    {
        sinkColorIndex = new int[wireCount];
        List<int> indices = new List<int>();
        for (int i = 0; i < wireCount; i++) indices.Add(i);
        for (int i = 0; i < wireCount; i++)
        {
            int r = Random.Range(0, indices.Count);
            sinkColorIndex[i] = indices[r];
            indices.RemoveAt(r);
        }

        float spacing = 1.2f;
        float startY = (wireCount - 1) * spacing / 2f;

        for (int i = 0; i < wireCount; i++)
        {
            Vector3 pos = new Vector3(-3, startY - i * spacing, 0);
            GameObject go = Instantiate(terminalPrefab, transform);
            go.transform.localPosition = pos;
            CircuitTerminal term = go.GetComponent<CircuitTerminal>();
            term.Setup(i, true, WIRE_COLORS[i], terminalSprite);
            term.OnClicked += HandleTerminalClick;
            sources.Add(term);
        }

        for (int i = 0; i < wireCount; i++)
        {
            Vector3 pos = new Vector3(3, startY - i * spacing, 0);
            GameObject go = Instantiate(terminalPrefab, transform);
            go.transform.localPosition = pos;
            CircuitTerminal term = go.GetComponent<CircuitTerminal>();
            term.Setup(i, false, WIRE_COLORS[sinkColorIndex[i]], terminalSprite);
            term.OnClicked += HandleTerminalClick;
            sinks.Add(term);
        }

        GameObject pgo = Instantiate(wirePrefab, transform);
        pendingLine = pgo.GetComponent<LineRenderer>();
        pendingLine.enabled = false;

        BuildCoreGlow();
    }

    private void BuildCoreGlow()
    {
        if (coreGlow != null) { Destroy(coreGlow.gameObject); coreGlow = null; }
        if (coreChargeSprite == null) return;

        GameObject glowGo = new GameObject("CoreGlow");
        glowGo.transform.SetParent(transform, false);
        glowGo.transform.localPosition = Vector3.zero; // board centre (between sources and sinks)
        glowGo.transform.localScale = new Vector3(coreChargeScale, coreChargeScale, 1f);

        coreGlow = glowGo.AddComponent<SpriteRenderer>();
        coreGlow.sprite = coreChargeSprite;
        coreGlow.sortingOrder = 20;
        coreGlow.enabled = false;
    }

    private IEnumerator ChargeCore()
    {
        if (coreGlow == null) yield break;
        coreGlow.enabled = true;
        Transform t = coreGlow.transform;
        Vector3 baseScale = new Vector3(coreChargeScale, coreChargeScale, 1f);
        float elapsed = 0f;
        float dur = 0.4f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float k = elapsed / dur;
            float s = Mathf.Lerp(0.2f, 1f, Mathf.SmoothStep(0f, 1f, k));
            t.localScale = baseScale * s;
            Color c = coreGlow.color;
            c.a = k;
            coreGlow.color = c;
            yield return null;
        }
        t.localScale = baseScale;
        Color cf = coreGlow.color; cf.a = 1f; coreGlow.color = cf;
    }

    private void HandleTerminalClick(int idx, bool isSource)
    {
        if (!inputEnabled || isSolved) return;

        if (isSource)
        {
            if (activeWires.ContainsKey(idx))
            {
                Destroy(activeWires[idx].line.gameObject);
                activeWires.Remove(idx);
            }
            pendingSourceIdx = idx;
            pendingLine.enabled = true;
            pendingLine.startColor = WIRE_COLORS[idx];
            pendingLine.endColor = WIRE_COLORS[idx];
            if (AudioManager.Instance) AudioManager.Instance.PlaySfx(GameState.Instance.sfxUiHover, 0.5f);
        }
        else
        {
            if (pendingSourceIdx == -1) return;

            int srcIdx = pendingSourceIdx;
            int sinkIdx = idx;
            bool correct = (srcIdx == sinkColorIndex[sinkIdx]);

            int existingSrc = -1;
            foreach (var kvp in activeWires)
            {
                if (kvp.Value.sinkIdx == sinkIdx)
                {
                    existingSrc = kvp.Key;
                    break;
                }
            }
            if (existingSrc != -1)
            {
                Destroy(activeWires[existingSrc].line.gameObject);
                activeWires.Remove(existingSrc);
            }

            GameObject wgo = Instantiate(wirePrefab, transform);
            LineRenderer lr = wgo.GetComponent<LineRenderer>();
            lr.SetPosition(0, sources[srcIdx].transform.position);
            lr.SetPosition(1, sinks[sinkIdx].transform.position);
            
            Color wireColor = correct ? WIRE_COLORS[srcIdx] : Color.red;
            lr.startColor = wireColor;
            lr.endColor = wireColor;

            activeWires[srcIdx] = new WireData { sinkIdx = sinkIdx, correct = correct, line = lr };
            
            pendingSourceIdx = -1;
            pendingLine.enabled = false;

            if (correct)
            {
                if (AudioManager.Instance) AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzlePlace, 0.7f);
            }
            else
            {
                if (AudioManager.Instance) AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.7f);
                StartCoroutine(FlashIncorrect(lr));
            }

            CheckSolved();
        }
    }

    private IEnumerator FlashIncorrect(LineRenderer lr)
    {
        for (int i = 0; i < 3; i++)
        {
            lr.enabled = false;
            yield return new WaitForSeconds(0.1f);
            lr.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Update()
    {
        if (pendingSourceIdx != -1)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            pendingLine.SetPosition(0, sources[pendingSourceIdx].transform.position);
            pendingLine.SetPosition(1, mousePos);
        }
    }

    private void CheckSolved()
    {
        int correctCount = 0;
        foreach (var w in activeWires.Values)
        {
            if (w.correct) correctCount++;
        }

        if (correctCount == wireCount)
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.8f);
            StartCoroutine(ChargeCore());
            Solve(80 + difficulty * 40);
        }
    }

    public void BotSolve()
    {
        for (int i = 0; i < wireCount; i++)
        {
            if (!activeWires.ContainsKey(i) || !activeWires[i].correct)
            {
                for (int s = 0; s < wireCount; s++)
                {
                    if (sinkColorIndex[s] == i)
                    {
                        pendingSourceIdx = i;
                        HandleTerminalClick(s, false);
                        return;
                    }
                }
            }
        }
    }
}
