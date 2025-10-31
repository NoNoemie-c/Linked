using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class cursor : MonoBehaviour
{
    private float t;
    public float increment = .05f;

    void FixedUpdate() {
        t += increment;

        transform.localScale = Vector3.one * (Mathf.Sin(t) / 20 + 1.1f);
    }
}
