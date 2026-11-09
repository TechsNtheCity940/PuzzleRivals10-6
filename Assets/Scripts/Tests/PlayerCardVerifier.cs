using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Verification script to test PlayerCard functionality
public class PlayerCardVerifier : MonoBehaviour
{
    [Header("Test Configuration")]
    public PlayerCard testCard;
    public Player testPlayer;
    
    void Start()
    {
        VerifyPlayerCard();
    }
    
    public void VerifyPlayerCard()
    {
        if (testCard == null)
        {
            Debug.LogError("PlayerCardVerifier: No test card assigned!");
            return;
        }
        
        // Create test loadout with all cosmetics
        var loadout = new CosmeticLoadout();
        
        // Try to get test sprites from the library
        var lib = CosmeticLibrary.Instance;
        if (lib != null)
        {
            loadout.background = lib.SpriteById(CosmeticCategory.BackgroundCard, 0);
            loadout.frame = lib.SpriteById(CosmeticCategory.Frame, 0);
            loadout.nameplate = lib.SpriteById(CosmeticCategory.Nameplate, 0);
            loadout.emblem = lib.SpriteById(CosmeticCategory.Emblem, 0);
            loadout.banner = lib.SpriteById(CosmeticCategory.Banner, 0);
            loadout.avatar = lib.SpriteById(CosmeticCategory.Avatar, 0);
        }
        
        // Create test player
        var player = new Player(0, "TestPlayer0", true);
        player.puzzlesSolved = 5;
        player.color = Color.red;
        
        // Bind the test data
        testCard.Bind(player, loadout);
        
        // Verify each component
        Debug.Log($"PlayerCard Verification Results:");
        Debug.Log($"  Name Text: {(testCard.nameText != null ? testCard.nameText.text : "NULL")}");
        Debug.Log($"  Solved Text: {(testCard.solvedText != null ? testCard.solvedText.text : "NULL")}");
        Debug.Log($"  Background Image: {(testCard.backgroundCard != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"  Avatar Image: {(testCard.avatar != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"  Frame Image: {(testCard.frame != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"  Emblem Image: {(testCard.emblem != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"  Nameplate Image: {(testCard.nameplate != null ? "ASSIGNED" : "NULL")}");
        
        // Check if all UI elements are present
        bool allPresent = testCard.backgroundCard != null &&
                         testCard.avatar != null &&
                         testCard.frame != null &&
                         testCard.emblem != null &&
                         testCard.nameplate != null &&
                         testCard.nameText != null &&
                         testCard.solvedText != null;
        
        if (allPresent)
        {
            Debug.Log("✅ PlayerCard: All UI components present and configured correctly!");
        }
        else
        {
            Debug.LogError("❌ PlayerCard: Some UI components are missing or null!");
        }
    }
}