using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

using System.IO;
namespace SimpleUI
{

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

        Debug.Log("creating textures at :: " + dirPath);

        SaveTexture (dirPath + "FillRect", CreateFillRectangle());
        SaveTexture (dirPath + "FillRectSpace", CreateFillRectangleWithSpace());
        SaveTexture (dirPath + "OutlineRect", CreateOutlineRect());

        SaveTexture (dirPath + "TopBottomRect", CreateTopBottomRect());
        SaveTexture (dirPath + "LeftRightRect", CreateLeftRightRect());

        SaveTexture (dirPath + "TopBottomRectWCorner", CreateTopBottomRectWCorner());
        SaveTexture (dirPath + "LeftRightRectWCorner", CreateLeftRightRectWCorner());

        SaveTexture (dirPath + "Corners", CreateCorners());

        AssetDatabase.Refresh();
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
