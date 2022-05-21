using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private enum AudioStates
    {
        Idle,
        Recording,
        Playing
    }

    [Header("References")]
    private AudioSource audioSource;
    [SerializeField] private Animator animator;
    [SerializeField] private Image backgroundCircle;
    [SerializeField] private Slider pitchSlider;

    [Header("Audio")]
    private AudioClip audioRecorded;
    private float startRecordingTime;
    private bool isRecording = false;
    private float pitch;

    [Header("Stages")]
    private AudioStates audioState = AudioStates.Idle;

    [Header("Animations")]
    private string currentState;
    private const string IDLE = "RECIdle";
    private const string RECORDING = "RECRecording";
    private const string LISTENING = "RECListening";

    private void Awake()
    {
        GetReferences();
        GetPitch();
    }

    private void GetPitch()
    {
        pitch = -pitchSlider.value;
    }

    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
    }

    private void GetReferences()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        SetIdleState();
        SwitchAnimations();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log(audioState);
        }
    }

    private void SwitchAnimations()
    {
        switch (audioState)
        {
            case AudioStates.Idle:
                PlayAnimation(IDLE);
                break;
            case AudioStates.Recording:
                PlayAnimation(RECORDING);
                break;
            case AudioStates.Playing:
                PlayAnimation(LISTENING);
                break;
            default:
                break;
        }
    }

    private void SetIdleState()
    {
        if (!audioSource.isPlaying && !isRecording)
        {
            audioState = AudioStates.Idle;
        }
    }

    public void ChangePitch(Slider slider)
    {
        pitch = -slider.value;
    }

    public void ResetPitch()
    {
        pitchSlider.value = 1;
        pitch = -1;
    }

    public void NewRecord()
    {
        // Switch recording states

        if(audioState != AudioStates.Playing)
        {
            isRecording = !isRecording;
        }

        if (isRecording && audioState == AudioStates.Idle)
        {
            if(!Microphone.IsRecording(""))
            {
                // Setup frequency

                int minFreq;
                int maxFreq;
                int freq = 44100;

                Microphone.GetDeviceCaps("", out minFreq, out maxFreq);

                if (maxFreq < 44100)
                {
                    freq = maxFreq;
                }

                // Start recording

                audioRecorded = Microphone.Start("", false, 300, freq);
                audioState = AudioStates.Recording;
                startRecordingTime = Time.time;
            }
        }

        if(!isRecording && audioState == AudioStates.Recording)
        {
            // End recording

            Microphone.End("");
            audioState = AudioStates.Idle;

            // Setup recording

            AudioClip recordingNew = AudioClip.Create(audioRecorded.name, (int)((Time.time - startRecordingTime) * audioRecorded.frequency), audioRecorded.channels, audioRecorded.frequency, false);
            float[] data = new float[(int)((Time.time - startRecordingTime) * audioRecorded.frequency)];
            audioRecorded.GetData(data, 0);
            recordingNew.SetData(data, 0);
            audioRecorded = recordingNew;

            NewPlay();
        }
    }

    private void NewPlay()
    {
        if (audioRecorded != null && audioState == AudioStates.Idle)
        {
            audioState = AudioStates.Playing;
            audioSource.clip = audioRecorded;
            audioSource.pitch = pitch;
            audioSource.timeSamples = audioRecorded.samples - 1;
            audioSource.Play();
        }
    }

    private void PlayAnimation(string newState)
    {
        if (currentState == newState)
        {
            return;
        }

        animator.Play(newState);
        currentState = newState;
    }
}
