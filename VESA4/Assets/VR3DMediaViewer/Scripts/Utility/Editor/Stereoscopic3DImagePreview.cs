using UnityEngine;
using UnityEditor;
using VR3D;

[CustomPreview(typeof(Stereoscopic3DImage))]
public class Stereoscopic3DImagePreview : ObjectPreview
{
    Texture2D previewTexture;

    public override bool HasPreviewGUI()
    {
        Stereoscopic3DImage theScript = (Stereoscopic3DImage)target;
        if (theScript.sourceTexture != null ||
            theScript.videoClip != null)
        {
            return true;
        }

        return false;
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle guiStyle)
    {
        Stereoscopic3DImage theScript = (Stereoscopic3DImage)target;
        
        if (theScript.videoClip)
        {
            if (previewTexture == null)
                previewTexture = AssetPreview.GetMiniThumbnail(theScript.videoClip);
            GUI.DrawTexture(r, previewTexture, ScaleMode.ScaleToFit);
        }
        else if (theScript.sourceTexture)
        {
            if (theScript.imageFormat == ImageFormat.HorizontalInterlaced ||
                theScript.imageFormat == ImageFormat.VerticalInterlaced ||
                theScript.imageFormat == ImageFormat.Checkerboard ||
                theScript.imageFormat == ImageFormat.Anaglyph)
            {
                if (previewTexture == null)
                {
                    Texture2D sourceTexture = (Texture2D)theScript.sourceTexture;
                    Texture2D readableSourceTexture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, sourceTexture.mipmapCount > 1);
                    readableSourceTexture.LoadRawTextureData(sourceTexture.GetRawTextureData());
                    readableSourceTexture.Apply();

                    switch (theScript.imageFormat)
                    {
                        case ImageFormat.HorizontalInterlaced:
                            previewTexture = Stereoscopic3DScreeenshot.InterlacedToTopBottomTexture(readableSourceTexture);
                            break;
                        case ImageFormat.VerticalInterlaced:
                            previewTexture = Stereoscopic3DScreeenshot.VerticalInterlacedToSBSTexture(readableSourceTexture);
                            break;
                        case ImageFormat.Checkerboard:
                            previewTexture = Stereoscopic3DScreeenshot.CheckerboardToSBSTexture(readableSourceTexture);
                            break;
                        case ImageFormat.Anaglyph:
                            previewTexture = Stereoscopic3DScreeenshot.AnaglyphToSBSTexture(readableSourceTexture, theScript.leftEyeColor, theScript.rightEyeColor);
                            break;
                    }
                }

                EditorGUI.DrawPreviewTexture(r, previewTexture, null, ScaleMode.ScaleToFit);
            }
            else if (theScript.imageFormat == ImageFormat.TwoImages)
            {
                //Texture2D sourceTexture = (Texture2D)theScript.sourceTexture;
                //Texture2D sourceTexture2 = (Texture2D)theScript.sourceTexture2;
                //Texture2D readableSourceTexture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, sourceTexture.mipmapCount > 1);
                //Texture2D readableSourceTexture2 = new Texture2D(sourceTexture2.width, sourceTexture2.height, sourceTexture2.format, sourceTexture2.mipmapCount > 1);
                //readableSourceTexture.LoadRawTextureData(sourceTexture.GetRawTextureData());
                //readableSourceTexture2.LoadRawTextureData(sourceTexture2.GetRawTextureData());
                //readableSourceTexture.Apply();
                //readableSourceTexture2.Apply();

                // TODO: May do something else here later.
                EditorGUI.DrawPreviewTexture(r, theScript.sourceTexture, null, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawPreviewTexture(r, theScript.sourceTexture, null, ScaleMode.ScaleToFit);
            }
        }

        if (theScript.imageFormat != ImageFormat.Side_By_Side &&
            theScript.imageFormat != ImageFormat.Top_Bottom &&
            theScript.imageFormat != ImageFormat.TwoImages &&
            theScript.imageFormat != ImageFormat.Mono_Side_By_Side &&
            theScript.imageFormat != ImageFormat.Mono_Top_Bottom &&
            theScript.imageFormat != ImageFormat.Mono_TwoImages &&
            theScript.imageFormat != ImageFormat._2D)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = Color.yellow;

            GUI.Label(r, "Shown as " +
                (theScript.imageFormat == ImageFormat.VerticalInterlaced ||
                theScript.imageFormat == ImageFormat.Checkerboard ||
                theScript.imageFormat == ImageFormat.Anaglyph ||
                theScript.imageFormat == ImageFormat.Mono_VerticalInterlaced ||
                theScript.imageFormat == ImageFormat.Mono_Checkerboard ||
                theScript.imageFormat == ImageFormat.Mono_Anaglyph ? "Side-by-Side" : "Top-Bottom"), labelStyle);
        }
    }
}
