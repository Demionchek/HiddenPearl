using Camera;
using Player;
using Zenject;

namespace DefaultNamespace.DependencyInjection
{
    public class IndustialSceneInstaller : MonoInstaller
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