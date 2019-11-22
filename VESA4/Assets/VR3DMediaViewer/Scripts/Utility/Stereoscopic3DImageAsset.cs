/*
 * Stereoscopic3DImageAsset
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

#if !UNITY_4 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_2 && !UNITY_5_4 && !UNITY_5_5
#define UNITY_5_6_PLUS
#endif

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace VR3D
{
    /// <summary>
    /// For creation of Stereoscopic 3D image container assets.
    /// </summary>
    public class Stereoscopic3DImageAsset
    {
        /// <summary>
        /// Creates a Stereoscopic 3D image container, and selects appropriate files into it and names it after them if they are selected.
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("Assets/Create/Stereoscopic 3D Image")]
        public static void CreateStereoscopic3DImage(MenuCommand command)
        {
            // We get the currently selected object and its type so we can allow users to create a new S3D image with a selected texture/video already selected in it.
            Object selectedObject = Selection.activeObject;
            System.Type selectedObjectType = null;
            if (selectedObject != null)
                selectedObjectType = Selection.activeObject.GetType();

            // Here we create a new empty S3D Image container.
            Stereoscopic3DImage s3DImageScriptableObject = ScriptableObject.CreateInstance<Stereoscopic3DImage>();

            // Need to figure out where we should place this newly created S3D Image.
            string selectedObjectPath = AssetDatabase.GetAssetPath(selectedObject);

            if (selectedObjectPath == string.Empty) selectedObjectPath = "Assets";
            else if (Path.GetExtension(selectedObjectPath) != string.Empty)
                selectedObjectPath = selectedObjectPath.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(selectedObjectPath + "/" + (selectedObject != null ? selectedObject.name : "NewStereoscopic3DImage") + ".asset");

            // Now we make an asset from the object.
            AssetDatabase.CreateAsset(s3DImageScriptableObject, assetPathAndName);

            // Save it.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select it.
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = s3DImageScriptableObject;
                        
            // Did we create this by having a asset selected?
            if (selectedObjectType != null)
            {
                // If it was a form of texture that was selected, we select that texture in the new S3DImage.
                if (selectedObjectType.ToString() == "UnityEngine.Texture" ||
                    selectedObjectType.ToString() == "UnityEngine.Texture2D" ||
                    selectedObjectType.ToString() == "UnityEngine.MovieTexture" ||
                    selectedObjectType.ToString() == "UnityEngine.RenderTexture")
                {
                    s3DImageScriptableObject.sourceTexture = selectedObject as Texture;                    
                }
#if UNITY_5_6_PLUS
                // If it was a Video that was selected, we select that video in the new S3DImage.
                else if (selectedObjectType.ToString() == "UnityEngine.Video.VideoClip")
                {
                    s3DImageScriptableObject.videoClip = selectedObject as UnityEngine.Video.VideoClip;
                }
#endif
                // Cant do anything for 2 images or URLs sadly.
                // So anything else we ignore.
            }
        }
    }
}
#endif