using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIRotation : MonoBehaviour
{
    private RectTransform _uiRect;

    public Vector3 Eulers;
    public float Speed;

    // Update is called once per frame
    void Update()
    {
        if(_uiRect == null)
            _uiRect = GetComponent<RectTransform>();

        _uiRect.Rotate((Eulers * Speed) * Time.deltaTime);
        
    }
}
