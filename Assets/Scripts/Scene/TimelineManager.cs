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

    private void Start()
    {
        if (_playOnStart && _cutscenes.Length > 0)
        {
            //_cameraController.SwitchVirtualCamera(false);
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

    // Публичный метод для запуска конкретной катсцены по индексу
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

    // Публичный метод для запуска последовательности катсцен
    public void PlayCutsceneSequence()
    {
        if (_cutscenes.Length == 0)
            return;

        //_isPlayingSequence = true;
        PlayCutscene(0);
    }

    // Публичный метод для пропуска текущей катсцены
    public void SkipCurrentCutscene()
    {
        if (_currentCutscene == null)
            return;

        _currentCutscene.Stop();
        OnCutsceneFinished();
    }

    // Остановка текущей катсцены
    private void StopCurrentCutscene()
    {
        if (_currentCutscene != null && _currentCutscene.state == PlayState.Playing)
        {
            _currentCutscene.Stop();
            _currentCutscene = null;
        }
    }

    // Вызывается по окончании катсцены (можно подключить через событие в Timeline)
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

    // Публичный метод для проверки, играет ли сейчас какая-либо катсцена
    public bool IsCutscenePlaying()
    {
        return _currentCutscene != null && _currentCutscene.state == PlayState.Playing;
    }

    public bool PauseCurrentCutscene()
    {
        if (!IsCutscenePlaying())
            return false;

        _currentCutscene.Pause();
        return true;
    }

    public void ResumeCurrentCutscene()
    {
        if (IsCutscenePaused())
            _currentCutscene.Resume();
    }

    public bool IsCutscenePaused() => _currentCutscene != null && _currentCutscene.state == PlayState.Paused;

    public PlayableDirector GetCutscene() => _currentCutscene;
}
