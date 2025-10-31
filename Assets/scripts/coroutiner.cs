using UnityEngine;
using System.Collections;

public class coroutiner : MonoBehaviour
{
    private static coroutiner instance;

    private void Awake() =>
        instance = this;
    
    public static Coroutine start(IEnumerator e) =>
        instance.StartCoroutine(e);
}