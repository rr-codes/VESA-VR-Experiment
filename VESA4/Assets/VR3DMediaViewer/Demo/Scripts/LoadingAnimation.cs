#if !UNITY_4 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_2 && !UNITY_5_4 && !UNITY_5_5
#define UNITY_5_6_PLUS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_5_6_PLUS
using UnityEngine.Video;
#endif

public class LoadingAnimation : MonoBehaviour 
{
#if UNITY_5_6_PLUS
    [SerializeField]
    private VideoPlayer videoPlayer;
#endif

    [SerializeField]
    private Image circle;

    [SerializeField]
    private Image circle2;

    public float speed = 0.1f;

    private Coroutine coroutine;

#if UNITY_5_6_PLUS
	// Use this for initialization
	void Start () 
    {
        circle.color = Random.ColorHSV();
        circle2.color = Random.ColorHSV();

        coroutine = StartCoroutine(_LoadingAnimation());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator _LoadingAnimation()
    {
        circle.enabled = true;
        circle2.enabled = true;

        float t = 0;
        while (true)
        {
            if (t >= 1)
            {
                t = 0;
                circle2.color = circle.color;
                circle.color = Random.ColorHSV();
            }

            t += Time.deltaTime * speed;
            float amount = Mathf.Lerp(0, 1, t);
            circle.fillAmount = amount;
            circle2.fillAmount = 1.0f - amount;
            yield return 0;
        }
    }

    void PrepareComplete(VideoPlayer videoPlayer)
    {
        StopCoroutine(coroutine);

        circle.enabled = false;
        circle2.enabled = false;
    }

    void OnEnable()
    {
        videoPlayer.prepareCompleted += PrepareComplete;
    }

    void OnDisable()
    {
        videoPlayer.prepareCompleted -= PrepareComplete;
    }
#endif
}
