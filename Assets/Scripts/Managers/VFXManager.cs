using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [Header("Puzzle Effects")]
    public GameObject solveEffectPrefab;
    public GameObject wrongEffectPrefab;
    
    [Header("Match Effects")]
    public GameObject winEffectPrefab;
    public GameObject attackEffectPrefab;
    public GameObject defenseEffectPrefab;

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

    public void PlaySolveEffect(Vector3 position)
    {
        if (solveEffectPrefab != null) Instantiate(solveEffectPrefab, position, Quaternion.identity);
        else Debug.Log("VFX: Correct Solve!");
    }

    public void PlayWrongEffect(Vector3 position)
    {
        if (wrongEffectPrefab != null) Instantiate(wrongEffectPrefab, position, Quaternion.identity);
        else Debug.Log("VFX: Wrong Selection!");
    }

    public void PlayWinEffect(Vector3 position)
    {
        if (winEffectPrefab != null) Instantiate(winEffectPrefab, position, Quaternion.identity);
        else Debug.Log("VFX: Match Win Celebration!");
    }

    public void PlayAttackEffect(Vector3 position)
    {
        if (attackEffectPrefab != null) Instantiate(attackEffectPrefab, position, Quaternion.identity);
        else Debug.Log("VFX: Attack Released!");
    }

    public void PlayDefenseEffect(Vector3 position)
    {
        if (defenseEffectPrefab != null) Instantiate(defenseEffectPrefab, position, Quaternion.identity);
        else Debug.Log("VFX: Defense Activated!");
    }
}
