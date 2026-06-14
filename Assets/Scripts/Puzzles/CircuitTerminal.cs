using UnityEngine;
using System;
using System.Collections;

public class CircuitTerminal : MonoBehaviour
{
    public int index;
    public bool isSource;
    public Color terminalColor;
    public SpriteRenderer mainSprite;
    public SpriteRenderer glowSprite;

    public event Action<int, bool> OnClicked;

    public void Setup(int idx, bool source, Color color, Sprite sprite)
    {
        index = idx;
        isSource = source;
        terminalColor = color;
        if (mainSprite) 
        {
            mainSprite.sprite = sprite;
            mainSprite.color = Color.white;
        }
        if (glowSprite) 
        {
            glowSprite.sprite = sprite;
            glowSprite.color = new Color(color.r, color.g, color.b, 0.5f);
        }
        
        StartCoroutine(PulseGlow());
    }

    private IEnumerator PulseGlow()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * 4f) + 1f) / 2f;
            if (glowSprite)
            {
                Color c = glowSprite.color;
                c.a = Mathf.Lerp(0.2f, 0.8f, t);
                glowSprite.color = c;
            }
            yield return null;
        }
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(index, isSource);
    }
}
