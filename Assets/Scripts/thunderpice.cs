using Match3;
using System.Collections.Generic;
using UnityEngine;

public class thunderpice : MonoBehaviour
{
    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }
    [SerializeField] private ColorType currentColor;
    public ColorSprite colorSprites;

    private ColorType _color;

    public ColorType Color
    {
        get => _color;
        set => SetColor(value);
    }

    

    private SpriteRenderer _sprite;
    [SerializeField]
    public Dictionary<ColorType, Sprite> _colorSpriteDict;

    private void Awake()
    {
        _sprite = transform.Find("piece").GetComponent<SpriteRenderer>();

        // instantiating and populating a Dictionary of all Color Types / Sprites (for fast lookup)
        _colorSpriteDict = new Dictionary<ColorType, Sprite>();

        _colorSpriteDict.Add(colorSprites.color, colorSprites.sprite);
    }

    public void SetColor(ColorType newColor)
    {
        _color = newColor;
        currentColor = newColor;
        if (_colorSpriteDict.ContainsKey(newColor))
        {
            _sprite.sprite = _colorSpriteDict[newColor];
        }
    }


}
