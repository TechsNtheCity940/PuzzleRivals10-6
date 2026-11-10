using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the trap logic for CircuitScramblePuzzle (Overcharge, Backdraft, Relay traps)
/// </summary>
public class CircuitScrambleTraps
{
    private CircuitScramblePuzzle puzzle;
    private System.Random rng;

    // Trap states
    private float overchargeTimer = 0f;
    private float backdraftTimer = 0f;
    private bool relayActive = false;
    private bool isExploding = false;

    // Trap positions
    private Vector2Int overchargeGatePos;
    private Vector2Int backdraftGatePos;
    private Vector2Int relayGatePos;
    private Vector2Int corePos;
    private Vector2Int srcAPos;
    private Vector2Int srcBPos;
    private Vector2Int correctGatePos;
    private bool gateIsAnd;

    // Trap constants
    private const float OVERCHARGE_TIME = 3f;
    private const float RELAY_CAP = 0.4f;

    public CircuitScrambleTraps(CircuitScramblePuzzle puzzle, System.Random rng)
    {
        this.puzzle = puzzle;
        this.rng = rng;
    }

    public void Initialize(
        Vector2Int overchargeGatePos,
        Vector2Int backdraftGatePos,
        Vector2Int relayGatePos,
        Vector2Int corePos,
        Vector2Int srcAPos,
        Vector2Int srcBPos,
        Vector2Int correctGatePos,
        bool gateIsAnd)
    {
        this.overchargeGatePos = overchargeGatePos;
        this.backdraftGatePos = backdraftGatePos;
        this.relayGatePos = relayGatePos;
        this.corePos = corePos;
        this.srcAPos = srcAPos;
        this.srcBPos = srcBPos;
        this.correctGatePos = correctGatePos;
        this.gateIsAnd = gateIsAnd;
        
        ResetTraps();
    }

    public void ResetTraps()
    {
        overchargeTimer = 0f;
        backdraftTimer = 0f;
        relayActive = false;
        isExploding = false;
    }

    public void UpdateTraps(HashSet<Vector2Int> powered, ref float currentCharge, ref bool isCurrentlySatisfied, ref bool relayActiveOut)
    {
        if (backdraftTimer > 0f)
            backdraftTimer = Mathf.Max(0f, backdraftTimer - Time.deltaTime);

        bool ocSatisfied = EvaluateOverchargeTrap(powered);
        bool relaySatisfied = EvaluateRelayTrap(powered);

        relayActive = relaySatisfied;
        relayActiveOut = relayActive;

        bool overchargeTriggered = false;
        if (ocSatisfied && !isExploding)
        {
            overchargeTimer += Time.deltaTime;
            if (overchargeTimer >= OVERCHARGE_TIME)
            {
                TriggerOverchargeSurge(ref currentCharge);
                overchargeTriggered = true;
            }
        }
        else if (!isExploding)
        {
            overchargeTimer = 0f;
        }

        bool backdraftTriggered = false;
        bool bdSatisfied = EvaluateBackdraftTrap(powered);
        if (bdSatisfied && backdraftTimer <= 0f && !isExploding)
        {
            TriggerBackdraftShortCircuit(ref currentCharge);
            backdraftTriggered = true;
        }

        // The puzzle owns correct-gate evaluation. Traps may interrupt a solve,
        // but they must never manufacture a successful circuit themselves.
        if (overchargeTriggered || backdraftTriggered)
            isCurrentlySatisfied = false;
    }

    private bool EvaluateOverchargeTrap(HashSet<Vector2Int> powered)
    {
        bool ocInA = puzzle.IsConnectedPublic(srcAPos, overchargeGatePos + new Vector2Int(-1, 0), powered);
        bool ocInB = puzzle.IsConnectedPublic(srcBPos, overchargeGatePos + new Vector2Int(0, -1), powered);
        bool ocOutA = puzzle.IsConnectedPublic(overchargeGatePos + new Vector2Int(1, 0), corePos, powered);
        bool ocOutB = puzzle.IsConnectedPublic(overchargeGatePos + new Vector2Int(0, 1), corePos, powered);
        return ocInA && ocInB && (ocOutA || ocOutB);
    }

    private bool EvaluateBackdraftTrap(HashSet<Vector2Int> powered)
    {
        bool bdInA = puzzle.IsConnectedPublic(srcAPos, backdraftGatePos + new Vector2Int(-1, 0), powered);
        bool bdInB = puzzle.IsConnectedPublic(srcBPos, backdraftGatePos + new Vector2Int(0, -1), powered);
        bool bdOutA = puzzle.IsConnectedPublic(backdraftGatePos + new Vector2Int(1, 0), corePos, powered);
        bool bdOutB = puzzle.IsConnectedPublic(backdraftGatePos + new Vector2Int(0, 1), corePos, powered);

        // Backdraft is the partial-circuit hazard: exactly one input reaches
        // the wrong gate while an output lane is live. Both inputs together
        // belong to the slower overcharge trap instead.
        return (bdInA ^ bdInB) && (bdOutA || bdOutB);
    }

    private bool EvaluateRelayTrap(HashSet<Vector2Int> powered)
    {
        bool relayInA = puzzle.IsConnectedPublic(srcAPos, relayGatePos + new Vector2Int(-1, 0), powered);
        bool relayInB = puzzle.IsConnectedPublic(srcBPos, relayGatePos + new Vector2Int(0, -1), powered);
        bool relayOut = puzzle.IsConnectedPublic(relayGatePos + new Vector2Int(1, 0), corePos, powered) ||
                        puzzle.IsConnectedPublic(relayGatePos + new Vector2Int(0, 1), corePos, powered);
        return relayInA && relayInB && relayOut;
    }

    public IEnumerator TriggerOverchargeSurge()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
        {
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 1f);
        }

        for (int i = 0; i < 4; i++)
        {
            foreach (var tile in puzzle.tilesInternal)
            {
                if (tile != null)
                {
                    tile.Image.color = (i % 2 == 0) ? Color.red : Color.white;
                }
            }
            yield return new WaitForSeconds(0.12f);
        }
        
        isExploding = false;
        puzzle.UpdatePowerFlow();
    }

    public void TriggerOverchargeSurge(ref float currentCharge)
    {
        // Reset charge and start the visual effect coroutine
        currentCharge = 0f;
        overchargeTimer = 0f;
        isExploding = true;
        puzzle.StartCoroutine(TriggerOverchargeSurge());
    }

    public void TriggerBackdraftShortCircuit(ref float currentCharge)
    {
        // Cooldown prevents a connected backdraft gate from re-triggering on
        // every flow refresh. The puzzle refreshes its flow after this call.
        backdraftTimer = 1.5f;
        currentCharge = 0f;

        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.9f);

        puzzle.ScrambleWiresInternal();
    }

    public bool IsExploding => isExploding;
    public bool RelayActive => relayActive;
    public float OverchargeTimer => overchargeTimer;
    
    public bool IsTrapActive(string trapName)
    {
        switch (trapName.ToLower())
        {
            case "overcharge": return overchargeTimer > 0f;
            case "relay": return relayActive;
            case "backdraft": return backdraftTimer > 0f;
            default: return false;
        }
    }
}