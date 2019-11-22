using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VR3D;

public class ImageCycler : MonoBehaviour 
{
    public Stereoscopic3DImage[] stereoscopic3DImages;
    public float cycleChangeTime = 5.0f;

    private int currentIndex = 0;
    private VR3DMediaViewer m_3DCanvas;
    private float timer = 0;

    // Use this for initialization
    void Start()
    {
        m_3DCanvas = GetComponent<VR3DMediaViewer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stereoscopic3DImages.Length > 0 && m_3DCanvas != null)
        {
            timer += Time.deltaTime;

            if (timer > cycleChangeTime)
            {
                currentIndex++;
                if (currentIndex > stereoscopic3DImages.Length-1) currentIndex = 0;

                m_3DCanvas.SetNewImage(stereoscopic3DImages[currentIndex]);

                timer = 0;
            }
        }
    }
}
