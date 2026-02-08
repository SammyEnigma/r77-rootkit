using BytecodeApi;
using BytecodeApi.Wpf;
using TestConsole.Helper;
using TestConsole.Model;

namespace TestConsole;

public sealed class R77ServiceUserControlViewModel : ViewModel
{
	public static R77ServiceUserControlViewModel? Singleton { get; private set; }
	public R77ServiceUserControl View { get; set; }

	public DelegateCommand OpenControlPipeTabPageCommand => field ??= new(OpenControlPipeTabPageCommand_Execute);

	public bool IsR77ServiceRunning { get; set => Set(ref field, value); }

	public R77ServiceUserControlViewModel(R77ServiceUserControl view)
	{
		Singleton = this;
		View = view;

		BeginUpdate();
	}

	private async void BeginUpdate()
	{
		while (true)
		{
			await Task.Run(() =>
			{
				if (ApplicationBase.Process.IsElevated)
				{
					// When elevated, check if the service process is running.
					IsR77ServiceRunning = ProcessListUserControlViewModel.Singleton?.Processes.Any(process => process.IsR77Service) == true;
				}
				else
				{
					// When not elevated, test the control pipe availability by attempting to connect.
					IsR77ServiceRunning = ControlPipe.IsAvailable;
				}
			});

			await Task.Delay(1000);
		}
	}

	private void OpenControlPipeTabPageCommand_Execute()
	{
		MainWindowViewModel.Singleton?.View.OpenControlPipeTabPage();
	}
}