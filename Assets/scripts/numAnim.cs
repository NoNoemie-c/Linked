using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class numAnim : MonoBehaviour
{
    public int duration;
    public int turns;
    public Vector3 v;
    public TextMeshProUGUI t;

    void Start() {
        v = new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), 0);
        t = GetComponent<TextMeshProUGUI>();
    }

    void FixedUpdate() {
        turns ++;

        v += Vector3.down * .1f;

        t.color = new Color(t.color.r, t.color.g, t.color.b, 1 - turns / (float)duration);
        transform.localPosition += v;

        if (turns > duration)
            Destroy(gameObject);
    }
}
