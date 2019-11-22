/*
 * Stereoscopic3DImage_Editor
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

#if !UNITY_4 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_2 && !UNITY_5_4 && !UNITY_5_5
#define UNITY_5_6_PLUS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VR3D;
using System.Linq;

[CustomEditor(typeof(Stereoscopic3DImage))]
public class Stereoscopic3DImage_Editor : Editor
{
    private Stereoscopic3DImage theScript;

    private SerializedProperty scriptProperty;
    private SerializedProperty sourceTextureProperty;
    private SerializedProperty sourceTexture2Property;
#if UNITY_5_6_PLUS
    private SerializedProperty videoClipProperty;
    private SerializedProperty videoURLProperty;
#endif
    private SerializedProperty imageFormatProperty;
    private SerializedProperty canvasFormatProperty;
    private SerializedProperty rotationProperty;
    private SerializedProperty swapLeftRightProperty;
    private SerializedProperty convergenceProperty;
    private SerializedProperty maxConvergenceProperty;
    private SerializedProperty convergenceModeProperty;

    private SerializedProperty wrapModeOverrideProperty;
    private SerializedProperty verticalFlipProperty;

    private SerializedProperty leftEyeColorProperty;
    private SerializedProperty rightEyeColorProperty;

    void OnEnable()
    {
        theScript = (Stereoscopic3DImage)target;

        scriptProperty = serializedObject.FindProperty("m_Script");
        sourceTextureProperty = serializedObject.FindProperty("sourceTexture");
        sourceTexture2Property = serializedObject.FindProperty("sourceTexture2");
#if UNITY_5_6_PLUS
        videoClipProperty = serializedObject.FindProperty("videoClip");
        videoURLProperty = serializedObject.FindProperty("videoURL");
#endif
        imageFormatProperty = serializedObject.FindProperty("imageFormat");
        canvasFormatProperty = serializedObject.FindProperty("canvasFormat");
        rotationProperty = serializedObject.FindProperty("rotation");
        swapLeftRightProperty = serializedObject.FindProperty("swapLeftRight");
        convergenceProperty = serializedObject.FindProperty("convergence");
        maxConvergenceProperty = serializedObject.FindProperty("maxConvergence");
        convergenceModeProperty = serializedObject.FindProperty("convergenceMode");

        wrapModeOverrideProperty = serializedObject.FindProperty("wrapModeOverride");
        verticalFlipProperty = serializedObject.FindProperty("verticalFlip");

        leftEyeColorProperty = serializedObject.FindProperty("leftEyeColor");
        rightEyeColorProperty = serializedObject.FindProperty("rightEyeColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Texture sourceTexture = ((Texture)sourceTextureProperty.objectReferenceValue);
        Texture sourceTexture2 = ((Texture)sourceTexture2Property.objectReferenceValue);

        GUILayout.BeginVertical(EditorStyles.objectFieldThumb);

        EditorGUILayout.LabelField("Source Media", EditorStyles.boldLabel);

#if UNITY_5_6_PLUS

        // We do this to hide non relavant fields.        
        if (sourceTextureProperty.objectReferenceValue == null &&
            videoClipProperty.objectReferenceValue == null &&
            videoURLProperty.stringValue != string.Empty &&
            (theScript.imageFormat != ImageFormat.TwoImages && theScript.imageFormat != ImageFormat.Mono_TwoImages))
        {
            EditorGUILayout.PropertyField(videoURLProperty);
        }
        // We do this to hide non relavant fields.
        else if (sourceTextureProperty.objectReferenceValue == null &&
            videoClipProperty.objectReferenceValue != null &&
            (theScript.imageFormat != ImageFormat.TwoImages && theScript.imageFormat != ImageFormat.Mono_TwoImages))
            EditorGUILayout.PropertyField(videoClipProperty);
        else
#endif
        if (sourceTextureProperty.objectReferenceValue != null &&
            (theScript.imageFormat == ImageFormat.TwoImages || theScript.imageFormat == ImageFormat.Mono_TwoImages))
        {
            // We name the texture fields differently when using TwoImages, to make it less confusing.
            EditorGUILayout.PropertyField(sourceTextureProperty, new GUIContent("Left Texture"));

            EditorGUILayout.PropertyField(sourceTexture2Property, new GUIContent("Right Texture"));
        }
        else if (sourceTextureProperty.objectReferenceValue != null)
        {
            EditorGUILayout.PropertyField(sourceTextureProperty);
        }
        else 
        {
            if (theScript.imageFormat == ImageFormat.TwoImages || theScript.imageFormat == ImageFormat.Mono_TwoImages)
            {
                EditorGUILayout.PropertyField(sourceTextureProperty, new GUIContent("Left Texture"));
                EditorGUILayout.PropertyField(sourceTexture2Property, new GUIContent("Right Texture"));
            }
            else
                EditorGUILayout.PropertyField(sourceTextureProperty);
#if UNITY_5_6_PLUS
            EditorGUILayout.PropertyField(videoClipProperty);
            EditorGUILayout.PropertyField(videoURLProperty);
#endif
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical(EditorStyles.objectFieldThumb);

        EditorGUILayout.LabelField("Format Properties", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(imageFormatProperty);

        EditorGUILayout.PropertyField(canvasFormatProperty);

        if (theScript.canvasFormat == CanvasFormat._360 || theScript.canvasFormat == CanvasFormat._180)
        {
            EditorGUILayout.PropertyField(rotationProperty);

            if (theScript.rotation < -359) theScript.rotation += 360;
            else if (theScript.rotation > 359) theScript.rotation -= 360;
        }

        EditorGUILayout.PropertyField(swapLeftRightProperty);

        // We only show the Anaglyph properties when Anaglyph is selected.
        if (theScript.imageFormat == ImageFormat.Anaglyph || theScript.imageFormat == ImageFormat.Mono_Anaglyph)
        {
            EditorGUILayout.PropertyField(leftEyeColorProperty);

            EditorGUILayout.PropertyField(rightEyeColorProperty);
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical(EditorStyles.objectFieldThumb);

        EditorGUILayout.LabelField("Convergence", EditorStyles.boldLabel);

        if (theScript.canvasFormat != CanvasFormat._360 && theScript.canvasFormat != CanvasFormat._180)
            EditorGUILayout.PropertyField(convergenceModeProperty, new GUIContent("Mode"));

        EditorGUILayout.PropertyField(convergenceProperty, new GUIContent("Current"));

        if (theScript.convergenceMode == ConvergenceMode.Tiled || theScript.canvasFormat != CanvasFormat.Standard) GUI.enabled = false;

        EditorGUILayout.PropertyField(maxConvergenceProperty, new GUIContent("Max"), false);

        GUI.enabled = true;

        GUILayout.EndVertical();

        GUILayout.BeginVertical(EditorStyles.objectFieldThumb);

        EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(wrapModeOverrideProperty);

        EditorGUILayout.PropertyField(verticalFlipProperty);

        GUILayout.EndVertical();

        // if using a texture, we show warnings based on texture import settings to help the use maximize the images quality.
        if (sourceTexture)
            ImageSettingsWarnings(sourceTexture, (sourceTexture2 ? "Left Texture" : string.Empty));

        if (sourceTexture2)
            ImageSettingsWarnings(sourceTexture2, "Right Texture");

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Returns the true size of an image file, as opposed to the potentiolly reduced size given by the texture importer.
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static bool GetImageSize(Texture2D asset, out int width, out int height)
    {
        if (asset != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                object[] args = new object[2] { 0, 0 };
                System.Reflection.MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                mi.Invoke(importer, args);

                width = (int)args[0];
                height = (int)args[1];

                return true;
            }
        }

        height = width = 0;
        return false;
    }

    /// <summary>
    /// We display some warnings about image import settings, as some import settings can make 3D decoding impossible, or at least make the image look worse then expected.
    /// The warning are based on quality being optimal. Not performence.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="label"></param>
    private void ImageSettingsWarnings(Texture texture, string label)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        if (textureImporter == null) return;

        if ((textureImporter.npotScale == TextureImporterNPOTScale.None ||
            isPowerOf2(texture)) &&
            !textureImporter.mipmapEnabled &&
            textureImporter.filterMode == FilterMode.Point &&
            textureImporter.textureCompression == TextureImporterCompression.Uncompressed)
            return;

        EditorGUILayout.Separator();

        if (label != "Right Texture")
        {
            EditorGUILayout.LabelField("Recomended Texture Import Settings:", EditorStyles.boldLabel);
            EditorGUILayout.Separator();
        }

        bool isInterlaced = ((theScript.imageFormat == ImageFormat.HorizontalInterlaced ||
            theScript.imageFormat == ImageFormat.VerticalInterlaced ||
            theScript.imageFormat == ImageFormat.Checkerboard ||
            theScript.imageFormat == ImageFormat.Mono_HorizontalInterlaced ||
            theScript.imageFormat == ImageFormat.Mono_VerticalInterlaced ||
            theScript.imageFormat == ImageFormat.Mono_Checkerboard) ? true : false);

        if (label != string.Empty &&
            (textureImporter.npotScale != TextureImporterNPOTScale.None ||
            textureImporter.mipmapEnabled ||
            textureImporter.filterMode != FilterMode.Point ||
            textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
            )
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (textureImporter.npotScale != TextureImporterNPOTScale.None && !isPowerOf2(texture))
            EditorGUILayout.HelpBox("Non Power of 2 = \t\t" + textureImporter.npotScale.ToString() + " (Reccomended: None)", (isInterlaced ? MessageType.Warning : MessageType.Info));

        if (textureImporter.mipmapEnabled)
            EditorGUILayout.HelpBox("Gerate Mip Maps = \ttrue" + " (Reccomended: false)", (isInterlaced ? MessageType.Warning : MessageType.Info));

        if (textureImporter.filterMode != FilterMode.Point)
            EditorGUILayout.HelpBox("Filter Mode = \t\t" + (textureImporter.filterMode == FilterMode.Bilinear ? FilterMode.Bilinear : FilterMode.Trilinear).ToString() + " (Reccomended: Point)", (isInterlaced ? MessageType.Warning : MessageType.Info));

        if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
        {
            string compressionText = "None";
            if (textureImporter.textureCompression == TextureImporterCompression.CompressedLQ) compressionText = "Low Quality";
            else if (textureImporter.textureCompression == TextureImporterCompression.Compressed) compressionText = "Normal Quality";
            else if (textureImporter.textureCompression == TextureImporterCompression.CompressedHQ) compressionText = "High Quality";
            EditorGUILayout.HelpBox("Compression = \t\t" + compressionText + " (Reccomended: None)", (isInterlaced ? MessageType.Warning : MessageType.Info));
        }

        object[] args = new object[2] { 0, 0 };
        System.Reflection.MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mi.Invoke(textureImporter, args);

        int trueWidth = (int)args[0];
        int trueHeight = (int)args[1];

        if (trueWidth > texture.width || trueHeight > texture.height && isInterlaced)
            EditorGUILayout.HelpBox("Max Size = \t\t" + texture.width + "x" + texture.height + " (True Size: " + trueWidth + "x" + trueHeight + ")", (isInterlaced ? MessageType.Warning : MessageType.Info));
    }

    private bool isPowerOf2(Texture texture)
    {
        return (Mathf.NextPowerOfTwo(texture.width) == texture.width &&
            Mathf.NextPowerOfTwo(texture.height) == texture.height);
    }
}