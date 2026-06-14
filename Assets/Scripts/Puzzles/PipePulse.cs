using UnityEngine;
using System.Collections.Generic;

public class PipePulse : PuzzleBase
{
    [Header("Assets")]
    public GameObject tilePrefab;
    public Sprite[] pipeSprites; // 0: Straight, 1: Angle, 2: Pump

    [Header("Flow Animation Frames")]
    public Sprite[] pipeStraightFrames; // shape 0
    public Sprite[] pipeAngleFrames;    // shape 1
    public Sprite[] pipePumpFrames;     // shape 2

    private int gridSize = 4;
    private float tileSpacing = 1.2f;
    private List<PipePulseTile> tiles = new List<PipePulseTile>();

    public override void Setup(int d)
    {
        base.Setup(d);
        gridSize = Mathf.Clamp(3 + d - 1, 3, 5);
        BuildGrid();
    }

    private void BuildGrid()
    {
        foreach (var t in tiles) if(t) Destroy(t.gameObject);
        tiles.Clear();

        float startPos = -(gridSize - 1) * tileSpacing / 2f;

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                int shape = Random.Range(0, 3);
                float scrambledRot = Random.Range(1, 4) * 90f;

                Vector3 pos = new Vector3(startPos + c * tileSpacing, -startPos - r * tileSpacing, 0);
                GameObject go = Instantiate(tilePrefab, transform);
                go.transform.localPosition = pos;
                
                PipePulseTile tile = go.GetComponent<PipePulseTile>();
                Sprite[] frames = GetShapeFrames(shape);
                Sprite fallback = (pipeSprites != null && shape < pipeSprites.Length) ? pipeSprites[shape] : null;
                tile.Setup(r, c, shape, scrambledRot, fallback, frames);
                tile.OnClicked += HandleTileClick;
                tiles.Add(tile);
            }
        }
        CheckSolved();
    }

    private Sprite[] GetShapeFrames(int shape)
    {
        switch (shape)
        {
            case 0: return pipeStraightFrames;
            case 1: return pipeAngleFrames;
            case 2: return pipePumpFrames;
            default: return null;
        }
    }

    private void HandleTileClick(int r, int c)
    {
        if (!inputEnabled || isSolved) return;
        
        PipePulseTile tile = tiles.Find(t => t.row == r && t.col == c);
        if (tile != null)
        {
            tile.Rotate();
            if (AudioManager.Instance != null && GameState.Instance != null)
                AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.5f);
            CheckSolved();
        }
    }

    private void CheckSolved()
    {
        int correct = 0;
        foreach (var tile in tiles)
        {
            bool isCorrect = Mathf.Approximately(tile.currentRotation % 360, tile.targetRotation);
            tile.SetSolved(isCorrect, new Color(0.1f, 0.4f, 0.1f), new Color(0.1f, 0.1f, 0.2f));
            if (isCorrect) correct++;
        }

        if (correct == tiles.Count)
        {
            Solve(100 + difficulty * 50);
        }
    }

    public void BotSolve()
    {
        List<PipePulseTile> unsolved = tiles.FindAll(t => !Mathf.Approximately(t.currentRotation % 360, t.targetRotation));
        if (unsolved.Count > 0)
        {
            PipePulseTile target = unsolved[Random.Range(0, unsolved.Count)];
            target.Rotate();
            CheckSolved();
        }
    }
}
