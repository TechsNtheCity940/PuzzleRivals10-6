using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A single row of data shown on the end-of-match leaderboard.
public struct LeaderboardEntry
{
    public string playerName;
    public string scoreLabel;   // e.g. "SOLVED: 3" or "SCORE: 800"
    public string placeLabel;   // e.g. "1ST"
    public Sprite emblem;       // bronze/silver/gold for top 3, null otherwise
}

// Full-screen end-of-match results overlay. Pure view: a HUD controller builds a
// ranked list of LeaderboardEntry and calls Show(). Rows are cloned from an
// inactive RowTemplate child (named children: Place, Emblem, Name, Score).
public class MatchLeaderboard : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text titleText;
    public RectTransform rowContainer;
    public GameObject rowTemplate;

    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    public void Show(string title, List<LeaderboardEntry> entries)
    {
        gameObject.SetActive(true);
        if (titleText != null) titleText.text = title;
        if (rowTemplate != null) rowTemplate.SetActive(false);

        foreach (var r in spawnedRows) if (r != null) Destroy(r);
        spawnedRows.Clear();

        if (rowTemplate == null || rowContainer == null || entries == null) return;

        foreach (var e in entries)
        {
            var row = Instantiate(rowTemplate, rowContainer);
            row.SetActive(true);
            SetText(row.transform, "Place", e.placeLabel);
            SetText(row.transform, "Name", e.playerName);
            SetText(row.transform, "Score", e.scoreLabel);

            var emblemTf = row.transform.Find("Emblem");
            if (emblemTf != null)
            {
                var img = emblemTf.GetComponent<Image>();
                if (img != null)
                {
                    if (e.emblem != null)
                    {
                        img.sprite = e.emblem;
                        img.color = Color.white;
                        img.enabled = true;
                    }
                    else
                    {
                        img.enabled = false;
                    }
                }
            }
            spawnedRows.Add(row);
        }
    }

    private void SetText(Transform parent, string child, string value)
    {
        var tf = parent.Find(child);
        if (tf == null) return;
        var t = tf.GetComponent<TMP_Text>();
        if (t != null) t.text = value;
    }
}
