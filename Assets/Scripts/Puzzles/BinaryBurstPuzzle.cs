using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BinaryBurstPuzzle : BasePuzzle
{
    [Header("Assets")]
    public Sprite nodeOffSprite;
    public Sprite nodeOnSprite;
    public Sprite[] nodeSolveFrames;
    public GameObject nodePrefab;

    [Header("UI")]
    public TMP_Text targetText;
    public TMP_Text currentText;
    public RectTransform nodeContainer;

    private int targetValue;
    private int currentValue;
    private List<BinaryNode> nodes = new List<BinaryNode>();
    private int nodeCount = 6; // Default to 2^0 to 2^5 (1 to 32)

    protected override void BuildBoard()
    {
        currentValue = 0;
        nodeCount = Mathf.Clamp(4 + difficulty / 2, 4, 8);
        int maxValue = (int)Mathf.Pow(2, nodeCount) - 1;
        targetValue = rng.Next(1, maxValue + 1);

        if (targetText != null) targetText.text = "TARGET: " + targetValue;
        UpdateCurrentText();

        ClearContainer();
        nodes.Clear();

        for (int i = 0; i < nodeCount; i++)
        {
            int val = (int)Mathf.Pow(2, i);
            GameObject go = Instantiate(nodePrefab, nodeContainer);
            BinaryNode node = go.GetComponent<BinaryNode>();
            node.Init(val, nodeOffSprite, nodeOnSprite);
            node.OnToggled += HandleNodeToggled;
            nodes.Add(node);
        }
    }

    private void ClearContainer()
    {
        foreach (Transform child in nodeContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void HandleNodeToggled(BinaryNode node)
    {
        if (!CanAcceptInput()) return;

        if (node.IsOn) currentValue += node.Value;
        else currentValue -= node.Value;

        UpdateCurrentText();
        CheckWin();
    }

    private void UpdateCurrentText()
    {
        if (currentText != null) currentText.text = "SUM: " + currentValue;
    }

    private void CheckWin()
    {
        if (currentValue == targetValue)
        {
            StartCoroutine(WinRoutine());
        }
    }

    private IEnumerator WinRoutine()
    {
        foreach (var node in nodes)
        {
            if (node.IsOn)
            {
                var anim = node.gameObject.AddComponent<UISpriteSheetAnimator>();
                anim.PlaySet(nodeSolveFrames, 12f, false);
            }
        }
        yield return new WaitForSeconds(0.5f);
        Solve();
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;

        // Determine which bit to toggle to get closer to target
        for (int i = nodeCount - 1; i >= 0; i--)
        {
            int val = (int)Mathf.Pow(2, i);
            bool bitInTarget = (targetValue & val) != 0;
            if (nodes[i].IsOn != bitInTarget)
            {
                nodes[i].ToggleManual();
                return;
            }
        }
    }
}

public class BinaryNode : MonoBehaviour
{
    public int Value;
    public bool IsOn;
    public event System.Action<BinaryNode> OnToggled;

    private Image image;
    private Sprite offSprite;
    private Sprite onSprite;
    private TMP_Text valText;

    private void Awake()
    {
        image = GetComponent<Image>();
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
    }

    public void Init(int val, Sprite off, Sprite on)
    {
        Value = val;
        offSprite = off;
        onSprite = on;
        IsOn = false;
        if (image == null) image = GetComponent<Image>();
        image.sprite = offSprite;
        valText = GetComponentInChildren<TMP_Text>();
        if (valText != null) valText.text = val.ToString();
    }

    public void ToggleManual()
    {
        IsOn = !IsOn;
        image.sprite = IsOn ? onSprite : offSprite;
        OnToggled?.Invoke(this);
    }

    public void OnClick()
    {
        ToggleManual();
    }
}
