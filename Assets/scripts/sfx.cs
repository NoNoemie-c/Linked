using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class sfx : MonoBehaviour
{
    void Update() {
        if (!GetComponent<AudioSource>().isPlaying)
            Destroy(gameObject);
    }
}
