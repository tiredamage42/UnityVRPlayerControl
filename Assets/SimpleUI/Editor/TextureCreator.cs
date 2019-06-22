using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

using System.IO;
namespace SimpleUI
{


    
public class TextureInverter : ScriptableWizard
{

    void SaveTexture (string path, int width, int height, Color[] colors) {
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(colors);        
        tex.Apply(false); 
        File.WriteAllBytes(path + ".png", tex.EncodeToPNG()); 
    }

    public string dirPath = "Assets/SimpleUI/Icons/";
    public bool invertAlpha;
    public Texture2D[] textures;
    
    [MenuItem("GameObject/SimpleUI/Texture Invert")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<TextureInverter>("Invert Texture");
    }

    void InvertTexture (Texture2D texture) {
        if (!dirPath.EndsWith("/")) {
            dirPath += "/";
        }

        Color[] colors = texture.GetPixels(0);

        for (int i = 0; i< colors.Length; i++) {
            Color c = colors[i];
            float m = 1.0f;
            colors[i] = new Color(m - c.r, m-c.g, m-c.b, invertAlpha ?  m-c.a : c.a);
        }
        SaveTexture(dirPath + texture.name + "_inverted", texture.width, texture.height, colors);
    }



    void OnWizardCreate () {
        
        for (int i =0 ; i < textures.Length; i++) {
            InvertTexture(textures[i]);
        }
    
        AssetDatabase.Refresh();
    }


}

public class TextureCreator : ScriptableWizard
{

    void SaveTexture (string path, Color32[] colors) {
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels32(colors);
        
        // var tex = new Texture2D(texture.width,texture.height);
        // // tex.isReadable = true; 
        // tex.SetPixels32(texture.GetPixels32()); 
        tex.Apply(false); 
        File.WriteAllBytes(path + ".png", tex.EncodeToPNG()); 
    }

    public string dirPath = "Assets/SimpleUI/";
    public int borderWidth = 4;
    public int spaceBuffer = 4;

    public int cornerReach = 12;

    public int width = 64;
    public int height = 64;


    [MenuItem("GameObject/SimpleUICreator")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<TextureCreator>("Create UI", "Create");//, "");
        //If you don't want to use the secondary button simply leave it out:
        //ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
    }

    void OnWizardCreate () {
        if (!dirPath.EndsWith("/")) {
            dirPath += "/";
        }

        // Debug.Log("creating textures at :: " + dirPath);

        // SaveTexture (dirPath + "FillRect", CreateFillRectangle());
        // SaveTexture (dirPath + "FillRectSpace", CreateFillRectangleWithSpace());
        // SaveTexture (dirPath + "OutlineRect", CreateOutlineRect());

        // SaveTexture (dirPath + "TopBottomRect", CreateTopBottomRect());
        // SaveTexture (dirPath + "LeftRightRect", CreateLeftRightRect());

        // SaveTexture (dirPath + "TopBottomRectWCorner", CreateTopBottomRectWCorner());
        // SaveTexture (dirPath + "LeftRightRectWCorner", CreateLeftRightRectWCorner());

        // SaveTexture (dirPath + "Corners", CreateCorners());


        SaveTexture(dirPath + "CircleFull", CreateCircle());
        SaveTexture(dirPath + "CircleEmpty", CreateCircleEmpty());
        

        AssetDatabase.Refresh();
    }

 
 Color32[] CreateCircle () {
     Color32[] colors = new Color32[width * height];

     Vector2 maskCenter = new Vector2(width * 0.5f, height * 0.5f);
        
     for(int y = 0; y < height; ++y){
         for(int x = 0; x < width; ++x){

 
             float distFromCenter = Vector2.Distance(maskCenter, new Vector2(x, y));

             float maskPixel = distFromCenter <= width * .5f ? 1 : 0;// (0.5f - (distFromCenter / width)) * maskThreshold;
             colors[x + y * width] = new Color(maskPixel, maskPixel, maskPixel, maskPixel);
             
         }
     }
     return colors;
 }


Color32[] CreateCircleEmpty () {
     Color32[] colors = new Color32[width * height];

     Vector2 maskCenter = new Vector2(width * 0.5f, height * 0.5f);
        
     for(int y = 0; y < height; ++y){
         for(int x = 0; x < width; ++x){

 
             float distFromCenter = Vector2.Distance(maskCenter, new Vector2(x, y));
            float edge = width * .5f;

            float innerEdge = edge - borderWidth;

             float maskPixel = distFromCenter <= edge && distFromCenter >= innerEdge ? 1 : 0;// (0.5f - (distFromCenter / width)) * maskThreshold;
             colors[x + y * width] = new Color(maskPixel, maskPixel, maskPixel, maskPixel);
             
         }
     }
     return colors;
 }














    Color32[] CreateCorners () {

        Color32[] colors = new Color32[width * height];

        int buffer = borderWidth;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (

                    
                    ((y < cornerReach || y > height - cornerReach) && (x < buffer || x > width - buffer)) || 
                    
                    ((x < cornerReach || x > width - cornerReach) && (y < buffer || y > height - buffer))
                    
                    
                    ) {
                    colors[x + y * width] = new Color32(255,255,255,255);
                }
                else {
                    colors[x + y * width] = new Color32(0,0,0,0);
                }
            }
        }
        return colors;
        
    }

    Color32[] CreateTopBottomRectWCorner () {

        Color32[] colors = new Color32[width * height];

        int buffer = borderWidth;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (((y < cornerReach || y > height - cornerReach) && (x < buffer || x > width - buffer)) || y < buffer || y > height - buffer) {
                    colors[x + y * width] = new Color32(255,255,255,255);
                }
                else {
                    colors[x + y * width] = new Color32(0,0,0,0);
                }
            }
        }
        return colors;
        
    }
    Color32[] CreateLeftRightRectWCorner () {

        Color32[] colors = new Color32[width * height];

        int buffer = borderWidth;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (((x < cornerReach || x > width - cornerReach) && (y < buffer || y > height - buffer)) || x < buffer || x > width - buffer) {
                    colors[x + y * width] = new Color32(255,255,255,255);
                }
                else {
                    colors[x + y * width] = new Color32(0,0,0,0);
                }
            }
        }
        return colors;
        
    }



    Color32[] CreateLeftRightRect () {

        Color32[] colors = new Color32[width * height];

        int buffer = borderWidth;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (x < buffer || x > width - buffer) {
                    colors[x + y * width] = new Color32(255,255,255,255);
                }
                else {
                    colors[x + y * width] = new Color32(0,0,0,0);
                }
            }
        }
        return colors;
        
    }
    Color32[] CreateTopBottomRect () {

        Color32[] colors = new Color32[width * height];

        int buffer = borderWidth;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (y < buffer || y > height - buffer) {
                    colors[x + y * width] = new Color32(255,255,255,255);
                }
                else {
                    colors[x + y * width] = new Color32(0,0,0,0);
                }
            }
        }
        return colors;
        
    }

    Color32[] CreateOutlineRect () {

        Color32[] colors = new Color32[width * height];

        int buffer = borderWidth;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (x < buffer || x > width - buffer || y < buffer || y > height - buffer) {
                    colors[x + y * width] = new Color32(255,255,255,255);
                }
                else {
                    colors[x + y * width] = new Color32(0,0,0,0);
                }
            }
        }
        return colors;
    }

    Color32[] CreateFillRectangleWithSpace () {

        Color32[] colors = new Color32[width * height];

        int buffer = borderWidth + spaceBuffer;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (x < buffer || x > width - buffer || y < buffer || y > height - buffer) {
                    colors[x + y * width] = new Color32(0,0,0,0);
                }
                else {
                    colors[x + y * width] = new Color32(255,255,255,255);
                }
            }
        }
        return colors;
    }

    Color32[] CreateFillRectangle () {

        Color32[] colors = new Color32[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colors[x + y * width] = new Color32(255,255,255,255);
            }
        }
        return colors;
    }



}
}
