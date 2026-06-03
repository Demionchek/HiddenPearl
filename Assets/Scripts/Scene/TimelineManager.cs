using System.Collections.Generic;
using System.Linq;
using Camera;
using Player;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using Zenject;

public class TimelineManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private float _skipHoldDuration = 3f;
    [SerializeField] private List<int> _cutscenesAllowMovement;

    [Header("Cutscenes")]
    [SerializeField] private PlayableDirector[] _cutscenes;

    [Inject]
    private InputHandler _inputHandler;
    [Inject]
    private CameraController _cameraController;

    private PlayableDirector _currentCutscene;
    private int _currentCutsceneIndex = 0;
    private float _skipHoldTimer = 0f;
    private bool _isHoldingSkip = false;
    public bool _isPlayingSequence = false;

    private double pauseTime = 0;

    private void Start()
    {
        if (_playOnStart && _cutscenes.Length > 0)
        {
            PlayCutsceneSequence();
        }
    }

    private void Update()
    {
        HandleSkipInput();
    }

    private void HandleSkipInput()
    {
        if (_currentCutscene == null || _currentCutscene.state != PlayState.Playing)
            return;

        if (_inputHandler == null) return;

        if (_inputHandler.JumpPressed)
        {
            _isHoldingSkip = true;
            _skipHoldTimer += Time.deltaTime;

            if (_skipHoldTimer >= _skipHoldDuration)
            {
                SkipCurrentCutscene();
                _isHoldingSkip = false;
                _skipHoldTimer = 0f;
            }
        }
        else if (_isHoldingSkip)
        {
            _isHoldingSkip = false;
            _skipHoldTimer = 0f;
        }
    }

    public void PlayCutscene(int index)
    {
        if (index < 0 || index >= _cutscenes.Length)
        {
            Debug.LogWarning($"Cutscene index {index} is out of range!");
            return;
        }

        StopCurrentCutscene();

        _currentCutsceneIndex = index;
        _currentCutscene = _cutscenes[index];
        _currentCutscene.Play();
    }

    public void PlayCutsceneSequence()
    {
        if (_cutscenes.Length == 0)
            return;

        //_isPlayingSequence = true;
        PlayCutscene(0);
    }

    public void SkipCurrentCutscene()
    {
        if (_currentCutscene == null)
            return;

        _currentCutscene.Stop();
        OnCutsceneFinished();
    }

    private void StopCurrentCutscene()
    {
        if (_currentCutscene != null && _currentCutscene.state == PlayState.Playing)
        {
            _currentCutscene.Stop();
            _currentCutscene = null;
        }
    }

    public void OnCutsceneFinished()
    {
        if (_isPlayingSequence)
        {
            _currentCutsceneIndex++;
            if (_currentCutsceneIndex < _cutscenes.Length)
            {
                PlayCutscene(_currentCutsceneIndex);
            }
            else
            {
                _isPlayingSequence = false;
            }
        }
    }

    public bool IsCutscenePlaying()
    {
        return _currentCutscene != null && _currentCutscene.state == PlayState.Playing;
    }

    public bool IsCutsceneAllowMovement()
    {
        bool isPlaying = IsCutscenePlaying();
        bool isAllowMovement = _cutscenes.Length > 0 && _cutscenesAllowMovement.Contains(_currentCutsceneIndex);
        return !isPlaying || isAllowMovement;
    }

    public void PauseCurrentCutscene()
    {
        if (!IsCutscenePlaying())
            return;

        pauseTime = _currentCutscene.time;
        _currentCutscene.Pause();
        _currentCutscene.time = pauseTime;
    }

    public void ResumeCurrentCutscene()
    {
        if (IsCutscenePaused())
        {
            _currentCutscene.Resume();
            _currentCutscene.time = pauseTime;
        }
    }

    public bool IsCutscenePaused() => _currentCutscene != null && _currentCutscene.state == PlayState.Paused;

    public PlayableDirector GetCutscene() => _currentCutscene;
}
