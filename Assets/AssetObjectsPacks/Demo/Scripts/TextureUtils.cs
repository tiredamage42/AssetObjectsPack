using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureUtils 
{
    public static Texture2D MakeGradientTexture (int width, int height){//}, Color32 color) {
        Color[] colors = new Color[width * height];
        
        Color white = new Color(1.0f,1.0f,1.0f,1.0f);
        Color white2 = new Color(2.0f,2.0f,2.0f,2.0f);
        
        float midHeight = height * .5f;
        for (int y = 0; y < height; y++) {
            float yt = Mathf.Abs(midHeight - y) / midHeight;
            for (int x = 0; x < width; x++) {
                colors[x + y * width] = Color32.Lerp(white2, white, yt);
            } 
        }
        Texture2D ret = new Texture2D(width, height);
        ret.SetPixels(colors);
        ret.Apply();
        return ret;
    }
}
