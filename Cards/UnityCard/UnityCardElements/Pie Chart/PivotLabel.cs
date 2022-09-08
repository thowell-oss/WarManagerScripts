using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotLabel : MonoBehaviour
{
    public TMPro.TMP_Text LabelText;
    private RectTransform _labelTextTransform;

    [SerializeField] TMPro.TMP_Text LabelTextPrefab;

    public Transform pivotObj;


    public void SetPivot(float rotation, float orbitDistance, float offsetX, float offsetY, float fillAmt, string label, string value, int fontSize)
    {

        orbitDistance = Mathf.Abs(orbitDistance);
        pivotObj.transform.localPosition = new Vector3(pivotObj.transform.localPosition.x, -orbitDistance, pivotObj.transform.localPosition.z);

        LeanTween.value(gameObject, (x) => { TweenRotation(x, orbitDistance, offsetX, offsetY); }, 0, rotation + fillAmt / 2, 1f).setEaseOutCubic();

        if (LabelText == null)
            LabelText = Instantiate<TMPro.TMP_Text>(LabelTextPrefab, pivotObj.transform);

        LabelText.fontSize = fontSize;

        _labelTextTransform = LabelText.GetComponent<RectTransform>();

        if (label == null || label == string.Empty)
            label = "<empty>";

        if (value == null || value == string.Empty)
            value = "<empty>";

        LabelText.text = label + "\n" + value;
    }

    private void TweenRotation(float currentAddedTotal, float orbitDistance, float offsetX, float offsetY)
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y,
         360 - (360 * currentAddedTotal)));

        //LabelText.transform.localPosition = pivotObj.transform.localPosition;
        LabelText.transform.rotation = Quaternion.Euler(Vector3.zero);

        float y = Mathf.Sin(currentAddedTotal * (2 * Mathf.PI)) * orbitDistance + .25f;
        float x = Mathf.Cos(-currentAddedTotal * (2 * Mathf.PI)) * orbitDistance - .25f;

        y = ((y + 1) / 2);
        x = ((x + 1) / 2);

        _labelTextTransform.pivot = new Vector2(y, x);
    }
}
