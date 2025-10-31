using UnityEngine;
using System.Collections;

public static class ScreenShaker
{
    public static void Shake(Vector3 delta, Vector3 rotation, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(delta, rotation, timeBeforeStart));
    public static void ShakeRand(float deltaStrength, float rotationStrength, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Random.insideUnitSphere * deltaStrength, Random.rotation.eulerAngles * rotationStrength, timeBeforeStart));
    public static void ShakeRot(Vector3 rotation, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Vector3.zero, rotation, timeBeforeStart));
    public static void ShakeRandRot(float rotationStrength, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Vector3.zero, Random.rotation.eulerAngles * rotationStrength, timeBeforeStart));
    public static void ShakePos(Vector3 delta, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(delta, Vector3.zero, timeBeforeStart));
    public static void ShakeRandPos(float deltaStrength, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Random.insideUnitSphere * deltaStrength, Vector3.zero, timeBeforeStart));

    private static IEnumerator _shake(Vector3 delta, Vector3 rotation, float timeBeforeStart) {
        yield return new WaitForSeconds(timeBeforeStart);

        Transform cam = Camera.main.transform;

        Vector3 pos = cam.position;
        Vector3 rot = cam.rotation.eulerAngles;

        cam.position += delta;
        cam.eulerAngles += rotation;

        int count = 0;
        while ((rot - cam.eulerAngles).magnitude > .001f || (pos - cam.position).magnitude > .001f) {
            cam.Rotate((rot - cam.eulerAngles) / 5, Space.World); 
            cam.position += (pos - cam.position) / 5;
            
            if (count > 50)
                break;

            yield return new WaitForSeconds(.02f);
            count ++;
        }   

        cam.position = pos;
        cam.eulerAngles = rot;
    }
}

public static class ScreenShakerUI
{
    private static Vector3 origin;

    public static void Setup(Transform canvas) =>
        origin = canvas.position;

    public static void Shake(Vector3 delta, Vector3 rotation, Transform canvas, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(delta, rotation, canvas, timeBeforeStart));
    public static void ShakeRand(float deltaStrength, float rotationStrength, Transform canvas, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Random.insideUnitSphere * deltaStrength, Random.rotation.eulerAngles * rotationStrength, canvas, timeBeforeStart));
    public static void ShakeRot(Vector3 rotation, Transform canvas, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Vector3.zero, rotation, canvas, timeBeforeStart));
    public static void ShakeRandRot(float rotationStrength, Transform canvas, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Vector3.zero, Random.rotation.eulerAngles * rotationStrength, canvas, timeBeforeStart));
    public static void ShakePos(Vector3 delta, Transform canvas, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(delta, Vector3.zero, canvas, timeBeforeStart));
    public static void ShakeRandPos(float deltaStrength, Transform canvas, float timeBeforeStart = 0) =>
        coroutiner.start(_shake(Random.onUnitSphere * deltaStrength, Vector3.zero, canvas, timeBeforeStart));

    private static IEnumerator _shake(Vector3 delta, Vector3 rotation, Transform canvas, float timeBeforeStart) {
        yield return new WaitForSeconds(timeBeforeStart);

        canvas.position += delta;
        canvas.eulerAngles += rotation;

        int count = 0;
        while (canvas.eulerAngles.magnitude > .001f || (origin - canvas.position).magnitude > .001f) {
            canvas.Rotate(-canvas.eulerAngles / 5, Space.World); 
            canvas.position += (origin - canvas.position) / 5;

            if (count > 50)
                break;

            yield return new WaitForSeconds(.02f);
            count ++;
        }   

        canvas.position = origin;
        canvas.eulerAngles = Vector3.zero;
    }
}