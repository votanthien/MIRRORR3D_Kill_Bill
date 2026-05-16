using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class ColorPiece : MonoBehaviour
    {
        [System.Serializable]
        public struct ColorSprite
        {
            public ColorType color;
            public Sprite sprite;
        }
        [SerializeField] private ColorType currentColor;
        public ColorSprite[] colorSprites;

        private ColorType _color;

        public ColorType Color
        {
            get => _color;
            set => SetColor(value);
        }

        public int NumColors => colorSprites.Length;

        private SpriteRenderer _sprite;
        [SerializeField]
        public Dictionary<ColorType, Sprite> _colorSpriteDict;

        private void Awake ()
        {
            _sprite = transform.Find("piece").GetComponent<SpriteRenderer>();

            // instantiating and populating a Dictionary of all Color Types / Sprites (for fast lookup)
            _colorSpriteDict = new Dictionary<ColorType, Sprite>();

            for (int i = 0; i < colorSprites.Length; i++)
            {
                if (!_colorSpriteDict.ContainsKey (colorSprites[i].color))
                {
                    _colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
                }
            }
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
}
