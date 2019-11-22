using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This lets you set a looping series of movements/rotations to be performed on this GameObject over time. Good for cinematic style camera shots.
/// </summary>
public class ObjectMotionsOverTime : MonoBehaviour 
{    
    [System.Serializable]
    public class GameObjectMotion : System.Object
    {
        public Vector3 startPosition;
        public Vector3 endPosition;
        public Vector3 startRotation;
        public Vector3 endRotation;
        public float duration;
    }

    [Tooltip("Randomly cycle through the different motions.")]
    public bool randomOrder = false;    

    [Tooltip("Here you can set up the simple motions to perform on your gameObject over time.")]
    public List<GameObjectMotion> gameObjectMotions = new List<GameObjectMotion>();

    private int motionNumber = 0;
    
    private Coroutine coroutine;

    public bool paused = false;
 
    // Use this for initialization
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        // Start a new GameObject motion routine if ther isnt one active.
        if (coroutine == null)
            MoveGameObject();
    }
    
    /// <summary>
    /// Starts the next motion routine for the gameObject.
    /// </summary>
    void MoveGameObject()
    {
        // Don't need to do anthing if there arent any motions set.
        if (gameObjectMotions.Count != 0)
        {
            // Detirmine the next GameObject motion to use.
            if (randomOrder)
                motionNumber = Random.Range(0, gameObjectMotions.Count);
            else if (motionNumber >= gameObjectMotions.Count)
                motionNumber = 0;

            // End any previous coroutines, just in case.
            if (coroutine != null)
                StopCoroutine(coroutine);

            // Start our TransformFromTo routine.
            coroutine = StartCoroutine(TransformFromTo(gameObjectMotions[motionNumber].startPosition, gameObjectMotions[motionNumber].endPosition, gameObjectMotions[motionNumber].startRotation, gameObjectMotions[motionNumber].endRotation, gameObjectMotions[motionNumber].duration));
        }
    }

    /// <summary>
    /// Move this transform from pointA to PointB and rotate transfrom from rotationA to rotationB over time.
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="rotationA"></param>
    /// <param name="rotationB"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator TransformFromTo(Vector3 pointA, Vector3 pointB, Vector3 rotationA, Vector3 rotationB, float time)
    {
        float lerpPosition = 0.0f;

        // 0 = pointA/rotationA, 1.0f = pointB/rotationB
        while (lerpPosition <= 1.0f)
        {
            if (!paused)
            {
                // We incriment the the postion/rotation along every frame until the transform reaches the goal.
                lerpPosition += Time.deltaTime / time;
                transform.localPosition = Vector3.Lerp(pointA, pointB, lerpPosition);
                transform.localEulerAngles = Vector3.Lerp(rotationA, rotationB, lerpPosition);
            }

            yield return 0;
        }
        
        coroutine = null;

        // Move to the next motion set.
        if (!randomOrder)
            motionNumber++;
    }    
}


