/*
 * LayerManager
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VR3D
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class LayerManager
    {
        /// <summary>
        /// If you want to use a different named layer for your Left eye images, change this.
        /// </summary>
        private const string leftImageLayerName = "LeftImage";

        /// <summary>
        /// If you want to use a different named layer for your Right eye images, change this.
        /// </summary>
        private const string rightImageLayerName = "RightImage";

        /// <summary>
        /// Returns the index for the layer that we use for the Left eye images.
        /// </summary>
        public static int LeftLayerIndex
        {
            get { return LayerMask.NameToLayer(leftImageLayerName); }
        }

        /// <summary>
        /// Returns the index for the layer that we use for the Right eye images.
        /// </summary>
        public static int RightLayerIndex
        {
            get { return LayerMask.NameToLayer(rightImageLayerName); }
        }

#if UNITY_EDITOR
        /// <summary>
        /// We use [InitializeOnLoad] so we can call code when this script is first installed.
        /// </summary>
        static LayerManager()
        {
            // This is what we need to call when installed.
            CheckLayers();
        }

        /// <summary>
        /// This looks for layers whoes names are defined above, and if it cant find them it creates them.
        /// </summary>
        static void CheckLayers()
        {
            // Check the list for "LeftImage" and "RightImage"?
            bool foundLeft = false;
            bool foundRight = false;

            // While we look for a name match we save a reference to the first 2 unused layers so we cant use them ourselves if need be.
            SerializedProperty firstEmptyLayerProperty = null;
            SerializedProperty secondEmptyLayerProperty = null;

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProperty = tagManager.FindProperty("layers");

            // Something went wrong, so return early.
            if (layersProperty == null || !layersProperty.isArray) return;

            // Here we loop through all the layers to see if any of them are marked for our assets use.
            for (int i = 8; i < 32; i++) // The first 8 layers are reserved by Unity.
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(i);

                if (layerProperty.stringValue == leftImageLayerName) foundLeft = true;
                else if (layerProperty.stringValue == rightImageLayerName) foundRight = true;
                else if (firstEmptyLayerProperty == null && layerProperty.stringValue == string.Empty)
                    firstEmptyLayerProperty = layerProperty;
                else if (secondEmptyLayerProperty == null && layerProperty.stringValue == string.Empty)
                    secondEmptyLayerProperty = layerProperty;

                // If we found them, great! We dont need to do anything more here.
                if (foundLeft && foundRight) return;
            }

            // And those if they dont exist, we add them now.
            if (!foundLeft)
            {
                Debug.Log("[LayerManager] LeftImage layer not found. Adding...");
                firstEmptyLayerProperty.stringValue = leftImageLayerName;
            }
            if (!foundRight)
            {
                Debug.Log("[LayerManager] RightImage layer not found. Adding...");
                secondEmptyLayerProperty.stringValue = rightImageLayerName;
            }

            tagManager.ApplyModifiedProperties();
        }
#endif
    }
}