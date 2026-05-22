using Animations;
using Camera;
using Player;
using UnityEngine;
using Zenject;

namespace DefaultNamespace.DependencyInjection
{
    public class SceneInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            if (FindAnyObjectByType<TimelineManager>() != null)
                Container.Bind<TimelineManager>().FromComponentInHierarchy().AsSingle();
            if (FindAnyObjectByType<PlayerController>() != null)
                Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
            if (FindAnyObjectByType<CameraController>() != null)
                Container.Bind<CameraController>().FromComponentInHierarchy().AsSingle();
            if (FindAnyObjectByType<InputHandler>() != null)
                Container.Bind<InputHandler>().FromComponentInHierarchy().AsSingle();
            if (FindAnyObjectByType<DialogueSystem>() != null)
                Container.Bind<DialogueSystem>().FromComponentInHierarchy().AsSingle();
            if (FindAnyObjectByType<CheckPoints>() != null)
                Container.Bind<CheckPoints>().FromComponentInHierarchy().AsSingle();
        }
    }
}