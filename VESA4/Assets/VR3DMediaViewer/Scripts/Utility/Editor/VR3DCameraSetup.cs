/*
 * VR3DCameraSetup
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

namespace VR3D
{
    /// <summary>
    /// This adds context menus to Camera components, whoes purpose is to turn a stock VR camera into 2 in which each are deticated to each eye, and thus can have different settings.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Camera), true)]
    public class VR3DCameraSetup : Editor
    {
        private static string[] ignoreList = { "UnityEngine.Camera", "UnityEngine.GUILayer", "UnityEngine.FlareLayer" };

        private Camera m_camera;
        private Editor m_editor;

        public bool preserveComponents = false;

        public override void OnInspectorGUI()
        {
            if (m_camera == null)
                m_camera = (Camera)target;

            if (m_editor == null)
            {
                Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly assembly in assemblies)
                {
                    System.Type type = assembly.GetType("UnityEditor.CameraEditor");

                    if (type != null)
                    {
                        m_editor = Editor.CreateEditor(target, type);
                        break;
                    }
                }
            }

            if (m_editor != null)
            {
                m_editor.OnInspectorGUI();
                                
                if (!CheckCameras())
                {
                    // This is just a visual sperator.
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    // We color this area of the inspector different to draw attention to it.
                    GUIStyle areaStyle = new GUIStyle();

                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, new Color(0.0f, 1.0f, 1.0f, 0.5f)); // Cyan with half opacity.
                    tex.Apply();

                    areaStyle.normal.background = tex;

                    GUILayout.BeginVertical(areaStyle);

                    // We make the areas text label red to also help draw attention to it.
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
                    labelStyle.normal.textColor = Color.red;
                    GUILayout.Label("VR3DMediaViewer Camera Setup", labelStyle);

                    GUILayout.Space(10);

                    preserveComponents = GUILayout.Toggle(preserveComponents, "Perserve components for both eyes");

                    if (GUILayout.Button("Make 3D Camera"))
                    {
                        SetupCameras((Camera)target, preserveComponents);
                    }

                    GUILayout.EndVertical();
                }
            }
        }


        /// <summary>
        /// Takes a single camera and splits it into 2 cameras, each deticated to a single eye. Both cameras are automatically set up to work with VR3DMediaViewer. Any components that are non-standard for a camera are placed on the left eyes camera.
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/Camera/VR3DMediaViewer Camera Setup")]
        static void SetupCamera(MenuCommand command)
        {
            Camera camera = (Camera)command.context;
            SetupCameras(camera, false);

            Debug.LogWarning("VR3DMediaViewer: Any non-standard camera components like scripts, that were on the original camera are now on the \"-Left\" camera. You may need to check over each component to make sure it's where it makes the most sence.");
        }

        /// <summary>
        /// Takes a single camera and splits it into 2 cameras, each deticated to a single eye. Both cameras are automatically set up to work with VR3DMediaViewer. Any components that are non-standard for a camera are placed on both eyes cameras.
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/Camera/VR3DMediaViewer Camera Setup - Perserve Components for Both Eyes")]
        static void SetupCamera2(MenuCommand command)
        {
            Camera camera = (Camera)command.context;
            SetupCameras(camera, true);

            Debug.LogWarning("VR3DMediaViewer: Any non-standard camera components like scripts, that were on the original camera are now on the \"-Left\" & \"-Right\" cameras. You may need to check over each component to make sure it's where it makes the most sence.");
        }

        /// <summary>
        /// The guts of the above functions.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="copyScripts">Copy scripts to both cameras or just one?</param>
        static private void SetupCameras(Camera camera, bool copyScripts)
        {
            //Camera leftCamera = (Camera)command.context;
            Camera leftCamera = camera;
            string cameraSourceName = leftCamera.name; 
            GameObject leftCameraObject = leftCamera.gameObject;

            Undo.RecordObject(leftCameraObject, leftCamera.name + " Changed");

            // The SteamVR camera rig uses a wierd script that maintains an camera hierarchy that doesnt work well with a split camera.
            // So we call its own collapse method, and remove the problem script first.
            if (leftCameraObject.GetComponent("SteamVR_Camera"))
            {
                leftCameraObject.GetComponent("SteamVR_Camera").SendMessage("Collapse");
                DestroyImmediate(leftCameraObject.GetComponent("SteamVR_Camera"));
            }

            // We make a copy of the camera, and this new copy will be for the right eye.
            GameObject rightCameraObject = GameObject.Instantiate(leftCameraObject, leftCameraObject.transform.parent);
            Camera rightCamera = rightCameraObject.GetComponent<Camera>();

            Undo.RegisterCreatedObjectUndo(rightCameraObject, "Create " + rightCameraObject);

            // Name these cameras for their purposes.
            leftCamera.name = cameraSourceName + "-Left";
            rightCamera.name = cameraSourceName + "-Right";

            // Set the camera to only render for their designated eyes.
            leftCamera.stereoTargetEye = StereoTargetEyeMask.Left;
            rightCamera.stereoTargetEye = StereoTargetEyeMask.Right;

            // Set these cameras to exclude seeing the other eyes images.
            leftCamera.cullingMask &= ~(1 << LayerManager.RightLayerIndex); // Everything except the right layer. 
            rightCamera.cullingMask &= ~(1 << LayerManager.LeftLayerIndex); // Everything except the left layer.

            if (!copyScripts) ClearBehaviors(rightCameraObject);

            // Dont need more then one audio listener.
            if (rightCameraObject.GetComponent<AudioListener>())
                DestroyImmediate(rightCameraObject.GetComponent<AudioListener>());

            // If the source game object had any children, we remove their copys from the right camera.
            ClearChildren(rightCameraObject);
        }

        /// <summary>
        /// Checks if a given behaviour is in a ignore list.
        /// </summary>
        /// <param name="behaviour">A behaviour you want to see if is suposed to be ignored.</param>
        /// <returns>True if the behaviour is ignored.</returns>
        private static bool BehaviourIgnore(Behaviour behaviour)
        {
            foreach (string ignoredBehavior in ignoreList)
                if (behaviour.GetType().ToString() == ignoredBehavior) return true;

            return false;
        }

        /// <summary>
        /// Removes all non-ignored behaviors from the given GameObject.
        /// </summary>
        /// <param name="targetGameObject">The GameObject to scan.</param>
        private static void ClearBehaviors(GameObject targetGameObject)
        {
            // We don't need it to have any scripts.            
            Behaviour[] behaviours = targetGameObject.GetComponents<Behaviour>();

            foreach (Behaviour behaviour in behaviours)
                if (!BehaviourIgnore(behaviour)) DestroyImmediate(behaviour);
        }

        /// <summary>
        /// Removes all children from the given GameObject.
        /// </summary>
        /// <param name="targetGameObject">The GameObject to scan.</param>
        private static void ClearChildren(GameObject targetGameObject)
        {
            foreach (Transform child in targetGameObject.transform)
                DestroyImmediate(child.gameObject);
        }

        /// <summary>
        /// Check the camera to see if its already set for 3D.
        /// </summary>
        bool CheckCameras()
        {
            if ((m_camera.stereoTargetEye == StereoTargetEyeMask.Left &&
                m_camera.cullingMask == ~(1 << VR3D.LayerManager.RightLayerIndex)) ||
                (m_camera.stereoTargetEye == StereoTargetEyeMask.Right &&
                m_camera.cullingMask == ~(1 << VR3D.LayerManager.LeftLayerIndex)) ||
                m_camera.gameObject.name.Contains("Example Cross-eyed Camera Rig") ||
                m_camera.transform.parent.name.Contains("Example Cross-eyed Camera Rig")) // Exclude our assets example cross-eyed camera rigs.
                return true;

            return false;
        }
    }
}