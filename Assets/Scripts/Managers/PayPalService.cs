using System;
using UnityEngine;

// Reconciliation stub. The project has no real-money payment backend, so this
// local gateway immediately reports success — allowing the Store / Season Pass
// USD flows to function in-editor. Replace with a real integration when needed.
public class PayPalService : MonoBehaviour
{
    private static PayPalService _instance;

    public static PayPalService Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PayPalService");
                _instance = go.AddComponent<PayPalService>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void InitiatePayment(float amountUSD, string description, Action<bool> onComplete)
    {
        Debug.Log($"[PayPalService stub] Simulating payment of ${amountUSD:0.00} for '{description}'. Auto-approving.");
        onComplete?.Invoke(true);
    }
}
