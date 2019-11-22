/*
 * Stereoscopic3DScreenshot
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

/*
 * This takes a Stereoscopic 3D screenshot of the current scene, and returns it as a texture or 2. What you do with the texture should be handled as required from another script.
 * 
 * To use this:
 * 	Place this script on your camera or object at the center of your players view.
 * 	Adjust the focalDistance to about what you would want for objects inbetween the Plane visual and the camera to appear to "pop out of the image" in the resulting screenshot.
 * 
 * Optional:
 * 	You can adjust the seperation if you want to change the distance between the left/right images.
 * 	
 * Notes:
 *  You can take Screenshots in multiple different Stereoscopic 3D formats, but generally you should just use Side-by-Side or Top/Bottom.
 *  The Interlaced and Checkerboard formats are easily ruined with different encoding.
 *  Two Images is just bulky.
*/

using UnityEngine;
using System.Collections;
using System.IO;

namespace VR3D
{
    public class Stereoscopic3DScreeenshot : MonoBehaviour
    {
        /// <summary>
        /// Image Formats available for this asset to take screenshots in.
        /// </summary>
        public enum ScreenShotImageFormat : int { Side_By_Side, Top_Bottom, TwoImages, HorizontalInterlaced, VerticalInterlaced, Checkerboard, Anaglyph };

        /// <summary>
        /// <para>Full = Make a image thats 2x the resolution, each eyes image being 1x. No quality loss.</para>
        /// <para>Half = Squash each eye down to half the resolution so both halves can fit inside the given resolution.</para>
        /// </summary>
        public enum ScreenShotImageSize : int { Full, Half };

        /// <summary>
        /// <para>Parallel = Essentiolly "do nothing". Both cameras focus at infinity.</para>
        /// <para>OffCenter = Like ToeIn, but better.</para>
        /// <para>ToeIn = Both cameras rotate to point towards the focal point. This helps nearer object pop in 3D, but can cause objects that appear near the edges of the image to not focus well.</para>
        /// </summary>
        public enum ProjectionMethod : int { Parallel, OffCenter, ToeIn };

        [Header("Main")]
        [Tooltip("The Stereoscopic 3D format you want the screenshot to be in.")]
        public ScreenShotImageFormat imageFormat = ScreenShotImageFormat.Side_By_Side;

        [Tooltip("Do you want a large but loss-less size screenshot, or a normal size but squashed one?")]
        public ScreenShotImageSize imageSize = ScreenShotImageSize.Full;

        [Tooltip("Sets the capture resolution of each half of the screenshot.")]
        public int resolutionWidth = 1920;

        [Tooltip("Sets the capture resolution of each half of the screenshot.")]
        public int resolutionHeight = 1080;

        [Tooltip("This controls the way the cameras focus to take the 3D screenshot. Generally OffCenter should be best.")]
        public ProjectionMethod projection = ProjectionMethod.OffCenter;

        [Tooltip("In millimeters.")]
        public float seperation = 64.0f;

        [Tooltip("The point to focus on. Things nearer then this point should appear as if in front of the picture.")]
        public float focalDistance = 1.0f;

        [Tooltip("The desired FOV for the screenshot.")]
        public float fieldOfView = 60;        

        [Header("Anaglyph")]
        [Tooltip("If your not taking an Anaglyph format screenshot, ignore this.")]
        public Color leftColor = Color.red;

        [Tooltip("If your not taking an Anaglyph format screenshot, ignore this.")]
        public Color rightColor = Color.cyan;

        /// <summary>
        /// Adjust this to zero if you dont want to see the frustrum planes for some reason...
        /// </summary>
        private const float FRUSTUM_PLANE_ALPHA = 0.25f;

        // Update is called once per frame
        void Update()
        {
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeObject == gameObject)
            {
                float frustumHeight = 2.0f * focalDistance * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
                float frustumWidth = frustumHeight * ((float)resolutionWidth / (float)resolutionHeight);

                Vector3 leftCameraPosition = transform.position + transform.right * -((seperation / 2) / 1000);
                Vector3 rightCameraPosition = transform.position + transform.right * ((seperation / 2) / 1000);

                // Draw some dots to represent camera positions.
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(leftCameraPosition, 0.01f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(rightCameraPosition, 0.01f);

                // A representation of the focal plane.
                Gizmos.color = new Color(1.0f, 0, 0, FRUSTUM_PLANE_ALPHA);
                Vector3 relativePosition = leftCameraPosition - ((projection == ProjectionMethod.ToeIn ? transform.position : leftCameraPosition) + (transform.TransformDirection(Vector3.forward) * focalDistance));
                DrawFrustumPlane(leftCameraPosition + (transform.TransformDirection(Vector3.forward) * focalDistance), Quaternion.LookRotation(relativePosition), new Vector3(frustumWidth, frustumHeight, 0));

                Gizmos.color = new Color(0, 1.0f, 1.0f, FRUSTUM_PLANE_ALPHA);
                relativePosition = rightCameraPosition - ((projection == ProjectionMethod.ToeIn ? transform.position : rightCameraPosition) + (transform.TransformDirection(Vector3.forward) * focalDistance));
                DrawFrustumPlane(rightCameraPosition + (transform.TransformDirection(Vector3.forward) * focalDistance), Quaternion.LookRotation(relativePosition), new Vector3(frustumWidth, frustumHeight, 0));
            }
        }
#endif

        /// <summary>
        /// Takes a screenshot of where this gameobject is facing, and returns it as textures in the ImageFormat thats selected.
        /// </summary>
        public void GetScreenshot(Texture2D tex, Texture2D tex2 = null)
        {
            switch (imageFormat)
            {
                case ScreenShotImageFormat.Side_By_Side:
                case ScreenShotImageFormat.Top_Bottom:
                    Get3DTextures(tex);
                    break;
                case ScreenShotImageFormat.TwoImages:
                    Get3DTextures(tex, tex2);
                    break;
                case ScreenShotImageFormat.HorizontalInterlaced:
                    Get3DTextures(tex);
                    tex = TopBottomToInterlacedTexture(tex);
                    break;
                case ScreenShotImageFormat.VerticalInterlaced:
                    Get3DTextures(tex);
                    tex = SBSToVerticalInterlacedTexture(tex);
                    break;
                case ScreenShotImageFormat.Checkerboard:
                    Get3DTextures(tex);
                    tex = SBSToCheckerboardTexture(tex);
                    break;
                case ScreenShotImageFormat.Anaglyph:
                    // Since the end result of an Anaglyph image is a normal sized image, it doesnt make sense to use ScreenShotImageSize.Half if its selected.
                    ScreenShotImageSize _imageSize = imageSize;
                    imageSize = ScreenShotImageSize.Full;

                    Get3DTextures(tex);
                    tex = SBSToAnaglyphTexture(tex, leftColor, rightColor);

                    imageSize = _imageSize;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Capture a 3D texture of where this GameObject is facing.
        /// </summary>
        /// <returns>Returns a Texture2D containing a Side-by-Side 3D Image.</returns>
        public void Get3DTextures(Texture2D texture, Texture2D texture2 = null)
        {
            // Create and get some temporary cameras.
            GameObject leftCameraGO = new GameObject("Left Camera");
            GameObject rightCameraGO = new GameObject("Right Camera");
            Camera leftCamera = leftCameraGO.AddComponent<Camera>();
            Camera rightCamera = rightCameraGO.AddComponent<Camera>();

            // Our temp cameras need to match the same position/rotation as the this camera.
            leftCameraGO.transform.parent = transform;
            rightCameraGO.transform.parent = transform;
            leftCameraGO.transform.localPosition = Vector3.zero;
            rightCameraGO.transform.localPosition = Vector3.zero;
            leftCameraGO.transform.localRotation = Quaternion.identity;
            rightCameraGO.transform.localRotation = Quaternion.identity;

            // Not sure if this is needed, but we dont need VR cameras.
            leftCamera.stereoTargetEye = StereoTargetEyeMask.None;
            rightCamera.stereoTargetEye = StereoTargetEyeMask.None;

            // User provided FOV.
            leftCamera.fieldOfView = fieldOfView;
            rightCamera.fieldOfView = fieldOfView;

            // Position the cameras aligned with this object.            
            leftCameraGO.transform.localPosition += new Vector3(1 * -((seperation / 2) / 1000), 0, 0);
            rightCameraGO.transform.localPosition += new Vector3(1 * ((seperation / 2) / 1000), 0, 0);

            switch (projection)
            {
                case ProjectionMethod.OffCenter:
                    // Skew the cameras.
                    // FYI, we use the left camera values for both since they are the same.
                    float fieldOfViewRadian = leftCamera.fieldOfView / 180.0f * Mathf.PI;
                    float aspectRatio = ((float)resolutionWidth / (float)resolutionHeight);
                    float a = leftCamera.nearClipPlane * Mathf.Tan(fieldOfViewRadian * 0.5f);
                    float b = leftCamera.nearClipPlane / focalDistance;

                    // Left camera                    
                    float left = -aspectRatio * a + ((seperation / 2) / 1000) * b;
                    float right = aspectRatio * a + ((seperation / 2) / 1000) * b;
                    leftCamera.projectionMatrix = PerspectiveOffCenter(left, right, -a, a, leftCamera.nearClipPlane, leftCamera.farClipPlane);
                    
                    // Right camera                    
                    left = -aspectRatio * a - ((seperation / 2) / 1000) * b;
                    right = aspectRatio * a - ((seperation / 2) / 1000) * b;
                    rightCamera.projectionMatrix = PerspectiveOffCenter(left, right, -a, a, rightCamera.nearClipPlane, rightCamera.farClipPlane);
                    break;
                case ProjectionMethod.ToeIn:
                    // Rotate the cameras towards the focal point.
                    leftCameraGO.transform.LookAt(transform.position + (transform.TransformDirection(Vector3.forward) * focalDistance));
                    rightCameraGO.transform.LookAt(transform.position + (transform.TransformDirection(Vector3.forward) * focalDistance));
                    break;
                default:
                    // Parallel. We dont need to do anything.
                    break;
            }            

            // Create and add some render textures to our temp cameras.
            leftCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
            rightCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);

            // Create our empty combined texture or textures.
            switch (imageFormat)
            {
                case ScreenShotImageFormat.Top_Bottom:
                case ScreenShotImageFormat.HorizontalInterlaced:
                    texture.Resize(resolutionWidth, resolutionHeight * 2);                    
                    break;
                case ScreenShotImageFormat.TwoImages:
                    texture.Resize(resolutionWidth, resolutionHeight);
                    texture2.Resize(resolutionWidth, resolutionHeight);
                    break;
                default: // Side-by-Side based
                    texture.Resize(resolutionWidth * 2, resolutionHeight);
                    break;
            }

            // Capture the left view.
            RenderTexture.active = leftCamera.targetTexture;
            leftCamera.Render();                        
            texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);

            // Capture the right view.
            RenderTexture.active = rightCamera.targetTexture;
            rightCamera.Render();

            switch (imageFormat)
            {   
                case ScreenShotImageFormat.Top_Bottom:
                case ScreenShotImageFormat.HorizontalInterlaced:
                    texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, resolutionHeight);
                    texture.Apply();
                    break;
                case ScreenShotImageFormat.TwoImages:
                    texture2.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
                    texture.Apply();
                    break;
                default: // Side-by-Side based
                    texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), resolutionWidth, 0);
                    texture.Apply();
                    break;
            }

            if (imageSize == ScreenShotImageSize.Half)
                ResizeTexture(texture, resolutionWidth, resolutionHeight);

            RenderTexture.active = null;

            // Clean up.
            Destroy(leftCameraGO);
            Destroy(rightCameraGO);
        }

        /// <summary>
        /// Adjusts the size of the texture, squashing/stretching as needed.
        /// </summary>
        /// <param name="tex">The texture being resized. After this finishes the new textures content will be available with the same variable.</param>
        /// <param name="width">The width of the resized texture. Can be the same size.</param>
        /// <param name="height">The height of the resized texture. Can be the same size.</param>
        private void ResizeTexture(Texture2D tex, int width, int height)
        {
            // We resize by copying pixels 1-by-1.
            Color[] oldPixels = tex.GetPixels();
            Color[] newPixels = new Color[width * height];

            // Get the relative size ratio of the new resized texture compared to the old version.
            Vector2 newSizeRatio = new Vector2(1, 1);
            newSizeRatio.x /= ((float)width / (tex.width - 1));
            newSizeRatio.y /= ((float)height / (tex.height - 1));

            for (int y = 0; y < height; y++)
            {   
                // Since were not doing a 1-1 copy of the pixels we need figure out which y pixel we're copying from.
                int sourceY = (int)(newSizeRatio.y * y) * tex.width;                

                int newYIndex = y * width;

                for (int x = 0; x < width; x++)
                {
                    // Since were not doing a 1-1 copy of the pixels we need figure out which x pixel we're copying from.
                    int sourceX = (int)(newSizeRatio.x * x);                    
                    
                    // Copy the pixel!
                    newPixels[x + newYIndex] = oldPixels[sourceX + sourceY];
                }
            }

            // The texture needs to be of the new size now. This alone wouldn't do it and instead just make a blank texture.
            tex.Resize(width, height);

            // Apply the pixels of the resized texture back into the texture.
            tex.SetPixels(newPixels);
            tex.Apply();
        }

        #region Helper Functions

        private Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
        {
            float x = 2.0F * near / (right - left);
            float y = 2.0F * near / (top - bottom);
            float a = (right + left) / (right - left);
            float b = (top + bottom) / (top - bottom);
            float c = -(far + near) / (far - near);
            float d = -(2.0F * far * near) / (far - near);
            float e = -1.0F;

            Matrix4x4 m = new Matrix4x4();
            m[0, 0] = x;
            m[0, 1] = 0;
            m[0, 2] = a;
            m[0, 3] = 0;
            m[1, 0] = 0;
            m[1, 1] = y;
            m[1, 2] = b;
            m[1, 3] = 0;
            m[2, 0] = 0;
            m[2, 1] = 0;
            m[2, 2] = c;
            m[2, 3] = d;
            m[3, 0] = 0;
            m[3, 1] = 0;
            m[3, 2] = e;
            m[3, 3] = 0;

            return m;
        }

        private void DrawFrustumPlane(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 planeTransform = Matrix4x4.TRS(position, rotation, scale);
            Matrix4x4 oldMatrix = Gizmos.matrix;

            Gizmos.matrix *= planeTransform;

            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = oldMatrix;
        }

        #endregion // Helper Functions

        #region Format Conversion Functions

        /// <summary>
        /// Convert a Horizontal-Interlaced format Stereoscopic 3D texture to a Top/Bottom format Stereoscopic 3D texture.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Horizontal-Interlaced format Stereoscopic 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Horizontal-Interlaced format Stereoscopic 3D texture.</param>
        /// <returns>A Top/Bottom format Stereoscopic 3D texture.</returns>
        public static Texture2D InterlacedToTopBottomTexture(Texture2D sourceTexture)
        {
            // The new texture is the same size. We convert Horizontal-Interlaced textures to T/B rather than SBS as doing so
            // wouldn't require squashing/stretching the image, and can be done loss-less with a 1-1 pixel copy.
            Texture2D destTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

            // Writing the second image to the texture is done so on the bottom half, so the pixels will always have (sourceTexture.height / 2) added to their y position.
            int offset = sourceTexture.height / 2;

            // Move down to the next y row after each x pixel in that row is copied.
            for (int y = 0; y < sourceTexture.height; y++)
            {
                // Move on to the next x pixel after the previous one in that row is copied.
                for (int x = 0; x < sourceTexture.width; x++)
                {
                    // !!! Note for developers: The order of the rows might seem reversed. 
                    // !!! It's set the way it is due to most Horizontal Interlaced images I could find online having the odd rows being for the right eye image, and even for the left.
                    // !!! If this interpertation of the format doesnt suit you, change the "1" in the below if statement to a "0".

                    // This is a check to see if we are on a Even or Odd row.                
                    if (y % 2 == 1) // Copy pixels for the top image from odd rows.
                        destTexture.SetPixel(x, y / 2, sourceTexture.GetPixel(x, y));
                    else // Copy pixels for the bottom image from even rows.
                        destTexture.SetPixel(x, (offset + (y / 2)), sourceTexture.GetPixel(x, y));
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            return destTexture;
        }

        /// <summary>
        /// Convert a Top/Bottom format Stereoscopic 3D texture to a Horizontal-Interlaced format Stereoscopic 3D texture.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Top/Bottom format Stereoscopic 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Top/Bottom format Stereoscopic 3D texture.</param>
        /// <returns>A Horizontal-Interlaced format Stereoscopic 3D texture.</returns>
        public static Texture2D TopBottomToInterlacedTexture(Texture2D sourceTexture)
        {
            Texture2D destTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

            // Reading the second image from the sourceTexture is done so from the bottom half, so the pixels will always have (sourceTexture.height / 2) added to their y position.
            int offset = sourceTexture.height / 2;

            // Move down to the next y row after each x pixel in that row is copied.
            for (int y = 0; y < sourceTexture.height; y++)
            {
                // Move on to the next x pixel after the previous one in that row is copied.
                for (int x = 0; x < sourceTexture.width; x++)
                {
                    // !!! Note for developers: The order of the rows might seem reversed. 
                    // !!! It's set the way it is due to most Horizontal Interlaced images I could find online having the odd rows being for the right eye image, and even for the left.
                    // !!! If this interpertation of the format doesnt suit you, change the "1" in the below if statement to a "0".

                    // This is a check to see if we are on a Even or Odd row.
                    if (y % 2 == 1) // Copy pixels from the top image to odd rows.
                        destTexture.SetPixel(x, y, sourceTexture.GetPixel(x, y / 2));
                    else // Copy pixels from the bottom image to even rows.
                        destTexture.SetPixel(x, y, sourceTexture.GetPixel(x, (offset + (y / 2))));
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            // Encode the texture into PNG
            //byte[] bytes = destTexture.EncodeToPNG();        

            // Save the PNG file to disk in the applications directory.
            //File.WriteAllBytes(Path.GetFullPath(Application.dataPath + "/../") + Application.productName + " - " + 1 + ".png", bytes);

            return destTexture;
        }

        /// <summary>
        /// Convert a Vertical-Interlaced format Stereoscopic 3D texture to a Side-by-Side format Stereoscopic 3D texture.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Vertical-Interlaced format Stereoscopic 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Vertical-Interlaced format Stereoscopic 3D texture.</param>
        /// <returns>A Side-by-Side format Stereoscopic 3D texture.</returns>
        public static Texture2D VerticalInterlacedToSBSTexture(Texture2D sourceTexture)
        {
            // The new texture is the same size. We convert VI textures to SBS rather than T/B as doing so
            // wouldn't require squashing/stretching the image, and can be done loss-less with a 1-1 pixel copy.
            Texture2D destTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

            // Writing the second image to the texture is done so on the right half, so the pixels will always have (sourceTexture.width / 2) added to their x position.
            int offset = sourceTexture.width / 2;

            // Move right to the next x column after each y pixel in that column is copied.
            for (int x = 0; x < sourceTexture.width; x++)
            {
                // Move on to the next y pixel after the previous one in that column is copied.
                for (int y = 0; y < sourceTexture.height; y++)
                {
                    // !!! Note for developers: The order of the columns might seem reversed. 
                    // !!! It's set the way it is due to most Horizontal Interlaced images I could find online having the odd rows being for the right eye image, and even for the left.
                    // !!! I couldn't find many Vertical Interlaced images, so I figured I would make the default consistant with the Horizontal standard this script uses.
                    // !!! If this interpertation of the format doesnt suit you, change the "1" in the below if statement to a "0".

                    // This is a check to see if we are on a Even or Odd column.
                    if (x % 2 == 1) // Copy pixels for the left image from even columns.
                        destTexture.SetPixel(x / 2, y, sourceTexture.GetPixel(x, y));
                    else // Copy pixels for the right image from odd columns.
                        destTexture.SetPixel((offset + (x / 2)), y, sourceTexture.GetPixel(x, y));
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            return destTexture;
        }

        /// <summary>
        /// Convert a Side-by-Side format Stereoscopic 3D texture to a Vertical-Interlaced format Stereoscopic 3D texture.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Side-by-Side format Stereoscopic 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Side-by-Side format Stereoscopic 3D texture.</param>
        /// <returns>A Vertical-Interlaced format Stereoscopic 3D texture.</returns>
        public static Texture2D SBSToVerticalInterlacedTexture(Texture2D sourceTexture)
        {
            Texture2D destTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

            // Reading the second image from the sourceTexture is done so from the right half, so the pixels will always have (sourceTexture.width / 2) added to their x position.
            int offset = sourceTexture.width / 2;

            // Move right to the next x column after each y pixel in that column is copied.
            for (int x = 0; x < sourceTexture.width; x++)
            {
                // Move on to the next y pixel after the previous one in that column is copied.
                for (int y = 0; y < sourceTexture.height; y++)
                {
                    // !!! Note for developers: The order of the columns might seem reversed. 
                    // !!! It's set the way it is due to most Horizontal Interlaced images I could find online having the odd rows being for the right eye image, and even for the left.
                    // !!! I couldn't find many Vertical Interlaced images, so I figured I would make the default consistant with the Horizontal standard this script uses.
                    // !!! If this interpertation of the format doesnt suit you, change the "1" in the below if statement to a "0".

                    // This is a check to see if we are on a Even or Odd column.
                    if (x % 2 == 1) // Copy pixels from the left image to even columns.
                        destTexture.SetPixel(x, y, sourceTexture.GetPixel(x / 2, y));
                    else // Copy pixels from the right image to odd columns.
                        destTexture.SetPixel(x, y, sourceTexture.GetPixel((offset + (x / 2)), y));
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            return destTexture;
        }

        /// <summary>
        /// Convert a Checkerboard format Stereoscopic 3D texture to a Side-by-Side format Stereoscopic 3D texture.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Checkerboard format Stereoscopic 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Checkerboard format Stereoscopic 3D texture.</param>
        /// <returns>A Side-by-Side format Stereoscopic 3D texture.</returns>
        public static Texture2D CheckerboardToSBSTexture(Texture2D sourceTexture)
        {
            // The new texture is the same size. We convert CB textures to SBS rather than T/B as doing so
            // wouldn't require squashing/stretching the image, and can be done loss-less with a 1-1 pixel copy.
            Texture2D destTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

            // Writing the second image to the texture is done so on the right half, so the pixels will always have (sourceTexture.width / 2) added to their x position.
            int offset = sourceTexture.width / 2;

            // Move down to the next y row after each x pixel in that row is copied.
            for (int y = 0; y < sourceTexture.height; y++)
            {
                // Move on to the next x pixel after the previous one in that row is copied.
                for (int x = 0; x < sourceTexture.width; x++)
                {
                    // This is a check to see if we are on a Even or Odd pixel/row.
                    if (x % 2 == 0 && y % 2 == 0 || x % 2 != 0 && y % 2 != 0) // Copy pixels for the left image from even pixels on even rows, and odd pixels on odd rows.
                        destTexture.SetPixel(x / 2, y, sourceTexture.GetPixel(x, y));
                    else // Copy pixels for the right image from odd pixels on even rows, and even pixels on odd rows.
                        destTexture.SetPixel((offset + (x / 2)), y, sourceTexture.GetPixel(x, y));
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            return destTexture;
        }

        /// <summary>
        /// Convert a Side-by-Side format Stereoscopic 3D texture to a Checkerboard format Stereoscopic 3D texture.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Side-by-Side format Stereoscopic 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Side-by-Side format Stereoscopic 3D texture.</param>
        /// <returns>A Checkerboard format Stereoscopic 3D texture.</returns>
        public static Texture2D SBSToCheckerboardTexture(Texture2D sourceTexture)
        {
            Texture2D destTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

            // Reading the second image from the sourceTexture is done so from the right half, so the pixels will always have (sourceTexture.width / 2) added to their x position.
            int offset = sourceTexture.width / 2;

            // Move down to the next y row after each x pixel in that row is copied.
            for (int y = 0; y < sourceTexture.height; y++)
            {
                // Move on to the next x pixel after the previous one in that row is copied.
                for (int x = 0; x < sourceTexture.width; x++)
                {
                    // This is a check to see if we are on a Even or Odd pixel/row.
                    if (x % 2 == 0 && y % 2 == 0 || x % 2 != 0 && y % 2 != 0) // Copy pixels from the left image to even pixels on even rows, and odd pixels on odd rows.
                        destTexture.SetPixel(x, y, sourceTexture.GetPixel(x / 2, y));
                    else // Copy pixels from the right image to odd pixels on even rows, and even pixels on odd rows.
                        destTexture.SetPixel(x, y, sourceTexture.GetPixel((offset + (x / 2)), y));
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            return destTexture;
        }

        /// <summary>
        /// Convert a Red/Cyan Anaglyph format 3D texture to a Side-by-Side format Stereoscopic 3D texture.
        /// This can't restore color data that's lost in the creation of anaglyph images! General anaglyph image creation involves completely removing the red channel for the image to be seen by the left eye, and the blue/green channels for the image thats seen by the right.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Red\Cyan Anaglyph format 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Red/Cyan Anaglyph format 3D texture.</param>
        /// <param name="makeMonochrome">Option to make the resulting texture Monochrome. Otherwise it will still be Red/Cyan for each eye resulting in a contrast for each eye that MAY cause eye strain.
        /// The effect is similar to wearing Red/Cyan glasses, but with the benefit of not having everything else you look at besides the image filtered through colors.</param>
        /// <returns>A Side-by-Side format Stereoscopic 3D texture.</returns>
        public static Texture2D AnaglyphToSBSTexture(Texture2D sourceTexture, Color leftColor, Color rightColor, bool makeMonochrome = false)
        {
            Texture2D destTexture = new Texture2D(sourceTexture.width * 2, sourceTexture.height);

            // Writing the second image to the texture is done so on the right half, so the pixels will always have (sourceTexture.width / 2) added to their x position.
            int offset = sourceTexture.width;

            // Move down to the next y row after each x pixel in that row is copied.
            for (int y = 0; y < sourceTexture.height; y++)
            {
                // Move on to the next x pixel after the previous one in that row is copied.
                for (int x = 0; x < sourceTexture.width; x++)
                {
                    // Get the pixel and remove the red.
                    Color pixelColor = sourceTexture.GetPixel(x, y);
                                        
                    pixelColor.r = (leftColor.r > 0 ? pixelColor.r : 0);
                    pixelColor.b = (leftColor.b > 0 ? pixelColor.b : 0);
                    pixelColor.g = (leftColor.g > 0 ? pixelColor.g : 0);

                    if (makeMonochrome)
                    {
                        // Convert it to greyScale.
                        float grayScale = pixelColor.grayscale;
                        pixelColor = new Color(grayScale, grayScale, grayScale, pixelColor.a);
                    }

                    // Copy it to the new texture.
                    destTexture.SetPixel(x, y, pixelColor);

                    // Get the pixel and remove the blue and green.
                    pixelColor = sourceTexture.GetPixel(x, y);
                    
                    pixelColor.r = (rightColor.r > 0 ? pixelColor.r : 0);
                    pixelColor.b = (rightColor.b > 0 ? pixelColor.b : 0);
                    pixelColor.g = (rightColor.g > 0 ? pixelColor.g : 0);

                    if (makeMonochrome)
                    {
                        // Convert it to greyScale.
                        float grayScale = pixelColor.grayscale;
                        pixelColor = new Color(grayScale, grayScale, grayScale, pixelColor.a);
                    }

                    // Copy it to the new texture.
                    destTexture.SetPixel((offset + x), y, pixelColor);
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            return destTexture;
        }

        /// <summary>
        /// Convert a Side-by-Side format Stereoscopic 3D texture to a Anaglyph format Stereoscopic 3D texture.
        /// No scan of the sourceTexture is performed to confirm that it is indead a Side-by-Side format Stereoscopic 3D texture.
        /// </summary>
        /// <param name="sourceTexture">A Side-by-Side format Stereoscopic 3D texture.</param>
        /// <param name="leftColor">The color filter for the Left eye.</param>
        /// <param name="rightColor">The color filter for the Right eye.</param>
        /// <returns></returns>
        public static Texture2D SBSToAnaglyphTexture(Texture2D sourceTexture, Color leftColor, Color rightColor)
        {
            Texture2D destTexture = new Texture2D(sourceTexture.width / 2, sourceTexture.height);

            // Reading the second image from the sourceTexture is done so from the right half, so the pixels will always have (sourceTexture.width / 2) added to their x position.
            int offset = sourceTexture.width / 2;

            // Move down to the next y row after each x pixel in that row is copied.
            for (int y = 0; y < sourceTexture.height; y++)
            {
                // Move on to the next x pixel after the previous one in that row is copied.
                for (int x = 0; x < sourceTexture.width / 2; x++)
                {
                    // Get the pixel and remove the red.
                    Color leftPixelColor = sourceTexture.GetPixel(x, y);
                    Color rightPixelColor = sourceTexture.GetPixel(x + offset, y);

                    // This will be our final color. We just enforce an Alpha of 1.0f for now.
                    Color pixelColor = Color.black;

                    // If the color channel of 1 given eye color is greater then the other, we use the greaters corosponding side channels value. If they are equal we use the default of 0.
                    if (leftColor.r > rightColor.r) pixelColor.r = leftPixelColor.r;
                    else if (rightColor.r > leftColor.r) pixelColor.r = rightPixelColor.r;

                    if (leftColor.g > rightColor.g) pixelColor.g = leftPixelColor.g;
                    else if (rightColor.g > leftColor.g) pixelColor.g = rightPixelColor.g;

                    if (leftColor.b > rightColor.b) pixelColor.b = leftPixelColor.b;
                    else if (rightColor.b > leftColor.b) pixelColor.b = rightPixelColor.b;

                    // Copy it to the new texture.                
                    destTexture.SetPixel(x, y, pixelColor);
                }
            }

            // The finished texture needs to be saved before it will show.
            destTexture.Apply();

            return destTexture;
        }

        #endregion // Format Conversion Functions
    }
}

