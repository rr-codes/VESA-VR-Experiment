using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;

public struct Duo<T>
{
    public T Left, Right;

    public static Duo<T> Of(T left, T right) => new Duo<T>() {Left = left, Right = right};
}

public struct ImageSet<T>
{
    public Duo<T> Original, Compressed;
    
    public ImageSet(Duo<T> original, Duo<T> compressed)
    {
        this.Original = original;
        this.Compressed = compressed;
    }
}

public class Controller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static List<ImageSet<Texture2D>> ConvertImagesToTextures(params ImageSet<string>[] stringSet)
    {
        var list = new List<ImageSet<Texture2D>>();
        foreach (var set in stringSet)
        {
            var origLeft  = Cv2.ImRead(set.Original.Left,    ImreadModes.AnyColor | ImreadModes.AnyDepth);
            var origRight = Cv2.ImRead(set.Original.Right,   ImreadModes.AnyColor | ImreadModes.AnyDepth);
            var compLeft  = Cv2.ImRead(set.Compressed.Left,  ImreadModes.AnyColor | ImreadModes.AnyDepth);
            var compRight = Cv2.ImRead(set.Compressed.Right, ImreadModes.AnyColor | ImreadModes.AnyDepth);

            list.Add(new ImageSet<Texture2D>(
                Duo<Texture2D>.Of(MatToTexture(origLeft), MatToTexture(origRight)),
                Duo<Texture2D>.Of(MatToTexture(compLeft), MatToTexture(compRight))
                )
            );
        }

        return list;
    }

    static Texture2D MatToTexture(Mat mat)
    {
        var h = mat.Height;
        var w = mat.Width;

        var data = new byte[h * w];
        mat.GetArray(0, 0, data);
        
        var colors = new Color[h * w];
        Parallel.For(0, h, i =>
        {
            for (var j = 0; j < w; j++)
            {
                var vec = data[j + i * w];
                var color = new Color(vec, vec, vec, 0);
                colors[j + i * w] = color;
            }
        });
        
        var texture = new Texture2D(w, h, TextureFormat.RGBAHalf, true, true);
        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }
}
