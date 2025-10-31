using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class anim : MonoBehaviour
{
    public animData Data;
    public bool triggerOnStart = false;

    public void Start() {
        if (triggerOnStart)
            Trigger(i => i += 0);
    }

    public void Trigger(Action<int> a) =>
        StartCoroutine(Data.trigger(GetComponent<Image>(), a));
}
