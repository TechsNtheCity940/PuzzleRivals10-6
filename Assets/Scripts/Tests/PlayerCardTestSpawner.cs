// Quick test to spawn and verify PlayerCard in editor
// Attach this to any GameObject in the scene

using UnityEngine;
using UnityEngine.UI;

public class PlayerCardTestSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerCard cardPrefab;
    public Canvas targetCanvas;
    
    void Start()
    {
#if UNITY_EDITOR
        if (cardPrefab != null && targetCanvas != null)
        {
            CreateTestCard();
        }
#endif
    }
    
    public void CreateTestCard()
    {
        // Clear existing test cards
        var existing = FindObjectsByType<PlayerCard>(FindObjectsSortInstance.None);
        foreach (var card in existing)
        {
            if (card.name.Contains("TestCard"))
                DestroyImmediate(card.gameObject);
        }
        
        // Create test card
        var cardGO = Instantiate(cardPrefab, targetCanvas.transform);
        cardGO.name = "TestCard";
        var card = cardGO.GetComponent<PlayerCard>();
        
        // Create test loadout
        var lib = CosmeticLibrary.Instance;
        var loadout = new CosmeticLoadout();
        
        if (lib != null)
        {
            loadout.background = lib.SpriteById(CosmeticCategory.BackgroundCard, 0);
            loadout.frame = lib.SpriteById(CosmeticCategory.Frame, 0);
            loadout.nameplate = lib.SpriteById(CosmeticCategory.Nameplate, 0);
            loadout.emblem = lib.SpriteById(CosmeticCategory.Emblem, 0);
            loadout.banner = lib.SpriteById(CosmeticCategory.Banner, 0);
            loadout.avatar = lib.SpriteById(CosmeticCategory.Avatar, 0);
        }
        
        // Create test player (puzzle-tag = username)
        var player = new Player(0, "PuzzleMaster_001", true);
        player.puzzlesSolved = 42;
        
        // Bind and display
        card.Bind(player, loadout);
        
        Debug.Log("✅ Test PlayerCard created with all cosmetics:");
        Debug.Log("   - Avatar: " + (loadout.avatar != null));
        Debug.Log("   - Frame: " + (loadout.frame != null));
        Debug.Log("   - Emblem: " + (loadout.emblem != null));
        Debug.Log("   - Nameplate: " + (loadout.nameplate != null));
        Debug.Log("   - Banner: " + (loadout.banner != null));
        Debug.Log("   - Background: " + (loadout.background != null));
        Debug.Log("   - Username (puzzle-tag): " + player.playerName);
        Debug.Log("   - Solved Puzzles: " + player.puzzlesSolved);
    }
    
    void OnValidate()
    {
        // Call in edit mode to test
        if (Application.isEditor && !Application.isPlaying)
        {
            CreateTestCard();
        }
    }
}