using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Toggle))]
[ExecuteInEditMode]
public class SpriteToggle : MonoBehaviour
{
    
    Toggle _toggle;
    Image _img;
    public Sprite activeSprite;
    public Sprite disableSprite;

    // Start is called before the first frame update
    void Start()
    {
        _toggle = this.GetComponent<Toggle>();
        _img =(Image) _toggle.graphic;
        _toggle.onValueChanged.AddListener((isOn) =>
        {
            _img.sprite = isOn? activeSprite:disableSprite;
        });
    }

    
}
