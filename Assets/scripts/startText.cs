using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public sealed class startText : MonoBehaviour
{
    public float time, timePre;
    public Image img;
    public TextMeshProUGUI txt;
    public AudioSource sfx;

    private IEnumerator Start() {
        if (Time.time > 10) 
            yield break;
            
        float a;

        txt.color = new Color(txt.color.r, txt.color.g, txt. color.b, 0);

        yield return new WaitForSeconds(timePre);

        for (int i = 0; i < 51; i++) {
            a = i / 50f;

            txt.color = new Color(txt.color.r, txt.color.g, txt. color.b, a);

            yield return new WaitForSeconds(.02f);
        }

        yield return new WaitForSeconds(time - 1);

        for (int i = 50; i >= 0; i--) {
            a = i / 50f;

            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
            txt.color = new Color(txt.color.r, txt.color.g, txt. color.b, a);

            yield return new WaitForSeconds(.02f);
        }

        img.gameObject.SetActive(false);

        sfx.Play();
    }
}
