namespace LambdaTriggers.Mobile;

class App(AppShell shell) : Application
{
	readonly AppShell _appShell = shell;

	protected override Window CreateWindow(IActivationState? activationState) => new(_appShell);
}