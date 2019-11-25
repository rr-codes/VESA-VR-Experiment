using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.Serialization;
using UnityEngine.UI;

public struct ImageSet
{
    public Texture2D OriginalLeft, OriginalRight;
    public Texture2D AlternateLeft, AlternateRight;
}

public struct Trial
{
    public string ImageName;
    public ImageSet ImageSet;
    public Vector2 Position;

    public bool ShouldFlicker;

    public bool ResponseIsCorrect;
    public float ResponseDuration;
}

public enum Gender
{
    Male, Female, Other
}

public struct Participant
{
    public string id;
    public int Age;
    public Gender Gender;
}

public struct Run
{
    public string OriginalImagesDirectory;
    public int SessionNumber;
    public Participant Participant;
    public Queue<Trial> Trials;
}

public enum State
{
    Start, InTrial, BetweenTrials, Finish
}

public class Controller : MonoBehaviour
{
    private Run _run;
    private State _currentState;
    private float _timer;

    private static readonly Vector2 _imageSize = new Vector2(1200, 1000);
    private static readonly float _flickerRate = 0.1f;
    private static readonly float _trialDuration = 8.0f;

    private ImageSet currentImageSet;
    
    public string configurationFilePath;
    public GameObject canvas;
    public GameObject startView;
    
    // Start is called before the first frame update
    void Start()
    {
        _timer = 0.0f;
        _run = new Run();
        _currentState = State.Start;

        var file = File.ReadAllLines(configurationFilePath);
        InitializeHeader(lines: file);
        EnqueueTrials(5, file);

        currentImageSet = _run.Trials.Dequeue().ImageSet;
    }

    private void EnqueueTrials(int startLine, params string[] lines)
    {
        // directory | name | x | y | shouldFlicker
        for (int i = startLine; i < lines.Length; i++)
        {
            var trial = new Trial();
            var split = lines[i].Split(new[]{", "}, StringSplitOptions.RemoveEmptyEntries);

            trial.ImageName = split[1];
            trial.Position = new Vector2(int.Parse(split[2]), int.Parse(split[3]));

            var paths = new string[]
            {
                _run.OriginalImagesDirectory + "/" + trial.ImageName + "left_orig.ppm",
                _run.OriginalImagesDirectory + "/" + trial.ImageName + "right_orig.ppm",
                split[0] + "/" + trial.ImageName + "right_dec.ppm",
                split[0] + "/" + trial.ImageName + "left_dec.ppm"
            };

            const ImreadModes flag = ImreadModes.AnyColor | ImreadModes.AnyDepth;

            var set = new ImageSet
            {
                OriginalLeft   = MatToTexture(Cv2.ImRead(paths[0], flag), trial.Position),
                OriginalRight  = MatToTexture(Cv2.ImRead(paths[1], flag), trial.Position),
                AlternateLeft  = MatToTexture(Cv2.ImRead(paths[2], flag), trial.Position),
                AlternateRight = MatToTexture(Cv2.ImRead(paths[3], flag), trial.Position)
            };

            trial.ImageSet = set;
            trial.ShouldFlicker = bool.Parse(split[5]);
            
            _run.Trials.Enqueue(trial);
        }
    }

    private void InitializeHeader(params string[] lines)
    {
        _run.SessionNumber = int.Parse(lines[0]);

        var p = new Participant
        {
            id = lines[1], 
            Age = int.Parse(lines[2])
        };
        
        Enum.TryParse(lines[3], true, out p.Gender);

        _run.Participant = p;
        _run.OriginalImagesDirectory = lines[4];
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentState)
        {
            case State.Start:
            {
                if (!Input.GetKeyDown(KeyCode.Return)) return;
                
                startView.GetComponent<MeshRenderer>().enabled = false;
                _currentState = State.InTrial;
                return;
            }

            case State.InTrial:
            {
                _timer += Time.deltaTime;

                if (_timer > _trialDuration)
                {
                    _currentState = State.BetweenTrials;
                    return;
                }
            }
        }
    }


    static Texture2D MatToTexture(Mat mat, Vector2 position)
    {
        var h = mat.Height;
        var w = mat.Width;

        var data = new byte[h * w];
        mat.GetArray(0, 0, data);
        
        var colors = new Color[h * w];
        Parallel.For(0, h, i =>
        {
            for (var j = 0; j < w; j++)
            {
                var vec = data[j + i * w];
                var color = new Color(vec, vec, vec, 0);
                colors[j + i * w] = color;
            }
        });;
        
        var texture = new Texture2D(w, h, TextureFormat.RGBAHalf, true, true);
        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }
}
