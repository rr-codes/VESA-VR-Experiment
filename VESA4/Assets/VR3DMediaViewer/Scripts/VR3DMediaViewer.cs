/*
 * VR3DMediaViewer
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

#if !UNITY_4 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_2 && !UNITY_5_4 && !UNITY_5_5
#define UNITY_5_6_PLUS
#endif

// Comment this out if you need/want to see the right image object. We just hide it to keep things looking clean and simple.
#define HIDE_RIGHT

// Comment this out if you need/want to see the VideoPlayer component this script adds when it doesnt find one. We just hide it to keep things looking clean and simple.
#define HIDE_VIDEOPLAYER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_5_6_PLUS
using UnityEngine.Video;
#endif

namespace VR3D
{
    /// <summary>
    /// Image Formats available for this asset to display textures in. 3D, 2D and Monoscopic.
    /// <para>2D = Standard image. No 3D formating is applied.</para>
    /// <para>Side-by-Side = 3D format. 2 images from slightly different perspectives placed side-by-side.</para>
    /// <para>Top/Bottom = 3D format. 2 images from slightly different perspectives placed one on top of the other.</para>
    /// <para>Two Images = 3D format. 2 images from slightly different perspectives in different files.</para>
    /// <para>Horizontal Interlaced = 3D format. 2 images from slightly different perspectives interwoven with horizontal lines of pixels.</para>
    /// <para>Vertical Interlaced = 3D format. 2 images from slightly different perspectives interwoven with vertical lines of pixels.</para>
    /// <para>Checkerboard = 3D format. 2 images from slightly different perspectives interwoven with a checkerboard pattern of pixels.</para>
    /// <para>Anaglyph = 3D format. 2 images from slightly different perspectives with opposing RGB color channels removed, overlayed on top of each other.</para>
    /// <para>Mono = A version of the named 3D format, but using the image for a single eye for both eyes so no 3D effect can be seen.</para>    
    /// </summary>
    public enum ImageFormat : int { _2D, Side_By_Side, Top_Bottom, TwoImages, HorizontalInterlaced, VerticalInterlaced, Checkerboard, Anaglyph, Mono_Side_By_Side, Mono_Top_Bottom, Mono_TwoImages, Mono_HorizontalInterlaced, Mono_VerticalInterlaced, Mono_Checkerboard, Mono_Anaglyph };

    /// <summary>
    /// 3D Image Types this asset can use.
    /// <para>Standard = A normal square, generally around 60 degree FOV 3D image.</para>
    /// <para>360 = A 360 degree panoramic image.</para>
    /// <para>180 = A 180 degree panoramic image.</para>
    /// </summary>
    public enum CanvasFormat : int { Standard, _360, _180 };

    /// <summary>
    /// Override used for general easy access, but mostly for textures generated at runtime like withe VideoClips.
    /// <para>Default = Don't apply our own selection, and instead use whatever is already set to the texture.</para>
    /// </summary>
    public enum TextureWrapMode3D : int { Default = -1, Repeat = 0, Clamp = 1 };

    /// <summary>
    /// How the converge is handled.
    /// <para>Cropped = Shave pixels for each unit of MaxConvergence and retain a bounds for adjustment.</para>
    /// <para>Tiled = Shift the texture and let the tiling of the texture warp around to the other side.</para>
    /// </summary>
    public enum ConvergenceMode : int { Cropped, Tiled };

    /// <summary>
    /// Show Stereoscopic 3D Images on a GameObjects mesh.
    /// </summary>
    public class VR3DMediaViewer : MonoBehaviour 
    {
        [Header("Canvas")]
        [Tooltip("This stereoscopic 3D image will be displayed immediately on start.")]
        public Stereoscopic3DImage defaultImage;

        [Tooltip("A list of texture shader properties in which we assign our stereoscopic 3D images.")]
        public string[] targetTextureMaps = { "_MainTex" };

        private DisplayCanvas m_displayCanvas = null;

        /// <summary>
        /// When we load a new image into this canvas we save it so we can reference its settings. FYI, changing the settings in editor at runtime applies those settings to the S3DImage in the project heirarcy. Conveinent!
        /// </summary>
        public Stereoscopic3DImage currentImage { get; private set; }
                
        /// <summary>
        /// The Image Format of the S3DImage currently being displayed int he canvas.
        /// </summary>
        public ImageFormat ImageFormat
        {
            get 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage != null)
                    return currentImage.imageFormat;
                else
                    return ImageFormat._2D; // If theres no S3D image, we display the image as a normal texture.
            }
            private set { currentImage.imageFormat = value; }
        }

        /// <summary>
        /// Controls the type of canvas we are using. Standard, or Panoramic specific.
        /// </summary>
        public CanvasFormat CanvasFormat
        {
            get
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage != null)
                    return currentImage.canvasFormat;
                else
                    return CanvasFormat.Standard; // If theres no S3D image, we display the image as a normal texture.
            }
            private set
            {
                currentImage.canvasFormat = value;

                if (Application.isPlaying)
                    //Update3DTypeSettings();
                    m_displayCanvas.CanvasFormat = currentImage.canvasFormat;
            }
        }

        /// <summary>
        /// Rotation value for panoramic media.
        /// </summary>
        public float Rotation
        {
            get
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage != null)
                    return currentImage.rotation;
                else
                    return 0; // If theres no S3D image, we dont add any convergence.
            }
            set
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage == null) return;
                
                currentImage.rotation = value;

                if (currentImage.rotation < -359) currentImage.rotation += 360;
                else if (currentImage.rotation > 359) currentImage.rotation -= 360;

                if (Application.isPlaying)
                    //Update3DTypeSettings();
                    m_displayCanvas.Rotation = currentImage.rotation;
            }
        }

        /// <summary>
        /// The Swap Left & Right value of the S3DImage currently being displayed int he canvas.
        /// </summary>
        public bool SwapLeftRight
        {   
            get 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage != null)
                    return currentImage.swapLeftRight;
                else
                    return false; // If theres no S3D image, we display don't swap anything.
            }
            set
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage == null) return;

                currentImage.swapLeftRight = value;

                if (Application.isPlaying)
                {
                    // If not using 2 images we just swap eyes by swapping the material offsets.
                    if (currentImage.imageFormat != ImageFormat.TwoImages &&
                        currentImage.imageFormat != ImageFormat.Mono_TwoImages)
                        SetMaterialOffset();
                    else
                    {

                        m_displayCanvas.LeftTexture = (currentImage.swapLeftRight ? currentImage.sourceTexture2 : currentImage.sourceTexture);
                        m_displayCanvas.RightTexture = (currentImage.swapLeftRight ? currentImage.sourceTexture : currentImage.sourceTexture2);
                    }
                }
            }
        }
            
        /// <summary>
        /// Cropped = Remove pixels from the sides of a image to allow a range of convergence.
        /// Tiled = When convergence is adjusted, the image will wrap around with tiling.
        /// Cropped is generally better, even if you lose pixels.
        /// </summary>
        public ConvergenceMode ConvergenceMode
        {
            get
            {
                return currentImage.convergenceMode;
            }
            set
            {
                currentImage.convergenceMode = value;
            }
        }
            
        /// <summary>
        /// Convergence is an abitrary value used to adjust the focal depth of an image. 
        /// This allows you to fix badly taken 3D images, control where certain elements pop out of the frame, and sudo-adjust the contentspercieved scale.
        /// When "Crop" is disabled this is a value ranging between -MaxConvergence and +MaxConvergence, and works at the cost of shaving some pixels off the sides of the image.
        /// Each unit of convergence equals 1 vertical row of pixels offset for each eye. So 2 vertical rows total.
        /// When "Crop" is enabled, this just slides the halves of the image and retain a common tiling pattern.
        /// </summary>
        public int Convergence
        {   
            get 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage != null)
                    return currentImage.convergence;
                else
                    return 0; // If theres no S3D image, we dont add any convergence.
            }
            set
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage == null || value == currentImage.convergence) return;

                // We only care about max convergence if we are cropping out pixels from the displayed image.
                if (currentImage.convergenceMode == ConvergenceMode.Cropped && currentImage.canvasFormat == CanvasFormat.Standard)
                {
                    int m = MaximumAllowedConvergence;

                    // We want raising convergence to also raise maxConvergence during non-runtime, but during runtime we just set convergence back down to maxConvergence if it exceeds it.
                    if ((Application.isPlaying && Mathf.Abs(value) > MaxConvergence) || (m != -1 && Mathf.Abs(value) > m))
                        currentImage.convergence = (value >= 0 ? MaxConvergence : -MaxConvergence);
                    else
                        currentImage.convergence = value;

                    // Raise the max convergence if it isnt runtime and value is higher than the current max.
                    if (!Application.isPlaying && Mathf.Abs(value) > MaxConvergence)
                        MaxConvergence = Mathf.Abs(value);

                    if (Application.isPlaying)
                        SetMaterialOffset();
                }
                else
                    currentImage.convergence = value;
            }
        }        

        /// <summary>
        /// See Convergence. This sets the max -/+ range of Convergence if Crop is enabled.
        /// The maximum this can be set to is 1/4 the amount of pixels of the displayed image an eye sees.
        /// </summary>
        public int MaxConvergence
        {
            get 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage != null)
                    return currentImage.maxConvergence;
                else
                    return 0; // If theres no S3D image, we dont add any convergence.
            }
            set
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                // Discard any negetive values.
                if (currentImage == null ||
                    value == currentImage.maxConvergence ||
                    value < 0) return;
                                
                int m = MaximumAllowedConvergence;

                if (m != -1 && value > m)
                {
                    // Discard values if they would reduce the displayed image to less than half of the split image size.
                    return;
                }

                // Ensure convergence is always lower than or equal to maxConvergence.
                if (value < Mathf.Abs(currentImage.convergence))
                    currentImage.convergence = value;

                currentImage.maxConvergence = value;

                if (Application.isPlaying)
                {
                    SetMaterialTiling();
                    SetMaterialOffset();
                }
            }
        }

        /// <summary>
        /// Some textures are mirrored vertically (not be be confused with up-side-down). This allows you to fix that.
        /// </summary>
        public bool VerticalFlip
        {
            get 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage != null)
                    return currentImage.verticalFlip;
                else
                    return false;
            }
            set 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage == null) return;
                currentImage.verticalFlip = value;
            }
        }

        /// <summary>
        /// The left eye color filter on the active Anaglyph image.
        /// </summary>
        public Color LeftEyeColor
        {   
            get 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage == null)
                    return Color.black;
                else
                    return currentImage.leftEyeColor;
            }
            // No "set" because this shouldn't be adjusted at runtime.
        }

        /// <summary>
        /// The right eye color filter on the active Anaglyph image.
        /// </summary>
        public Color RightEyeColor
        {
            get 
            {
                if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

                if (currentImage == null) 
                    return Color.black;
                else
                    return currentImage.rightEyeColor;
            }
            // No "set" because this shouldn't be adjusted at runtime.
        }
        
        /// <summary>
        /// A shortcut to get the width of the currently displayed image, whatever type it may be of.
        /// </summary>
        private int CurrentImageWidth
        {
            get 
            {
                if (currentImage.sourceTexture != null)
                    return currentImage.sourceTexture.width;
#if UNITY_5_6_PLUS
                else if (currentImage.videoClip != null)
                    return (int)currentImage.videoClip.width;
                else if (currentImage.videoURL != string.Empty)
                    return GetComponent<VideoPlayer>().texture.width;
#endif
                return 0;
            }
        }

        /// <summary>
        /// A shortcut to get the height of the currently displayed image, whatever type it may be of.
        /// </summary>
        private int CurrentImageHeight
        {
            get 
            {
                if (currentImage.sourceTexture != null)
                    return currentImage.sourceTexture.height;
#if UNITY_5_6_PLUS
                else if (currentImage.videoClip != null)
                    return (int)currentImage.videoClip.height;
                else if (currentImage.videoURL != string.Empty)
                    return GetComponent<VideoPlayer>().texture.height;
#endif
                return 0;
            }
        }
                
        /// <summary>
        /// A Texel is a unit of TextureScale thats equal to 1 pixel of the current displayed texture.
        /// </summary>
        private Vector2 CurrentImageTexelSize
        {
            get
            {
                if (currentImage.sourceTexture != null)
                    return currentImage.sourceTexture.texelSize;
#if UNITY_5_6_PLUS
                else if (currentImage.videoClip != null ||
                    currentImage.videoURL != string.Empty)
                    return GetComponent<VideoPlayer>().texture.texelSize;
#endif
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Calculates the maximum that MaxConvergence can be set to, based on the current texture.
        /// The max is 1/4 of each eyes pixels as a limit has to be set, and really... anymore is going to distort the image to much.
        /// </summary>        
        private int MaximumAllowedConvergence
        {
            get
            {
                int splitTextureWidth = CurrentImageWidth;

                if (splitTextureWidth != 0)
                {
                    switch (ImageFormat)
                    {
                        case ImageFormat.Side_By_Side:
                        case ImageFormat.VerticalInterlaced:
                        case ImageFormat.Checkerboard:
                        case ImageFormat.Anaglyph:
                        case ImageFormat.Mono_Side_By_Side:
                        case ImageFormat.Mono_VerticalInterlaced:
                        case ImageFormat.Mono_Checkerboard:
                        case ImageFormat.Mono_Anaglyph:
                            splitTextureWidth /= 8;
                            break;
                        default:                            
                            splitTextureWidth /= 4;
                            break;
                    }

                    // Quartered instead of halved because convergences deal in double.                    
                    return splitTextureWidth;
                }

                // Return -1 if theres no texture.
                return -1;
            }
        }

        /// <summary>
        /// The source texture thats being filtered because its of a Special format.
        /// </summary>
        private Texture m_renderTextureSource;
        
        /// <summary>
        /// The internal texture we use for filtering Special formats into SBS/TB.
        /// </summary>
        private RenderTexture m_renderTexture;

        /// <summary>
        /// The internal material we use for filtering Special formats into SBS/TB.
        /// </summary>
        private Material m_renderMaterial;

        /// <summary>
        /// We use this shader to do our conversions in the render material.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private Shader m_renderShader;
        
        /// <summary>
        /// The index of the pass we use in the renderShader.
        /// </summary>
        private int m_shaderPass = -1;

        // Just so we have some context for the shaderPass values when we set them.
        private const int SHADER_PASS_HORIZONTAL_INTERLACED = 0;
        private const int SHADER_PASS_VERTICAL_INTERLACED = 1;
        private const int SHADER_PASS_CHECKERBOARD = 2;
        private const int SHADER_PASS_ANAGLYPH = 3;

        [HideInInspector]
        [SerializeField]
        private Shader m_panoShader;

        // Some events you can subscribe to.
        public delegate void NewImageLoaded(Stereoscopic3DImage newImage);
        public static event NewImageLoaded OnNewImageLoadedEvent = (Stereoscopic3DImage newImage) => { };
        public delegate void FormatChanged(ImageFormat imageFormat);
        public static event FormatChanged OnFormatChangedEvent = (ImageFormat imageFormat) => { };
        public delegate void CanvasFormatChanged(CanvasFormat canvasFormat);
        public static event CanvasFormatChanged OnCanvasFormatChangedEvent = (CanvasFormat canvasFormat) => { };

        // Use this for initialization
        void Awake () 
        {
            // If this gameObject is a Clone we don't want the script to execute anything as doing so would be a waste of resources and could lead to infinite loops.
            if (gameObject.name.Contains("Clone")) return;

            m_displayCanvas = new DisplayCanvas(gameObject, targetTextureMaps);
            
            // After everything is setup, we can check and see if theres a default texture selected, as well as default 3D formatting settings to use.            
            if (defaultImage != null) DisplayImageInCanvas(defaultImage);
        }

	    // Update is called once per frame
	    void Update () 
        {
            /*
            // Test code to cycle through all the Image Formats possible.
		    if (Input.GetKeyDown(KeyCode.F))
            {
                int index = (int)ImageFormat;
                if (index < System.Enum.GetValues(typeof(ImageFormat)).Length - 1)
                    SetImageFormat((ImageFormat)(index + 1));
                else
                    SetImageFormat(ImageFormat._2D);                
            }
            */
            /*
            // Test code to cycle through all the Image Types possible.
		    if (Input.GetKeyDown(KeyCode.T))
            {
                int index = (int)ImageType;
                if (index < System.Enum.GetValues(typeof(ImageType)).Length - 1)
                    SetImageType((ImageType)(index + 1));
                else
                    SetImageType(ImageType.Standard);
            }
            */

            if (m_renderTexture != null &&
                m_renderTextureSource != null &&
                m_renderMaterial != null &&
                m_renderShader != null &&
                m_shaderPass != -1)
            {
                switch (currentImage.imageFormat)
                {
                    case ImageFormat.HorizontalInterlaced:                    
                    case ImageFormat.VerticalInterlaced:
                    case ImageFormat.Checkerboard:
                    case ImageFormat.Anaglyph:
                    case ImageFormat.Mono_HorizontalInterlaced:
                    case ImageFormat.Mono_VerticalInterlaced:
                    case ImageFormat.Mono_Checkerboard:
                    case ImageFormat.Mono_Anaglyph:
                        Graphics.Blit(m_renderTextureSource, m_renderTexture, m_renderMaterial, m_shaderPass);
                        break;
                    default:
                        break;
                }
            }
	    }

#if UNITY_EDITOR
        void Reset()
        {
            // Our shaders need to be included in the build, but we dont reference them until runtime. So we store them here.
            if (m_renderShader == null)
            {
                m_renderShader = Shader.Find("Hidden/VR3D/3DFormatConversion");

                if (m_renderShader == null)
                    Debug.LogWarning("[VR3DMediaViewer] Unable to load the \"3DFormatConversion\" shader. Did you delete it?");
            }
            if (m_panoShader == null)
            {
                m_panoShader = Shader.Find("Hidden/VR3D/3DPranorama");

                if (m_panoShader == null)
                    Debug.LogWarning("[VR3DMediaViewer] Unable to load the \"3DPranorama\" shader. Did you delete it?");
            }
        }
#endif

#region Core Methods

        /// <summary>
        /// This processes a given S3DImage and displays it in the scene.
        /// </summary>
        /// <param name="s3DImage"></param>
        private void DisplayImageInCanvas(Stereoscopic3DImage s3DImage)
        {
            if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

            // Save the 3D settings from the currently selected 3D image to our canvas.            
            currentImage = s3DImage;
            
            //
            if (currentImage.sourceTexture != null)
            {
                // We override the TextureWrapMode if the user selected to do so.
                // We do this because textures like videos do not expose TextureWrapMode to users to select
                // in Import Settings, but do support chaning it at runtime. So we provide a easy access option.
                if (currentImage.wrapModeOverride != TextureWrapMode3D.Default)
                {
                    currentImage.sourceTexture.wrapMode = (TextureWrapMode)currentImage.wrapModeOverride;
                    if (currentImage.sourceTexture2 != null) currentImage.sourceTexture2.wrapMode = (TextureWrapMode)currentImage.wrapModeOverride;
                }

                SetTexturesToMaterials(currentImage.sourceTexture, currentImage.sourceTexture2);
                Update3DCanvasFormatSettings();
                Update3DFomatSettings();
            }
#if UNITY_5_6_PLUS
            else if (currentImage.videoClip != null || currentImage.videoURL != string.Empty)
            {
                // Video player setup.
                VideoPlayer videoPlayer = gameObject.GetComponent<VideoPlayer>();
                
                if (videoPlayer == null)
                {
                    videoPlayer = gameObject.AddComponent<VideoPlayer>();
#if HIDE_VIDEOPLAYER
                    videoPlayer.hideFlags = HideFlags.HideInInspector;
#endif
                }
                else
                {
                    /*
                     * (06/04/19)
                     * I know this seems wierd, but if a VideoPlayer exists we delete it and add a new one.
                     * This is because I've observed a wierd issue (with at least Unity 5.6) where the audio will
                     * not work for a VideoPlayer component if it existed before runtime. I'm not sure why. I 
                     * tried a few workarounds, but nothing fixed the issue.
                    */
                    VideoPlayer newVideoPlayer = gameObject.AddComponent<VideoPlayer>();

                    newVideoPlayer.isLooping = videoPlayer.isLooping;
                    newVideoPlayer.playbackSpeed = videoPlayer.playbackSpeed;

                    DestroyImmediate(videoPlayer);
                    videoPlayer = newVideoPlayer;                    
                }

                /*
                * (05/21/17)
                * We only support VideoRenderMode.APIOnly.
                * Reasoning: 
                *  VideoRenderMode.CameraFarPlane   = Doesnt seems to make sense to support.
                *  VideoRenderMode.CameraNearPlane  = Doesnt seems to make sense to support.
                *  VideoRenderMode.RenderTexture    = Not compatible. (Since some of our formats use a RenderTexture, and basically we would have to end up rendering a RenderTexture into a RenderTexture.)
                *  VideoRenderMode.MaterialOverride = Not compatible. (It locks the "mainTextureScale"/"mainTextureOffset" of the materials.)
                */
                videoPlayer.renderMode = VideoRenderMode.APIOnly;

                videoPlayer.playOnAwake = true;
                videoPlayer.waitForFirstFrame = true;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                videoPlayer.controlledAudioTrackCount = 1;

                // Audio setup.
                AudioSource audioSource = gameObject.GetComponent<AudioSource>();

                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();
                                
                audioSource.spatialBlend = 1;
                
                videoPlayer.SetTargetAudioSource(0, audioSource);
                
                // Load our video.
                if (currentImage.videoClip != null)
                {
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = currentImage.videoClip;
                }
                else if (currentImage.videoURL != string.Empty)
                {
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.url = currentImage.videoURL;
                }

                // We finish setting up in "OnPrepareComplete" once the video is loaded.
                videoPlayer.prepareCompleted += OnPrepareComplete;
                videoPlayer.Prepare();
            }            
#endif
            else
                Debug.LogError("[VR3DMediaViewer] No Stereoscopic 3D image/video texture found in " + s3DImage.name + ". Unable to display anything.");

            OnNewImageLoadedEvent(currentImage);
        }

        /// <summary>
        /// This applies the settings of the currently selected S3DImage to the canvas.
        /// </summary>
        private void Update3DFomatSettings()
        {   
            // Apply the new format settings to our canvas.
            SetMaterialTiling();
            SetMaterialOffset();

            OnFormatChangedEvent(ImageFormat);
        }

        private void Update3DCanvasFormatSettings()
        {
            bool isUsingPanoramicMedia = (CanvasFormat == CanvasFormat._360 || CanvasFormat == CanvasFormat._180);
            
            m_displayCanvas.CanvasFormat = CanvasFormat;
            m_displayCanvas.Rotation = Rotation;

            OnCanvasFormatChangedEvent(CanvasFormat);            
        }

        /// <summary>
        /// This changes the main texture Tiling in the shader properties of the left/right materials, so each shows a different half of the texture.
        /// </summary>
        private void SetMaterialTiling()
        {   
            // The full source image, each eyes half of the source image, and the actual image each eye sees are different dimensions.
            // We need the dimensions of each image after the full source image has been split in 2 then cropped to allow a range of convergence adjustment.
            // We quad the maxConvergence values as both eyes lose double.
            int modifiedTextureWidth = CurrentImageWidth - (Mathf.Abs(MaxConvergence) * 2); // Convergence adjustments only need to be calculated on the horizontal axis as this is based on eyes being side-by-side, not placement of the images on the texture.            

            Vector2 newTiling = new Vector2((modifiedTextureWidth * CurrentImageTexelSize.x), m_displayCanvas.DefaultMainTextureScale.y);

            switch (ImageFormat)
            {
                // Get tiling for Side-by-Side based formats.
                case ImageFormat.Side_By_Side:
                case ImageFormat.VerticalInterlaced:
                case ImageFormat.Checkerboard:
                case ImageFormat.Anaglyph:
                case ImageFormat.Mono_Side_By_Side:
                case ImageFormat.Mono_VerticalInterlaced:
                case ImageFormat.Mono_Checkerboard:
                case ImageFormat.Mono_Anaglyph:
                    newTiling.x /= 2;
                    break;
                // Get tiling for Top/Bottom based formats.
                case ImageFormat.Top_Bottom:
                case ImageFormat.HorizontalInterlaced:
                case ImageFormat.Mono_Top_Bottom:
                case ImageFormat.Mono_HorizontalInterlaced:                    
                    newTiling.y /= 2;
                    break;
                default:
                    break;
            }
                        
            // I hate this, but some textures are like this...
            newTiling.y = (VerticalFlip ? (newTiling.y * -1) : newTiling.y);

            m_displayCanvas.LeftMainTextureScale = newTiling;            

            switch (ImageFormat)
            {
                case ImageFormat._2D:
                case ImageFormat.Side_By_Side:
                case ImageFormat.Top_Bottom:
                case ImageFormat.TwoImages:
                case ImageFormat.HorizontalInterlaced:
                case ImageFormat.VerticalInterlaced:
                case ImageFormat.Checkerboard:
                case ImageFormat.Anaglyph:
                    m_displayCanvas.RightMainTextureScale = newTiling;                    
                    break;
                default:
                    m_displayCanvas.RightMainTextureScale = m_displayCanvas.LeftMainTextureScale;
                    break;
            }
        }

        /// <summary>    
        /// This changes the main texture Offset in the shader properties of the left/right materials, so each shows a different half of the texture.
        /// </summary>
        private void SetMaterialOffset()
        {
            // We need the convergence translated into a TextureScale value.
            float currentTextureScaleOffset = (Convergence * CurrentImageTexelSize.x);

            // When cropping out pixels our texture alignment wont be centered anymore unless we compensate for the amount of scale that we lost.
            // This is a quarter of the texture scale amount that has been lost to cropping, because EACH EYE is losing X pixels on each side. So X*2 for each eye = X*4.                        
            float maxTextureScaleOffset = (MaxConvergence * CurrentImageTexelSize.x);

            // We use the default values as a fallback.
            Vector2 newLeftEyeOffset = m_displayCanvas.DefaultMainTextureOffset;
            Vector2 newRightEyeOffset = m_displayCanvas.DefaultMainTextureOffset;

            switch (ImageFormat)
            {
                // Get offset for Side-by-Side based formats.
                case ImageFormat.Side_By_Side:
                case ImageFormat.VerticalInterlaced:
                case ImageFormat.Checkerboard:
                case ImageFormat.Anaglyph:
                case ImageFormat.Mono_Side_By_Side:
                case ImageFormat.Mono_VerticalInterlaced:
                case ImageFormat.Mono_Checkerboard:
                case ImageFormat.Mono_Anaglyph:                
                    // Texture coordinates use a format where "0,0" is the "Left,Bottom".
                    // So we place the Left image on the Left when not swapping due to our interpriation of the standards.
                    if (currentImage.swapLeftRight)
                        newLeftEyeOffset.x = (m_displayCanvas.DefaultMainTextureScale.x / 2);
                    else
                        newRightEyeOffset.x = (m_displayCanvas.DefaultMainTextureScale.x / 2);
                
                    // Since the image is halved we need to half this too.
                    currentTextureScaleOffset /= 2;
                    maxTextureScaleOffset /= 2;
                    break;
                // Get offset for Top/Bottom based formats.
                case ImageFormat.Top_Bottom:
                case ImageFormat.HorizontalInterlaced:
                case ImageFormat.Mono_Top_Bottom:
                case ImageFormat.Mono_HorizontalInterlaced:
                    // Texture coordinates use a format where "0,0" is the "Left,Bottom".
                    // So we place the Left image on the top when not swapping due to our interpriation of the standards.
                    if (currentImage.swapLeftRight)
                        newRightEyeOffset.y = (m_displayCanvas.DefaultMainTextureScale.y / 2);
                    else
                        newLeftEyeOffset.y = (m_displayCanvas.DefaultMainTextureScale.y / 2);
                    break;
                default:
                    break;
            }

            m_displayCanvas.LeftMainTextureOffset = new Vector2(
                    (newLeftEyeOffset.x + currentTextureScaleOffset) + maxTextureScaleOffset,
                    newLeftEyeOffset.y);

            switch (ImageFormat)
            {
                // Non-Mono formats.
                case ImageFormat._2D:
                case ImageFormat.Side_By_Side:
                case ImageFormat.Top_Bottom:
                case ImageFormat.TwoImages:
                case ImageFormat.HorizontalInterlaced:
                case ImageFormat.VerticalInterlaced:
                case ImageFormat.Checkerboard:
                case ImageFormat.Anaglyph:
                    m_displayCanvas.RightMainTextureOffset = new Vector2(
                    (newRightEyeOffset.x - currentTextureScaleOffset) + maxTextureScaleOffset,
                    newRightEyeOffset.y);                    
                    break;
                // Mono formats.
                default:
                    m_displayCanvas.RightMainTextureOffset = m_displayCanvas.LeftMainTextureOffset;                    
                    break;
            }
        }
        
        /// <summary>
        /// Some formats require "decrypting" the image data in order to be displayed, like Anaglyph, Checkerboard and the Interlaced formats.
        /// So for those we use a RenderTexture and a special shader, as the alternative would be to slow for animated textures like video.
        /// </summary>
        private void SetSpecialFormatTextureToMaterials(Texture sourceTexture)
        {   
            // Setup the appropriate material/shader.
            switch (currentImage.imageFormat)
            {
                case ImageFormat.HorizontalInterlaced:
                case ImageFormat.Mono_HorizontalInterlaced:
                    m_renderMaterial = new Material(m_renderShader);
                    m_shaderPass = SHADER_PASS_HORIZONTAL_INTERLACED;
                    break;
                case ImageFormat.VerticalInterlaced:
                case ImageFormat.Mono_VerticalInterlaced:
                    m_renderMaterial = new Material(m_renderShader);
                    m_shaderPass = SHADER_PASS_VERTICAL_INTERLACED;
                    break;
                case ImageFormat.Checkerboard:
                case ImageFormat.Mono_Checkerboard:
                    m_renderMaterial = new Material(m_renderShader);
                    m_shaderPass = SHADER_PASS_CHECKERBOARD;
                    break;
                case ImageFormat.Anaglyph:
                case ImageFormat.Mono_Anaglyph:
                    m_renderMaterial = new Material(m_renderShader);
                    m_shaderPass = SHADER_PASS_ANAGLYPH;
                    m_renderMaterial.SetColor("_LeftColor", currentImage.leftEyeColor);
                    m_renderMaterial.SetColor("_RightColor", currentImage.rightEyeColor);
                    break;
                default:
                    break;
            }

            // Create our RenderTexture.
            if (m_renderTexture)
            {
                RenderTexture.active = null;
                m_renderTexture.Release();
            }

            int width = (currentImage.imageFormat == ImageFormat.Anaglyph ||
                         currentImage.imageFormat == ImageFormat.Mono_Anaglyph ?
                         (sourceTexture.width * 2) :
                         sourceTexture.width);
            m_renderTexture = new RenderTexture(width, sourceTexture.height, 24, RenderTextureFormat.ARGB32);
            m_renderTexture.filterMode = FilterMode.Point;

            m_renderTextureSource = sourceTexture;

            // Render the initial image.
            Graphics.Blit(m_renderTextureSource, m_renderTexture, m_renderMaterial, m_shaderPass);
            
            m_displayCanvas.LeftTexture = m_renderTexture;
            m_displayCanvas.RightTexture = m_renderTexture;
        }
                
        /// <summary>
        /// Applys the given texture(s) to the texture maps of the materials.
        /// </summary>
        /// <param name="texture">The texture to be displayed in both materials, or the left eye if "texture2" isnt null.</param>
        /// <param name="texture2">The texture to be displayed to the right eye if "texture" isnt used for both.</param>
        private void SetTexturesToMaterials(Texture texture, Texture texture2 = null)
        {
            switch (currentImage.imageFormat)
            {
                case ImageFormat.HorizontalInterlaced:
                case ImageFormat.Mono_HorizontalInterlaced:
                case ImageFormat.VerticalInterlaced:
                case ImageFormat.Mono_VerticalInterlaced:
                case ImageFormat.Checkerboard:
                case ImageFormat.Mono_Checkerboard:
                case ImageFormat.Anaglyph:
                case ImageFormat.Mono_Anaglyph:
                    SetSpecialFormatTextureToMaterials(texture);
                    break;                
                default:
                    if ((currentImage.imageFormat == ImageFormat.TwoImages ||
                        currentImage.imageFormat == ImageFormat.Mono_TwoImages) &&
                        texture2 != null)
                    {
                        m_displayCanvas.LeftTexture = (currentImage.swapLeftRight ? (texture2 != null ? texture2 : texture) : texture);
                        m_displayCanvas.RightTexture = (currentImage.swapLeftRight ? texture : (texture2 != null ? texture2 : texture));
                    }
                    else
                    {
                        m_displayCanvas.LeftTexture = texture;
                        m_displayCanvas.RightTexture = texture;
                    }

                    // We null these so the Graphics.Blit doesnt happen in the Update every frame when a RenderTexture isnt even being used.
                    m_renderTexture = null;
                    m_renderTextureSource = null;
                    m_renderMaterial = null;
                    m_shaderPass = -1;
                    break;
            }
        }
        
#if UNITY_5_6_PLUS
        /// <summary>
        /// We catch when a video is ready after the VideoPlayer being previosuly set because until now we didnt have a texture file to work with.
        /// </summary>
        /// <param name="videoPlayer"></param>
        private void OnPrepareComplete(VideoPlayer videoPlayer)
        {   
            videoPlayer.prepareCompleted -= OnPrepareComplete;

            if (videoPlayer.clip != null || videoPlayer.url != null)
            {
                // We override the TextureWrapMode if the user selected to do so.
                // We do this because textures like videos do not expose TextureWrapMode to users to select
                // in Import Settings, but do support chaning it at runtime. So we provide a easy access option.
                if (currentImage.wrapModeOverride != TextureWrapMode3D.Default)
                    videoPlayer.texture.wrapMode = (TextureWrapMode)currentImage.wrapModeOverride;

                SetTexturesToMaterials(videoPlayer.texture);
                Update3DCanvasFormatSettings();
                Update3DFomatSettings();
            }
        }
#endif

#endregion

#region Public Functions for Runtime Use

        /// <summary>
        /// Set a new Stereoscopic 3D Image to be displayed with this VR3DMediaViewer instance.
        /// </summary>
        /// <param name="newS3DImage"></param>
        public void SetNewImage(Stereoscopic3DImage newS3DImage)
        {
            DisplayImageInCanvas(newS3DImage);
        }

        /// <summary>
        /// Set a new Stereoscopic 3D Image to be displayed with this VR3DMediaViewer instance.
        /// When not supplying the formatting data like with the Stereoscopic3DImage overload, its just assumed you want to use the current 3D settings.
        /// </summary>
        /// <param name="texture">A single texture containing images for both eyes for a Stereoscopic 3D Image. Can be Texture, Texture2D, MovieTexture etc.</param>
        public void SetNewImage(Texture texture)
        {
            Stereoscopic3DImage newS3DImage = Stereoscopic3DImage.Create(texture, ImageFormat, SwapLeftRight, Convergence, MaxConvergence, VerticalFlip, LeftEyeColor, RightEyeColor);

            SetNewImage(newS3DImage);
        }

        /// <summary>
        /// Set a new Stereoscopic 3D Image to be displayed with this VR3DMediaViewer instance.
        /// When not supplying the formatting data like with the Stereoscopic3DImage overload, its just assumed you want to use the current 3D settings.
        /// </summary>
        /// <param name="texture">A texture containing the image for the Left eye of a Stereoscopic 3D Image.</param>
        /// <param name="texture2">A texture containing the image for the Right eye of a Stereoscopic 3D Image.</param>
        public void SetNewImage(Texture texture, Texture texture2)
        {
            Stereoscopic3DImage newS3DImage = Stereoscopic3DImage.Create(texture, texture2, ImageFormat, SwapLeftRight, Convergence, MaxConvergence, VerticalFlip, LeftEyeColor, RightEyeColor);

            SetNewImage(newS3DImage);
        }

#if UNITY_5_6_PLUS
        /// <summary>
        /// Set a new Stereoscopic 3D Image to be displayed with this VR3DMediaViewer instance.
        /// When not supplying the formatting data like with the Stereoscopic3DImage overload, its just assumed you want to use the current 3D settings.
        /// </summary>
        /// <param name="videoClip">A videoClip of a Stereoscopic 3D format video. Requires Unity 5.6 and a UnityEngine.Video.VideoPlayer.</param>
        public void SetNewImage(VideoClip videoClip)
        {
            Stereoscopic3DImage newS3DImage = Stereoscopic3DImage.Create(videoClip, ImageFormat, SwapLeftRight, Convergence, MaxConvergence, VerticalFlip, LeftEyeColor, RightEyeColor);
            
            SetNewImage(newS3DImage);
        }

        /// <summary>
        /// Set a new Stereoscopic 3D Image to be displayed with this VR3DMediaViewer instance.
        /// When not supplying the formatting data like with the Stereoscopic3DImage overload, its just assumed you want to use the current 3D settings.
        /// </summary>
        /// <param name="videoURL">A URL to a video file of a Stereoscopic 3D format video. Requires Unity 5.6 and a UnityEngine.Video.VideoPlayer.</param>
        public void SetNewImage(string videoURL)
        {
            Stereoscopic3DImage newS3DImage = Stereoscopic3DImage.Create(videoURL, ImageFormat, SwapLeftRight, Convergence, MaxConvergence, VerticalFlip, LeftEyeColor, RightEyeColor);

            SetNewImage(newS3DImage);
        }
#endif

        /// <summary>
        /// Call to change the display format of the currently displayed S3DImage.
        /// Can also be use to set the canavas to display the source image with no 3D formating.
        /// Can also be used to display the canvas in "Mono" mode, which displays the same half of a S3DImage to both eyes.
        /// </summary>
        /// <param name="newFormat">Side-by-Side, Top/Bottom. Two Images, etc.</param>
        public void SetImageFormat(ImageFormat newFormat, bool verticalFlip = false)
        {
            if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

            ImageFormat = newFormat;
            VerticalFlip = verticalFlip;
                        
            // We call these to refresh the image with the new formatting.             
#if UNITY_5_6_PLUS
            if (currentImage.videoClip != null || currentImage.videoURL != string.Empty)
                SetTexturesToMaterials(GetComponent<VideoPlayer>().texture);
            else
#endif
                SetTexturesToMaterials(currentImage.sourceTexture, currentImage.sourceTexture2);
            
            Update3DFomatSettings();
        }

        /// <summary>
        /// Call to change the canvas format of the currently displayed S3DImage.
        /// </summary>
        /// <param name="newType"></param>
        public void SetCanvasFormat(CanvasFormat newType)
        {
            if (m_displayCanvas == null) m_displayCanvas = new DisplayCanvas(gameObject);

            CanvasFormat = newType;            

            // We call these to refresh the image with the new formatting.             
#if UNITY_5_6_PLUS
            if (currentImage.videoClip != null || currentImage.videoURL != string.Empty)
                SetTexturesToMaterials(GetComponent<VideoPlayer>().texture);
            else
#endif
                SetTexturesToMaterials(currentImage.sourceTexture, currentImage.sourceTexture2);

            Update3DCanvasFormatSettings();
        }

        #endregion

        public class DisplayCanvas
        {
            /// <summary>
            /// Used if the display canvas uses a MeshRenderer or SkinnedMeshRenderer component.
            /// </summary>
            private Renderer m_rendererComponent;

            /// <summary>
            /// Used if the display canvas uses a RawImage component.
            /// </summary>
            private UnityEngine.UI.RawImage m_rawImageComponent;

            /// <summary>
            /// The material belonging to the the renderer of the GameObject thats visible to the left eye.
            /// </summary>
            private Material m_leftImageMaterial;

            /// <summary>
            /// The material belonging to the the renderer of the GameObject thats visible to the right eye.
            /// </summary>
            private Material m_rightImageMaterial;

            /// <summary>
            /// Use with the RawImage component.
            /// </summary>
            private Material m_defaultMaterial;

            /// <summary>
            /// A list of texture shader properties in which we assign our stereoscopic 3D images.
            /// </summary>
            private string[] m_targetTextureMaps = { "_MainTex" };

            /// <summary>
            /// The RawImage component belonging to the the renderer of the GameObject thats visible to the left eye.
            /// </summary>
            private UnityEngine.UI.RawImage m_leftRawImage;

            /// <summary>
            /// The RawImage component belonging to the the renderer of the GameObject thats visible to the right eye.
            /// </summary>
            private UnityEngine.UI.RawImage m_rightRawImage;            

            /// <summary>
            /// We change the materials/RawImages main texture map Tiling value at runtime, so we save what it was at default as a reference for some calculations.
            /// </summary>
            private Vector2 m_defaultMainTextureScale;

            /// <summary>
            /// We change the materials/RawImages main texture map Tiling value at runtime, so we save what it was at default as a reference for some calculations.
            /// </summary>
            public Vector2 DefaultMainTextureScale
            {
                get { return m_defaultMainTextureScale; }
            }

            /// <summary>
            /// We change the materials/RawImages main texture map Offset value at runtime, so we save what it was at default as a reference for some calculations.
            /// </summary>
            private Vector2 m_defaultMainTextureOffset;

            /// <summary>
            /// We change the materials/RawImages main texture map Offset value at runtime, so we save what it was at default as a reference for some calculations.
            /// </summary>
            public Vector2 DefaultMainTextureOffset
            {
                get { return m_defaultMainTextureOffset; }
            }

            /// <summary>
            /// Used to restore the selected shader in instances in which this script changes it.
            /// </summary>
            private Shader m_defaultShader;

            /// <summary>
            /// Current scale for the left texture.
            /// </summary>
            public Vector2 LeftMainTextureScale
            {
                get
                {
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                        return m_leftImageMaterial.mainTextureScale;                      
                    else
                        return m_leftRawImage.uvRect.size;
                }
                set
                {
                    // Now that we have our values, we update the 2 materials.
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                    {
                        m_leftImageMaterial.mainTextureScale = value;

                        if (UsingRawImage && isUsingPanoramicCanvas)
                            m_leftRawImage.material = m_leftImageMaterial;
                    }
                    else
                        m_leftRawImage.uvRect = new Rect(m_leftRawImage.uvRect.position, value);
                }
            }

            /// <summary>
            /// Current scale for the right texture.
            /// </summary>
            public Vector2 RightMainTextureScale
            {
                get
                {
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                        return m_rightImageMaterial.mainTextureScale;
                    else
                        return m_rightRawImage.uvRect.size;
                }
                set
                {
                    // Now that we have our values, we update the 2 materials.
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                    {
                        m_rightImageMaterial.mainTextureScale = value;

                        if (UsingRawImage && isUsingPanoramicCanvas)
                            m_rightRawImage.material = m_rightImageMaterial;
                    }
                    else
                        m_rightRawImage.uvRect = new Rect(m_rightRawImage.uvRect.position, value);
                }
            }

            /// <summary>
            /// Current offset for the left texture.
            /// </summary>
            public Vector2 LeftMainTextureOffset
            {
                get
                {
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                        return m_leftImageMaterial.mainTextureOffset;
                    else
                        return m_leftRawImage.uvRect.position;
                }
                set
                {
                    // Now that we have our values, we update the 2 materials.
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                    {
                        m_leftImageMaterial.mainTextureOffset = value;

                        if (UsingRawImage && isUsingPanoramicCanvas)
                            m_leftRawImage.material = m_leftImageMaterial;
                    }
                    else
                        m_leftRawImage.uvRect = new Rect(value, m_leftRawImage.uvRect.size);
                }
            }

            /// <summary>
            /// Current offset for the right texture.
            /// </summary>
            public Vector2 RightMainTextureOffset
            {
                get
                {
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                        return m_rightImageMaterial.mainTextureOffset;
                    else
                        return m_rightRawImage.uvRect.position;
                }
                set
                {
                    // Now that we have our values, we update the 2 materials.
                    if (!UsingRawImage ||
                        (UsingRawImage && isUsingPanoramicCanvas))
                    {
                        m_rightImageMaterial.mainTextureOffset = value;

                        if (UsingRawImage && isUsingPanoramicCanvas)
                            m_rightRawImage.material = m_rightImageMaterial;
                    }
                    else
                        m_rightRawImage.uvRect = new Rect(value, m_rightRawImage.uvRect.size);
                }
            }

            /// <summary>
            /// Reference to the left texture.
            /// </summary>
            public Texture LeftTexture
            {
                get
                {
                    if (UsingRawImage)
                        return m_leftRawImage.mainTexture;
                    else
                        return m_leftImageMaterial.mainTexture;
                }
                set
                {
                    if (UsingRawImage)
                        m_leftRawImage.texture = value;                        
                    else
                    {
                        // Select the given texture into the texture maps of both materials.
                        foreach (string targetTextureMap in m_targetTextureMaps)
                            m_leftImageMaterial.SetTexture(targetTextureMap, value);
                    }
                }
            }

            /// <summary>
            /// Reference to the right texture.
            /// </summary>
            public Texture RightTexture
            {
                get
                {
                    if (UsingRawImage)
                        return m_rightRawImage.mainTexture;
                    else
                        return m_rightImageMaterial.mainTexture;
                }
                set
                {
                    if (UsingRawImage)       
                        m_rightRawImage.texture = value;
                    else
                    {
                        // Select the given texture into the texture maps of both materials.
                        foreach (string targetTextureMap in m_targetTextureMaps)
                            m_rightImageMaterial.SetTexture(targetTextureMap, value);
                    }
                }
            }

            /// <summary>
            /// If we are using a RawImage component or not. If not, it is assumed we are using the default Renderer support.
            /// </summary>
            public bool UsingRawImage
            {
                get
                {
                    if (m_rawImageComponent)
                        return true;                    
                    else
                        return false;
                }                
            }

            private CanvasFormat m_canvasFormat = CanvasFormat.Standard;

            /// <summary>
            /// Controls the type of canvas we are using. Standard, or Panoramic specific.
            /// </summary>
            public CanvasFormat CanvasFormat
            {
                get
                {
                    return m_canvasFormat;
                }
                set
                {
                    CanvasFormat oldCanvasFormat = m_canvasFormat;

                    m_canvasFormat = value;

                    if (UsingRawImage)
                    {
                        // Changing to using Panoramic canvas from standard.
                        if (oldCanvasFormat == CanvasFormat.Standard &&
                            (m_canvasFormat == CanvasFormat._360 ||
                                m_canvasFormat == CanvasFormat._180))
                        {
                            SetRawImageMaterials();
                        }                        
                        // Changing between Panos.
                        else if ((oldCanvasFormat == CanvasFormat._360 ||
                                oldCanvasFormat == CanvasFormat._180) &&
                                (m_canvasFormat == CanvasFormat._360 ||
                                m_canvasFormat == CanvasFormat._180))
                        {
                            m_leftImageMaterial.SetFloat("_OneEightyDegrees", (m_canvasFormat == CanvasFormat._180 ? 1 : 0));
                            m_rightImageMaterial.SetFloat("_OneEightyDegrees", (m_canvasFormat == CanvasFormat._180 ? 1 : 0));

                            m_leftImageMaterial.SetFloat("_OneEightyClamp", (m_leftRawImage.mainTexture.wrapMode == TextureWrapMode.Clamp ? 1 : 0));
                            m_rightImageMaterial.SetFloat("_OneEightyClamp", (m_leftRawImage.mainTexture.wrapMode == TextureWrapMode.Clamp ? 1 : 0));

                            m_leftImageMaterial.SetFloat("_Rotation", m_rotation);
                            m_rightImageMaterial.SetFloat("_Rotation", m_rotation);                            
                        }
                        else
                        {
                            m_leftRawImage.material = m_defaultMaterial;
                            m_rightRawImage.material = m_defaultMaterial;
                        }
                    }
                    else
                    {
                        if (oldCanvasFormat == CanvasFormat.Standard &&
                            (m_canvasFormat == CanvasFormat._360 ||
                                m_canvasFormat == CanvasFormat._180))
                        {
                            // Change the shader.
                            m_leftImageMaterial.shader = Shader.Find("Hidden/VR3D/3DPanorama");
                            m_rightImageMaterial.shader = Shader.Find("Hidden/VR3D/3DPanorama");

                            m_leftImageMaterial.SetFloat("_OneEightyDegrees", (m_canvasFormat == CanvasFormat._180 ? 1 : 0));
                            m_rightImageMaterial.SetFloat("_OneEightyDegrees", (m_canvasFormat == CanvasFormat._180 ? 1 : 0));

                            m_leftImageMaterial.SetFloat("_OneEightyClamp", (m_leftImageMaterial.mainTexture.wrapMode == TextureWrapMode.Clamp ? 1 : 0));
                            m_rightImageMaterial.SetFloat("_OneEightyClamp", (m_rightImageMaterial.mainTexture.wrapMode == TextureWrapMode.Clamp ? 1 : 0)); 

                            m_leftImageMaterial.SetFloat("_Rotation", m_rotation);
                            m_rightImageMaterial.SetFloat("_Rotation", m_rotation);

                        }
                        // Changing between Panos.
                        else if ((oldCanvasFormat == CanvasFormat._360 ||
                                oldCanvasFormat == CanvasFormat._180) &&
                                (m_canvasFormat == CanvasFormat._360 ||
                                m_canvasFormat == CanvasFormat._180))
                        {
                            m_leftImageMaterial.SetFloat("_OneEightyDegrees", (m_canvasFormat == CanvasFormat._180 ? 1 : 0));
                            m_rightImageMaterial.SetFloat("_OneEightyDegrees", (m_canvasFormat == CanvasFormat._180 ? 1 : 0));

                            m_leftImageMaterial.SetFloat("_OneEightyClamp", (m_leftImageMaterial.mainTexture.wrapMode == TextureWrapMode.Clamp ? 1 : 0));
                            m_rightImageMaterial.SetFloat("_OneEightyClamp", (m_rightImageMaterial.mainTexture.wrapMode == TextureWrapMode.Clamp ? 1 : 0));

                            m_leftImageMaterial.SetFloat("_Rotation", m_rotation);
                            m_rightImageMaterial.SetFloat("_Rotation", m_rotation);
                        }
                        else
                        {
                            m_leftImageMaterial.shader = m_defaultShader;
                            m_rightImageMaterial.shader = m_defaultShader;
                        }
                    }
                }
            }

            private float m_rotation = 0;
            /// <summary>
            /// Rotation value for panoramic media.
            /// </summary>
            public float Rotation
            {
                get { return m_rotation; }
                set
                {
                    m_rotation = value;

                    if (isUsingPanoramicCanvas)
                    {
                        m_leftImageMaterial.SetFloat("_Rotation", m_rotation);
                        m_rightImageMaterial.SetFloat("_Rotation", m_rotation);
                    }
                }
            }

            private bool isUsingPanoramicCanvas
            {
                get { return (m_canvasFormat == CanvasFormat._360 || m_canvasFormat == CanvasFormat._180); }
            }

            /// <summary>
            /// Constructor for this class.
            /// </summary>
            /// <param name="gameObject">The GameObject containing a Renderer or RawImage component.</param>
            /// <param name="targetTextureMaps">Used only with Renderer support.</param>
            public DisplayCanvas(GameObject gameObject, string[] targetTextureMaps = null)
            {
                // We figure out what type of component we are using, and save a reference to it.
                if (gameObject.GetComponent<UnityEngine.UI.RawImage>())
                    m_rawImageComponent = gameObject.GetComponent<UnityEngine.UI.RawImage>();
                else
                {
                    m_rendererComponent = gameObject.GetComponent<Renderer>();

                    if (targetTextureMaps != null)
                        m_targetTextureMaps = targetTextureMaps;                        
                }

                SetupCanvas();
            }            
                        
            /// <summary>
            /// Our Canvas is just the GameObjects we use to display the S3DImage. But to do that we need to create a second GameObject and use 1 for the Left eye and one for the Right eye.
            /// </summary>
            private void SetupCanvas()
            {
                // This will be our reference to a new object used to display the right image.
                GameObject rightImageCanvas = null;

                if (m_rendererComponent != null)
                {
                    // We use the settings in the material as our default.
                    m_defaultMainTextureScale = m_rendererComponent.sharedMaterial.mainTextureScale;
                    m_defaultMainTextureOffset = m_rendererComponent.sharedMaterial.mainTextureOffset;
                    m_defaultShader = m_rendererComponent.sharedMaterial.shader;                    

                    // Duplicate this entire gameobject and immediately remove any behaviours from the dupe.
                    rightImageCanvas = Instantiate(m_rendererComponent.gameObject, m_rendererComponent.transform.position, m_rendererComponent.transform.rotation) as GameObject;

                    // We don't need it to have any scripts.
                    Behaviour[] behaviours = rightImageCanvas.GetComponents<Behaviour>();

                    foreach (Behaviour behaviour in behaviours)
                        DestroyImmediate(behaviour);

                    // Parent it to this GameObject so if its transform properties change the copy is changed a well.
                    rightImageCanvas.transform.parent = m_rendererComponent.transform;

                    // Ensure its scale is the same as the parent/left image.
                    rightImageCanvas.transform.localScale = new Vector3(1, 1, 1);

                    m_leftImageMaterial = m_rendererComponent.material;
                    m_rightImageMaterial = rightImageCanvas.GetComponent<Renderer>().material;

                    // Set the layer of each canvas.
                    m_rendererComponent.gameObject.layer = LayerManager.LeftLayerIndex;
                    rightImageCanvas.layer = LayerManager.RightLayerIndex;
                }
                else if (m_rawImageComponent != null)
                {
                    // We use the settings in the material as our default.
                    m_defaultMainTextureScale = new Vector2(m_rawImageComponent.uvRect.width, m_rawImageComponent.uvRect.height);
                    m_defaultMainTextureOffset = new Vector2(m_rawImageComponent.uvRect.x, m_rawImageComponent.uvRect.y);                    
                    m_defaultMaterial = m_rawImageComponent.material;
                    
                    // We parent the new copy to this GameObject so if its transform properties change the copy is changed a well.
                    rightImageCanvas = Instantiate(m_rawImageComponent.gameObject, m_rawImageComponent.transform.position, m_rawImageComponent.transform.rotation, m_rawImageComponent.transform) as GameObject;

                    // We don't need it to have any scripts.
                    Behaviour[] behaviours = rightImageCanvas.GetComponents<Behaviour>();

                    foreach (Behaviour behaviour in behaviours)
                    {
                        if (behaviour.GetType().ToString() != "UnityEngine.UI.RawImage")
                            DestroyImmediate(behaviour);                        
                    }
                    
                    // Ensure its scale is the same as the parent/left image.
                    rightImageCanvas.transform.localScale = new Vector3(1, 1, 1);

                    m_leftRawImage = m_rawImageComponent;
                    m_rightRawImage = rightImageCanvas.GetComponent<UnityEngine.UI.RawImage>();

                    if (isUsingPanoramicCanvas) SetRawImageMaterials();

                    // We add a Canvas component because if we dont, when we put the new copy on a different layer that layer will be ignored and overridden by the parent with a Canvas component.
                    rightImageCanvas.AddComponent<Canvas>();

                    // Set the layer of each canvas.
                    m_rawImageComponent.gameObject.layer = LayerManager.LeftLayerIndex;
                    rightImageCanvas.layer = LayerManager.RightLayerIndex;

                    // The UI.Canvas parent needs to be on the left layer too.
                    Transform currentTransform = m_rawImageComponent.transform.parent;
                    do
                    {
                        if (currentTransform.GetComponent<Canvas>())
                        {
                            currentTransform.gameObject.layer = LayerManager.LeftLayerIndex;
                            break;
                        }

                        currentTransform = currentTransform.parent;
                    } while (currentTransform);
                }
                else
                {
                    Debug.LogWarning("[VR3DMediaViewer] No Renderer or RawImage component found. Canvas setup incomplete!");
                    return;
                }
#if HIDE_RIGHT
                // It doesn't need to be seen.
                rightImageCanvas.hideFlags = HideFlags.HideInHierarchy;
#endif
            }            

            /// <summary>
            /// Sets up Materials for RawImage components, when neeeded.
            /// </summary>
            private void SetRawImageMaterials()
            {
                if (m_defaultMaterial.name == "Default UI Material")
                    m_leftImageMaterial = new Material(Shader.Find("Hidden/VR3D/3DPanorama"));
                else
                {
                    m_leftImageMaterial = m_leftRawImage.material;
                    m_leftImageMaterial.shader = Shader.Find("Hidden/VR3D/3DPanorama");
                }

                m_leftImageMaterial.SetFloat("_OneEightyDegrees", (m_canvasFormat == CanvasFormat._180 ? 1 : 0));
                m_leftImageMaterial.SetFloat("_Rotation", m_rotation);
                m_leftImageMaterial.SetFloat("_OneEightyClamp", (m_leftRawImage.mainTexture.wrapMode == TextureWrapMode.Clamp ? 1 : 0));

                m_rightImageMaterial = new Material(m_leftImageMaterial);

                m_leftRawImage.material = m_leftImageMaterial;
                m_rightRawImage.material = m_rightImageMaterial;
            }
        }
    }
}
