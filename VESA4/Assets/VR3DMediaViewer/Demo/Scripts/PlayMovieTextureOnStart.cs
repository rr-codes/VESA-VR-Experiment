using UnityEngine;
using System.Collections;

public class PlayMovieTextureOnStart : MonoBehaviour
{
    public bool loop = true;

    // Use this for initialization
    void Start()
    {
#if UNITY_IPHONE
        Debug.LogWarning("MovieTexture is unavailable on iOS. This script has effectively been disabled.");
#elif UNITY_ANDROID
        Debug.LogWarning("MovieTexture is unavailable on Android. This script has effectively been disabled.");
#elif UNITY_WEBGL
        Debug.LogWarning("MovieTexture is unavailable on WebGL. This script has effectively been disabled.");
#else
        Renderer renderer = GetComponent<Renderer>();

        if (renderer)
        {
            MovieTexture movieTexture = renderer.material.mainTexture as MovieTexture;

            if (movieTexture)
            {
                movieTexture.loop = loop;
                movieTexture.Play();
            }
        }
#endif
    }
}