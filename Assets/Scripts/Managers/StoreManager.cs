using UnityEngine;
using System.Collections.Generic;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;
    public StoreConfig config;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PurchaseItemWithCoins(CustomizationItem item)
    {
        var profile = ProfileManager.Instance.currentProfile;
        if (profile.coins >= item.coinPrice)
        {
            profile.coins -= item.coinPrice;
            UnlockItem(item, profile);
            ProfileManager.Instance.SaveProfile();
            Debug.Log($"Purchased {item.itemName} with coins.");
        }
        else
        {
            Debug.LogWarning("Not enough coins.");
        }
    }

    public void PurchaseItemWithGems(CustomizationItem item)
    {
        var profile = ProfileManager.Instance.currentProfile;
        if (profile.gems >= item.gemPrice)
        {
            profile.gems -= item.gemPrice;
            UnlockItem(item, profile);
            ProfileManager.Instance.SaveProfile();
            Debug.Log($"Purchased {item.itemName} with gems.");
        }
        else
        {
            Debug.LogWarning("Not enough gems.");
        }
    }

    public void PurchaseBundle(StoreBundle bundle)
    {
        if (bundle.usdPrice > 0)
        {
            PayPalService.Instance.InitiatePayment(bundle.usdPrice, bundle.bundleName, (success) => {
                if (success)
                {
                    var profile = ProfileManager.Instance.currentProfile;
                    foreach (var item in bundle.items) UnlockItem(item, profile);
                    ProfileManager.Instance.SaveProfile();
                    Debug.Log($"Purchased bundle {bundle.bundleName} with USD.");
                }
            });
        }
    }

    private void UnlockItem(CustomizationItem item, PlayerProfileData profile)
    {
        switch (item.type)
        {
            case ItemType.Banner: if (!profile.unlockedBanners.Contains(item.id)) profile.unlockedBanners.Add(item.id); break;
            case ItemType.Frame: if (!profile.unlockedFrames.Contains(item.id)) profile.unlockedFrames.Add(item.id); break;
            case ItemType.Avatar: if (!profile.unlockedAvatars.Contains(item.id)) profile.unlockedAvatars.Add(item.id); break;
            case ItemType.Emblem: if (!profile.unlockedEmblems.Contains(item.id)) profile.unlockedEmblems.Add(item.id); break;
        }
    }
}
