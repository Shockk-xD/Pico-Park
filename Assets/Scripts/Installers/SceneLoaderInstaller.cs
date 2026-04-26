using Zenject;

public class SceneLoaderInstaller : MonoInstaller
{
    public override void InstallBindings() {
        InstallSceneLoader();
    }

    private void InstallSceneLoader() {
        Container.Bind<SceneLoader>()
                    .FromComponentInHierarchy()
                    .AsSingle();
    }
}
