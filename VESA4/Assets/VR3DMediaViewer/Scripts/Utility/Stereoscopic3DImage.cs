/*
 * Stereoscopic3DImage
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

#if !UNITY_4 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_2 && !UNITY_5_4 && !UNITY_5_5
#define UNITY_5_6_PLUS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_5_6_PLUS
using UnityEngine.Video;
#endif

namespace VR3D
{
    /// <summary>
    /// Think of this as like a texture, but 3D specific. With all the 3D settings you cant supply on a normal texture to identify it as 3D.
    /// </summary>
    public class Stereoscopic3DImage : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The texture we will show in 3D. The left one if sourceImage2 is supplied.")]
        public Texture sourceTexture;

        [Tooltip("The texture we will show in 3D. The right side. Ignore if you just use one source texture.")]
        public Texture sourceTexture2;
#if UNITY_5_6_PLUS
        [Tooltip("A UnityEngine.Video.VideoClip format stereoscopic 3D video file.")]
        public VideoClip videoClip;

        [Tooltip("A URL to a stereoscopic 3D video file.")]
        public string videoURL = string.Empty;
#endif
        [Tooltip("What Stereoscopic 3D format does this media use?")]
        public ImageFormat imageFormat = ImageFormat.Side_By_Side;

        [Tooltip("How should this media be displayed?")]
        public CanvasFormat canvasFormat = CanvasFormat.Standard;

        [Tooltip("Adjusts the Y axis rotation for the output of the panoramic media.")]
        public float rotation = 0;

        [Tooltip("Swap the images. This is needed for images that are meant for cross-eyed viewing, and to compensate for a lack of Standards with Stereoscopic 3D image formats.")]
        public bool swapLeftRight = false;

        [Tooltip("Pixel value of how much to offset the images horizontally to allow for a different focal point along the Z-axis from the cameras perspective.")]
        public int convergence = 0;

        [Tooltip("Max range (+/-) that convergence can be adjusted in if Cropped mode is enabled.")]
        public int maxConvergence = 0;

        [Tooltip("Crop pixels from the image when creating convergence range, or let the texture wrap.")]
        public ConvergenceMode convergenceMode = ConvergenceMode.Cropped;

        [Tooltip("Overrides/sets the texture's TextureWrapMode at runtime.")]
        public TextureWrapMode3D wrapModeOverride = TextureWrapMode3D.Default;

        /// <summary>
        /// Some textures are mirrored vertically (not be be confused with up-side-down). This allows you to fix that.        
        /// </summary>
        [Tooltip("Mirror texture verically.")]
        public bool verticalFlip = false;

        [Tooltip("The color filter for the left eye when using Anaglyph format media.")]
        public Color leftEyeColor = Color.red;

        [Tooltip("The color filter for the right eye when using Anaglyph format media.")]
        public Color rightEyeColor = Color.cyan;

        /// <summary>
        /// Think of this as like a texture, but 3D specific. With all the 3D settings you cant supply on a normal texture to identify it as 3D.
        /// </summary>
        /// <param name="sourceTexture">A Texture, Texture2D, MovieTexture, RenderTexture, etc that has perspective for each eye of a 3D image.</param>
        /// <param name="imageFormat">The type of stereoscopic 3D image this is.</param>
        /// <param name="swapLeftRight">Swap the eyes so this image can be viewed cross-eyed, or fix images made for cross-eyed viewing.</param>
        /// <param name="convergence">How many picels to offset the images in each eye.</param>
        /// <param name="maxConvergence">How many vertical rows of pixels to crop to allow a convergence range.</param>
        /// <param name="verticalFlip">Vertically mirror the display of the texture.</param>
        /// <param name="leftEyeColor">If using Anaglyph media this is the color filter for the left eye.</param>
        /// <param name="rightEyeColor">If using Anaglyph media this is the color filter for the right eye.</param>
        public static Stereoscopic3DImage Create(Texture sourceTexture, ImageFormat imageFormat, bool swapLeftRight = false, int convergence = 0, int maxConvergence = 0, bool verticalFlip = false, Color? leftEyeColor = null, Color? rightEyeColor = null)
        {
            Stereoscopic3DImage newS3DImage = ScriptableObject.CreateInstance<Stereoscopic3DImage>();

            newS3DImage.sourceTexture = sourceTexture;
            newS3DImage.imageFormat = imageFormat;
            newS3DImage.swapLeftRight = swapLeftRight;
            newS3DImage.convergence = convergence;
            newS3DImage.maxConvergence = maxConvergence;
            newS3DImage.verticalFlip = verticalFlip;
            newS3DImage.leftEyeColor = leftEyeColor ?? Color.red;
            newS3DImage.rightEyeColor = rightEyeColor ?? Color.cyan;

            return newS3DImage;
        }

        /// <summary>
        /// Think of this as like a texture, but 3D specific. With all the 3D settings you cant supply on a normal texture to identify it as 3D.
        /// </summary>
        /// <param name="sourceTexture">A Texture, Texture2D, MovieTexture, RenderTexture, etc that has the perspective for the Left eye of a 3D image.</param>
        /// <param name="sourceTexture2">A Texture, Texture2D, MovieTexture, RenderTexture, etc that has the perspective for the Right eye of a 3D image.</param>
        /// <param name="imageFormat">The type of stereoscopic 3D image this is.</param>
        /// <param name="swapLeftRight">Swap the eyes so this image can be viewed cross-eyed, or fix images made for cross-eyed viewing.</param>
        /// <param name="convergence">How many picels to offset the images in each eye.</param>
        /// <param name="maxConvergence">How many vertical rows of pixels to crop to allow a convergence range.</param>
        /// <param name="verticalFlip">Vertically mirror the display of the texture.</param>
        /// <param name="leftEyeColor">If using Anaglyph media this is the color filter for the left eye.</param>
        /// <param name="rightEyeColor">If using Anaglyph media this is the color filter for the right eye.</param>
        public static Stereoscopic3DImage Create(Texture sourceTexture, Texture sourceTexture2, ImageFormat imageFormat, bool swapLeftRight = false, int convergence = 0, int maxConvergence = 0, bool verticalFlip = false, Color? leftEyeColor = null, Color? rightEyeColor = null)
        {
            Stereoscopic3DImage newS3DImage = ScriptableObject.CreateInstance<Stereoscopic3DImage>();

            newS3DImage.sourceTexture = sourceTexture;
            newS3DImage.sourceTexture2 = sourceTexture2;
            newS3DImage.imageFormat = imageFormat;
            newS3DImage.swapLeftRight = swapLeftRight;
            newS3DImage.convergence = convergence;
            newS3DImage.maxConvergence = maxConvergence;
            newS3DImage.verticalFlip = verticalFlip;
            newS3DImage.leftEyeColor = leftEyeColor ?? Color.red;
            newS3DImage.rightEyeColor = rightEyeColor ?? Color.cyan;

            return newS3DImage;
        }

#if UNITY_5_6_PLUS
        /// <summary>
        /// Think of this as like a texture, but 3D specific. With all the 3D settings you cant supply on a normal texture to identify it as 3D.
        /// </summary>
        /// <param name="videoClip">A UnityEngine.Video.VideoClip that has perspective for each eye of a 3D image.</param>
        /// <param name="imageFormat">The type of stereoscopic 3D image this is.</param>
        /// <param name="swapLeftRight">Swap the eyes so this image can be viewed cross-eyed, or fix images made for cross-eyed viewing.</param>
        /// <param name="convergence">How many picels to offset the images in each eye.</param>
        /// <param name="maxConvergence">How many vertical rows of pixels to crop to allow a convergence range.</param>
        /// <param name="verticalFlip">Vertically mirror the display of the texture.</param>
        /// <param name="leftEyeColor">If using Anaglyph media this is the color filter for the left eye.</param>
        /// <param name="rightEyeColor">If using Anaglyph media this is the color filter for the right eye.</param>
        public static Stereoscopic3DImage Create(VideoClip videoClip, ImageFormat imageFormat, bool swapLeftRight = false, int convergence = 0, int maxConvergence = 0, bool verticalFlip = false, Color? leftEyeColor = null, Color? rightEyeColor = null)
        {
            Stereoscopic3DImage newS3DImage = ScriptableObject.CreateInstance<Stereoscopic3DImage>();

            newS3DImage.videoClip = videoClip;
            newS3DImage.imageFormat = imageFormat;
            newS3DImage.swapLeftRight = swapLeftRight;
            newS3DImage.convergence = convergence;
            newS3DImage.maxConvergence = maxConvergence;
            newS3DImage.verticalFlip = verticalFlip;
            newS3DImage.leftEyeColor = leftEyeColor ?? Color.red;
            newS3DImage.rightEyeColor = rightEyeColor ?? Color.cyan;

            return newS3DImage;
        }

        /// <summary>
        /// Think of this as like a texture, but 3D specific. With all the 3D settings you cant supply on a normal texture to identify it as 3D.
        /// </summary>
        /// <param name="videoURL">A URL to a video file that has perspective for each eye of a 3D image.</param>
        /// <param name="imageFormat">The type of stereoscopic 3D image this is.</param>
        /// <param name="swapLeftRight">Swap the eyes so this image can be viewed cross-eyed, or fix images made for cross-eyed viewing.</param>
        /// <param name="convergence">How many picels to offset the images in each eye.</param>
        /// <param name="maxConvergence">How many vertical rows of pixels to crop to allow a convergence range.</param>
        /// <param name="verticalFlip">Vertically mirror the display of the texture.</param>
        /// <param name="leftEyeColor">If using Anaglyph media this is the color filter for the left eye.</param>
        /// <param name="rightEyeColor">If using Anaglyph media this is the color filter for the right eye.</param>
        /// <returns></returns>
        public static Stereoscopic3DImage Create(string videoURL, ImageFormat imageFormat, bool swapLeftRight = false, int convergence = 0, int maxConvergence = 0, bool verticalFlip = false, Color? leftEyeColor = null, Color? rightEyeColor = null)
        {
            Stereoscopic3DImage newS3DImage = ScriptableObject.CreateInstance<Stereoscopic3DImage>();

            newS3DImage.videoURL = videoURL;
            newS3DImage.imageFormat = imageFormat;
            newS3DImage.swapLeftRight = swapLeftRight;
            newS3DImage.convergence = convergence;
            newS3DImage.maxConvergence = maxConvergence;
            newS3DImage.verticalFlip = verticalFlip;
            newS3DImage.leftEyeColor = leftEyeColor ?? Color.red;
            newS3DImage.rightEyeColor = rightEyeColor ?? Color.cyan;

            return newS3DImage;
        }
#endif        

        /// <summary>
        /// Think of this as like a texture, but 3D specific. With all the 3D settings you cant supply on a normal texture to identify it as 3D.
        /// </summary>        
        public static Stereoscopic3DImage Create()
        {
            Stereoscopic3DImage newS3DImage = ScriptableObject.CreateInstance<Stereoscopic3DImage>();
            return newS3DImage;
        }
    }
}
