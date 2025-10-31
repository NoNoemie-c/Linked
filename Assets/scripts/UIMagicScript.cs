using UnityEngine;

public sealed class UIMagicScript : MonoBehaviour
{
    // bruh canvas in screen space camera
    void Update() =>
        Camera.main.orthographicSize = 0.281f * Screen.width;
}
