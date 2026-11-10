using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BrainVSBrainUI : MonoBehaviour
{
    public static BrainVSBrainUI Instance;

    [Header("Score Display")]
    public TMP_Text playerScoreText;
    public TMP_Text opponentScoreText;
    
    [Header("Notification")]
    public TMP_Text notificationText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateScores(int playerPoints, int opponentPoints)
    {
        if (playerScoreText != null) playerScoreText.text = playerPoints.ToString();
        if (opponentScoreText != null) opponentScoreText.text = opponentPoints.ToString();
    }

    public void ShowNotification(string message, Color color)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.color = color;
        }
    }
}