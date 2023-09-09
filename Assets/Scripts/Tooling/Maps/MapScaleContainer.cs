using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapScaleContainer : VisualElement
{

    public Vector2 _baseSize;

    public Vector2 CurrentSize => _baseSize;
     
    private Vector2 _pan = Vector2.zero;
 

    public MapScaleContainer(Vector2 baseSize)
    {
        _baseSize = baseSize;
 
        // Set the initial size
        style.width = new Length(baseSize.x, LengthUnit.Pixel);
        style.height = new Length(baseSize.y, LengthUnit.Pixel);
    }

  

    public void Pan(Vector2 delta)
    {  
        _pan += delta;
        style.left = new Length(style.left.value.value + delta.x, LengthUnit.Pixel);
        style.top = new Length(style.top.value.value + delta.y, LengthUnit.Pixel);
    }
}
