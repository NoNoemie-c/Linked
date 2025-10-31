using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class numBar : MonoBehaviour
{
    private float val;
    private float actualVal;
    private float baseWidth;
    private RectTransform t;
    public TextMeshProUGUI Text;

    public int var1, var2;

    void Start() {
        t = GetComponent<RectTransform>();
        baseWidth = t.rect.width;
    }

    void FixedUpdate() {
        Text.text = $"{var1} / {var2}";

        if (var1 == 0 && var2 == 0)
            return;
            
        val = (float) var1 / (float) var2;
        
        if (Mathf.Abs(val - actualVal) < .02f)
            return;
        
        actualVal = Mathf.Lerp(actualVal, val, .4f);

        t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, baseWidth * actualVal);
    }
}
