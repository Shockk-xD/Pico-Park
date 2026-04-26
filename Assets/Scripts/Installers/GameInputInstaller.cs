using Zenject;

public class GameInputInstaller : MonoInstaller
{
    public override void InstallBindings() {
        InstallInputSystem();
    }

    private void InstallInputSystem() {
        var gameInput = new GameInput();
        Container.Bind<GameInput>()
            .FromInstance(gameInput)
            .AsSingle();
    }
}
