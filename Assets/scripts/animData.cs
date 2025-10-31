using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "new anim", menuName = "anim")]
public sealed class animData : ScriptableObject
{
    public Sprite[] sprites;
    public float interval;

    public IEnumerator trigger(Image img, Action<int> update) {
        for (int i = 0; i < sprites.Length; i++) {
            img.sprite = sprites[i];

            update(i);

            yield return new WaitForSeconds(interval);
        }

        Destroy(img.gameObject);
    }
}