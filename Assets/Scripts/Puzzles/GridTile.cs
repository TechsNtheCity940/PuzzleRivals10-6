using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Shared UI grid tile used by the PipePulse, CollapseGrid and MirrorMaze
/// overlays (all reference GridTile.prefab). Holds a grid coordinate and a
/// generic integer "type" the owning puzzle interprets (pipe shape, brick
/// state, mirror orientation, etc.). Raises OnClicked when tapped.
/// </summary>
public class GridTile : UITile
{
    public int type;
    private Image _image;
    
    public void Init(int gx, int gy, int tileType)
    {
        base.Init(gx, gy);
        x = gx;
        y = gy;
        type = tileType;
    }
    
    public Image OverlayImage => Image;
    
    public override void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
    }
}