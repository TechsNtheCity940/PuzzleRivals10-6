using UnityEngine;
using System;
using System.Collections;

public class CollapseCell : MonoBehaviour
{
    public int x, y;
    public int colorIdx;
    public SpriteRenderer mainSprite;
    public SpriteRenderer bgRenderer;
    public Sprite[] animFrames;

    public event Action<int, int> OnClicked;

    public void Setup(int x, int y, int colorIdx, Color color, Sprite sprite)
    {
        this.x = x;
        this.y = y;
        this.colorIdx = colorIdx;
        if (mainSprite) 
        {
            mainSprite.sprite = sprite;
            mainSprite.color = color;
        }
        if (bgRenderer) bgRenderer.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.9f);
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(x, y);
    }

    public void Vaporize()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateVaporize());
    }

    private IEnumerator AnimateVaporize()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0;
        float duration = 0.3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if(animFrames != null && animFrames.Length > 0)
            {
                int frameIdx = Mathf.Clamp(Mathf.FloorToInt(t * animFrames.Length), 0, animFrames.Length - 1);
                if(mainSprite) mainSprite.sprite = animFrames[frameIdx];
            }

            transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.5f, t);
            if (mainSprite)
            {
                Color c = mainSprite.color;
                c.a = 1f - t;
                mainSprite.color = c;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
