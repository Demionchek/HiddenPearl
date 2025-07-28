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
            Container.Bind<TimelineManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CameraController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<InputHandler>().FromComponentInHierarchy().AsSingle();
            Container.Bind<DialogueSystem>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CheckPoints>().FromComponentInHierarchy().AsSingle();
        }
    }
}