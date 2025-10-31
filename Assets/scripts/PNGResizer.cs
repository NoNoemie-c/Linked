using System.IO;
using UnityEngine;
using System;

public sealed class PNGResizer {
    public static string resources = "C:/Users/thiba/game/Assets/Resources/";
    public static Color[] clear;
    
    public static void resize(string path, int size = 512) {
        path = path.Replace(resources, "").Replace(".png", "");
        Debug.Log(path);
        Texture2D oldTex = Resources.Load<Texture2D>(path);
        if (oldTex.width == 512 && oldTex.height == 512)
            return;
        Texture2D newTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        int oldSize = Mathf.Max(oldTex.width, oldTex.height);

        for (int x = 0; x < size; x ++)
            for (int y = 0; y < size; y ++)
                newTex.SetPixel(x, y, 
                    /*x < (size - oldSize) / 2 || y < (size - oldSize) / 2 || x > size - (size - oldSize) / 2 || y > size - (size - oldSize) / 2 ? 
                    Color.clear :*/ oldTex.GetPixel(Mathf.RoundToInt(x / (float) size * oldSize), Mathf.RoundToInt(y / (float) size * oldSize)));
        
        File.Move(resources + path + ".png", resources + path + " save.png");
        File.WriteAllBytes($"{resources}{path}.png", newTex.EncodeToPNG());
    }

}