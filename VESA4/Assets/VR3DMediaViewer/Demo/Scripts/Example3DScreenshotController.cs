/*
 * Example3DScreenshotController
 * 
 * This script was made by Jason Peterson (DarkAkuma) of http://darkakumadev.z-net.us/
*/

/*
 * This is just an example Stereoscopic 3D Screenshot controller to call the Stereoscopic3DScreenshot script, get a texture, then save it as a .png file.
 * This uses Keyboard/Mouse, so it's PC/Mac/Linux only.
 * How you would design a script like this for VR would be VR SDK dependant, so we can't provide any such examples.
 *  The gist of what you would need to do differently though is, use buttons/input from the SDKs Input devices to replace the screenshotKey and focalDistance***Keys.
 *  
 * Minimum usage: (Generally just for dev usage to take screenshots from the Editor.)
 * 	Place this script on your camera or object at the center of your players view. Doing so will add a Stereoscopic3DScreenshot script as well, if one isnt already present.
 * 	Select a screenshot key.
 * 	Adjust the focalDistance in the Stereoscopic3DScreenshot script to about what you would want for objects inbetween the Plane visual and the camera to appear to "pop out of the image" in the resulting screenshot.
 * 	Compile, Run, and press that key when you see what you want to capture.
 * 	
 * Advanced usage: (A game time useable implimentation to allow players to take GOOD 3D screenshots)
 * 	Place this script on your Camera, object at the center of your players view, or a phyical object meant to work like a real Photo Camera. Doing so will add a Stereoscopic3DScreenshot script as well, if one isnt already present.
 * 	Select a screenshot key, and focalDistance zoom keys.
 * 	Select UI prefabs to be seen in scene as representations of the cameras bounds and focal distance.
 * 	(Optional) Select sounds for the screenshot snap and zoom.
 * 	(Optional) Select a VR3DMediaViewer component with which to display the newly taken screenshot in scene.
 * 
 * Optional:
 * 	If you want to use a different date/time format, you can change the Date Format parameter.
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VR3D;

[RequireComponent(typeof(Stereoscopic3DScreeenshot))]
public class Example3DScreenshotController : MonoBehaviour
{
    [Tooltip("Select a button that the user should press to take the screenshot.")]
    public KeyCode screenshotKey;
    [Tooltip("Select a button that the user should press to move the focal plane away from the screenshot camera.")]
    public KeyCode focalDistanceOutKey;
    [Tooltip("Select a button that the user should press to move the focal plane closer to the screenshot camera.")]
    public KeyCode focalDistanceInKey;

    [Tooltip("This is a prefab to be spawned in and used for the focal plane representation for the left eye.")]
    public GameObject leftFramePrefab;
    [Tooltip("This is a prefab to be spawned in and used for the focal plane representation for the right eye.")]
    public GameObject rightFramePrefab;

    [Tooltip("A sound to play when the user takes the screenshot.")]
    public AudioClip screenshotSnapSound;
    [Tooltip("A sound to play when the user zooms the focal distance in/out.")]
    public AudioClip zoomSound;
        
    [Tooltip("Used as part of the file name.")]
    public string dateFormat = "(MM_dd_yy) - (HH_mm)";

    [Tooltip("If supplied, the screenshot will be displayed here immediately.")]
    public VR3DMediaViewer canvas;    
        
    private GameObject leftFrameObject;
    private GameObject rightFrameObject;
        
    private const float FOCAL_DISTANCE_MIN = 0.31f;
    private const float FOCAL_DISTANCE_MAX = 10.0f;
    private const float FOCAL_DISTANCE_ZOOM_SPEED = 1.0f;

    private Stereoscopic3DScreeenshot mainScript;

    private AudioSource zoomAudioSource;

    void Awake()
    {
        mainScript = GetComponent<Stereoscopic3DScreeenshot>();

        zoomAudioSource = gameObject.AddComponent<AudioSource>();
        zoomAudioSource.clip = zoomSound;
        zoomAudioSource.loop = true;
        zoomAudioSource.playOnAwake = false;
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (screenshotKey != KeyCode.None)
        {
            if (Input.GetKeyDown(screenshotKey))
            {
                // We need to hide the UI so its not seen in the screenshot.
                if (leftFrameObject != null && rightFrameObject != null)
                {
                    leftFrameObject.SetActive(false);
                    rightFrameObject.SetActive(false);
                }

                string imagePath = TakeScreenshot();
                Debug.Log("Screenshot Taken: Saved to \"" + imagePath + "\".");

                // Now that the screenshot is taken, restore the UI.
                if (leftFrameObject != null && rightFrameObject != null)
                {
                    leftFrameObject.SetActive(true);
                    rightFrameObject.SetActive(true);
                }
                
                if (screenshotSnapSound != null)
                    AudioSource.PlayClipAtPoint(screenshotSnapSound, transform.position);

                // For our example scene, we choose to display the newly taken image directly in the scene.
                LoadImageFileToCanvas(imagePath);
            }
        }
        if (focalDistanceOutKey != KeyCode.None)
        {
            // We allow the user to hold the key down to slowly change the focalDistance each frame.
            if (Input.GetKey(focalDistanceOutKey))
            {
                if (mainScript.focalDistance < FOCAL_DISTANCE_MAX)
                    mainScript.focalDistance += FOCAL_DISTANCE_ZOOM_SPEED * Time.deltaTime;
                else
                    mainScript.focalDistance = FOCAL_DISTANCE_MAX;
            }

            // We play a loopable AudioClip with its pitch raised a little.
            if (Input.GetKeyDown(focalDistanceOutKey) && zoomSound != null)
            {
                zoomAudioSource.pitch = 1.1f;
                zoomAudioSource.Play();
            }
            else if (Input.GetKeyUp(focalDistanceOutKey) && zoomSound != null)
                zoomAudioSource.Stop();
        }
        if (focalDistanceInKey != KeyCode.None)
        {
            // We allow the user to hold the key down to slowly change the focalDistance each frame.
            if (Input.GetKey(focalDistanceInKey))
            {
                if (mainScript.focalDistance > FOCAL_DISTANCE_MIN)
                    mainScript.focalDistance -= FOCAL_DISTANCE_ZOOM_SPEED * Time.deltaTime;
                else
                    mainScript.focalDistance = FOCAL_DISTANCE_MIN;
            }

            // We play a loopable AudioClip with its pitch lowered a little.
            if (Input.GetKeyDown(focalDistanceInKey) && zoomSound != null)
            {
                zoomAudioSource.pitch = 0.9f;
                zoomAudioSource.Play();
            }
            else if (Input.GetKeyUp(focalDistanceInKey) && zoomSound != null)
                zoomAudioSource.Stop();
        }

        // We need to spawn in the UI frames if they havent been already, and theres prefrabs supplied.        
        if ((leftFrameObject == null || rightFrameObject == null) &&
            (leftFramePrefab != null && rightFramePrefab != null))
        {
            leftFrameObject = Instantiate(leftFramePrefab, transform);
            rightFrameObject = Instantiate(rightFramePrefab, transform);
        }

        // If that failed, likely if no prefabs are provided, then we don't need to do anything that follows.
        if (leftFrameObject == null || rightFrameObject == null) return;

        // We calculate the bounds of the cameras views.
        float frustumHeight = 2.0f * mainScript.focalDistance * Mathf.Tan(mainScript.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * ((float)mainScript.resolutionWidth / (float)mainScript.resolutionHeight);

        // Each camera is offset a little.
        Vector3 leftCameraPosition = transform.position + transform.right * -((mainScript.seperation / 2) / 1000);            
        Vector3 rightCameraPosition = transform.position + transform.right * ((mainScript.seperation / 2) / 1000);
        
        // The point of all of this is, the frames Z postion compared to the camera is used by the user to tell the focal distance of the screenshot.
        // Anything in front of the frame would appear in front of the resulting image, as if poping out of it.

        // Now that we have all the info, we set the frames positions/size.
        leftFrameObject.transform.position = leftCameraPosition + (transform.TransformDirection(Vector3.forward) * mainScript.focalDistance);
        leftFrameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(frustumWidth, frustumHeight);
                
        rightFrameObject.transform.position = rightCameraPosition + (transform.TransformDirection(Vector3.forward) * mainScript.focalDistance);
        rightFrameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(frustumWidth, frustumHeight);        
    }

    /// <summary>
    /// Takes a screenshot of where this gameobject is facing, and saves it to disk as a .png.
    /// </summary>
    /// <returns>The file path where the image was saved.</returns>
    public string TakeScreenshot()
    {
#if UNITY_STANDALONE // This is PC/Mac/Linux only because I'm just assuming the file path saving stuff would be different on platforms like mobile.
        
        // We use 0x0 for now. We resize it later as appropriate to the image format.
        Texture2D tex = new Texture2D(0, 0);
        Texture2D tex2 = new Texture2D(0, 0);

        mainScript.GetScreenshot(tex, tex2);        
                
        // Encode the texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        System.DateTime time = System.DateTime.Now;

        if (mainScript.imageFormat == Stereoscopic3DScreeenshot.ScreenShotImageFormat.TwoImages)
        {
            // Save the PNG file to disk in the applications directory.
            File.WriteAllBytes(Path.GetFullPath(Application.dataPath + "/../") + Application.productName + " - " + time.ToString(dateFormat) + "-Left.png", bytes);

            // Encode the texture into PNG
            bytes = tex2.EncodeToPNG();
            Destroy(tex2);

            // Save the PNG file to disk in the applications directory.
            File.WriteAllBytes(Path.GetFullPath(Application.dataPath + "/../") + Application.productName + " - " + time.ToString(dateFormat) + "-Right.png", bytes);

            return Path.GetFullPath(Application.dataPath + "/../") + Application.productName + " - " + time.ToString(dateFormat) + "-Left.png";
        }
        else
        {
            // Save the PNG file to disk in the applications directory.
            File.WriteAllBytes(Path.GetFullPath(Application.dataPath + "/../") + Application.productName + " - " + time.ToString(dateFormat) + ".png", bytes);

            return Path.GetFullPath(Application.dataPath + "/../") + Application.productName + " - " + time.ToString(dateFormat) + ".png";
        }
#else
        Debug.LogWarning("This script is only meant for PC/Mac/Linux.");

        return string.Empty;
#endif
    }

    /// <summary>
    /// This takes a path to a image file and displays the image in the selected VR3DMediaViewer canvas component.
    /// </summary>
    /// <param name="filePath"></param>
    public void LoadImageFileToCanvas(string filePath)
    {
        // If the canvas feidl is empty, we dont do anything.
        if (canvas == null) return;

        Texture2D newTex = null;
        byte[] imageData;

        if (File.Exists(filePath))
        {
            // Load the image as data, then convert it to a Texture2D.
            imageData = File.ReadAllBytes(filePath);
            newTex = new Texture2D(2, 2);
            newTex.LoadImage(imageData);

            // Set the texture to the canvas and set its format based on what format is selected in the Stereoscopic3DScreenshot component.
            canvas.SetNewImage(newTex);
            canvas.SetImageFormat(S3DScreeenshotImageFormat2Canvas(mainScript.imageFormat));            
        }
        else
            Debug.LogWarning("No file found.");
    }

    /// <summary>
    /// Translates a format type from the Stereoscopic3DScreenshot script to what the VR3DMediaViewer script can understand. 
    /// This is needed since its not a 1-1 conversion due to the fact that the VR3DMediaViewer script has 2D/Mono formats as well.
    /// </summary>
    /// <param name="screenshotFormat"></param>
    /// <returns></returns>
    public static ImageFormat S3DScreeenshotImageFormat2Canvas(Stereoscopic3DScreeenshot.ScreenShotImageFormat screenshotFormat)
    {
        ImageFormat canvasFormat = ImageFormat.Side_By_Side;

        switch (screenshotFormat)
        {
            case Stereoscopic3DScreeenshot.ScreenShotImageFormat.Side_By_Side:
                canvasFormat = ImageFormat.Side_By_Side;
                break;
            case Stereoscopic3DScreeenshot.ScreenShotImageFormat.Top_Bottom:
                canvasFormat = ImageFormat.Top_Bottom;
                break;
            case Stereoscopic3DScreeenshot.ScreenShotImageFormat.TwoImages:
                canvasFormat = ImageFormat.TwoImages;
                break;
            case Stereoscopic3DScreeenshot.ScreenShotImageFormat.HorizontalInterlaced:
                canvasFormat = ImageFormat.HorizontalInterlaced;
                break;
            case Stereoscopic3DScreeenshot.ScreenShotImageFormat.VerticalInterlaced:
                canvasFormat = ImageFormat.VerticalInterlaced;
                break;
            case Stereoscopic3DScreeenshot.ScreenShotImageFormat.Checkerboard:
                canvasFormat = ImageFormat.Checkerboard;
                break;
            case Stereoscopic3DScreeenshot.ScreenShotImageFormat.Anaglyph:
                canvasFormat = ImageFormat.Anaglyph;
                break;
            default:
                break;
        }

        return canvasFormat;
    }
}
