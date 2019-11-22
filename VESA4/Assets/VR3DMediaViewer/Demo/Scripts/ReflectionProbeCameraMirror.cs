using UnityEngine;
using UnityEngine.Rendering;

public class ReflectionProbeCameraMirror : MonoBehaviour 
{
    [Tooltip("Object to track as a camera reference point.")]
    public GameObject cameraObject;
    
    [Tooltip("Direction in local space. Values inbetween -1,-1,-1 and 1,1,1.")]
    public Vector3 forward;

    [Tooltip("THe Reflection Probe to use for producing the mirror image.")]
    public ReflectionProbe reflectionProbe;

    [Tooltip("Only call for the probe to render a new image every so many frames.")]
    public int frameSkip = 0;

    private int currentFrame = 0;
    
	// Use this for initialization
	void Start () 
    {        
        if (frameSkip > 0)
            reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (cameraObject && reflectionProbe)
        {
            UpdateProbePosition();

            if (reflectionProbe.refreshMode == ReflectionProbeRefreshMode.ViaScripting)
            {
                if (currentFrame == frameSkip)
                {
                    reflectionProbe.RenderProbe();
                    currentFrame = 0;
                }
                else
                    currentFrame++;
            }
        }
	}

    void UpdateProbePosition()
    {
        Vector3 forwardInWorldSpace = transform.TransformDirection(forward);
        Vector3 direction = cameraObject.transform.position - transform.position;        
        reflectionProbe.transform.position = transform.position + (direction - 2 * forwardInWorldSpace * Vector3.Dot(direction, forwardInWorldSpace) / Vector3.Dot(forwardInWorldSpace, forwardInWorldSpace));
    }
}
