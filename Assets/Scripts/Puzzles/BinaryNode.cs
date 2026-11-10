using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Binary node used by BinaryBurstPuzzle - represents a power-of-two toggleable value.
/// </summary>
[RequireComponent(typeof(Image))]
public class BinaryNode : MonoBehaviour, IPointerClickHandler
{
    private int _value; // Power of 2 (1, 2, 4, 8, 16, etc.)
    public int Value => _value;
    public bool IsOn { get; private set; }
    
    public event Action<BinaryNode> OnToggled;
    
    private Image _image;
    private UISpriteSheetAnimator _animator;

    // Store sprites for reset after animation
    private Sprite _offSprite;
    private Sprite _onSprite;
    
    public Image Image
    {
        get
        {
            if (_image == null) _image = GetComponent<Image>();
            return _image;
        }
    }
    
    public void Init(int powerValue, Sprite offSprite, Sprite onSprite, Sprite[] solveFrames = null)
    {
        _value = powerValue;
        IsOn = false;
        _offSprite = offSprite;
        _onSprite = onSprite;
        
        Image.sprite = offSprite;
        Image.color = Color.white;
        
        if (solveFrames != null && solveFrames.Length > 0)
        {
            _animator = gameObject.GetComponent<UISpriteSheetAnimator>();
            if (_animator == null) _animator = gameObject.AddComponent<UISpriteSheetAnimator>();
            _animator.playOnEnable = false;
        }
    }
    
    public void Toggle()
    {
        IsOn = !IsOn;
        if (_animator != null && _animator.frames != null)
        {
            _animator.PlaySet(_animator.frames, 12f, false, () => 
            {
                Image.sprite = IsOn ? _onSprite : _offSprite;
            });
        }
        OnToggled?.Invoke(this);
    }
    
    public void ToggleManual(bool on)
    {
        IsOn = on;
        UpdateVisualState();
        OnToggled?.Invoke(this);
    }
    
    private void UpdateVisualState()
    {
        if (_animator != null && _animator.frames != null)
        {
            _animator.PlaySet(_animator.frames, 12f, false, () => 
            {
                Image.sprite = IsOn ? _onSprite : _offSprite;
            });
        }
    }
    
    public void SetVisual(bool on, Sprite offSprite, Sprite onSprite)
    {
        IsOn = on;
        Image.sprite = on ? onSprite : offSprite;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }
}