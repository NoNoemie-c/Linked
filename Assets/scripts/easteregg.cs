using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class easteregg : MonoBehaviour
{
    public void startDeZoom() => StartCoroutine(deZoom());

    IEnumerator deZoom() {
        while (Camera.main.orthographicSize < 40) {
            Camera.main.orthographicSize += .1f;
            yield return new WaitForSeconds(.01f);
        }
    }
}
