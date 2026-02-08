using BytecodeApi.Extensions;
using BytecodeApi.Wpf;
using System.Collections.ObjectModel;
using TestConsole.Helper;
using TestConsole.Model;

namespace TestConsole;

public sealed class ProcessListUserControlViewModel : ViewModel
{
	public static ProcessListUserControlViewModel? Singleton { get; private set; }
	public ProcessListUserControl View { get; set; }

	public DelegateCommand<ProcessModel> InjectCommand => field ??= new(InjectCommand_Execute!);
	public DelegateCommand<ProcessModel> DetachCommand => field ??= new(DetachCommand_Execute!);
	public DelegateCommand<ProcessModel> HideCommand => field ??= new(HideCommand_Execute!);
	public DelegateCommand<ProcessModel> UnhideCommand => field ??= new(UnhideCommand_Execute!);

	public ObservableCollection<ProcessModel> Processes { get; set => Set(ref field, value); } = [];
	public ProcessModel? SelectedProcess { get; set => Set(ref field, value); }

	public ProcessListUserControlViewModel(ProcessListUserControl view)
	{
		Singleton = this;
		View = view;

		BeginUpdate();
	}

	private async void BeginUpdate()
	{
		while (true)
		{
			await Update();
			await Task.Delay(1000);
		}
	}
	public async Task Update()
	{
		await Task.Run(() =>
		{
			// Retrieve the new process list and mark processes as newly created or terminated based on the previous list.
			ObservableCollection<ProcessModel> newProcesses = ProcessList
				.GetProcesses()
				.Each(process => process.Status = Processes.Any() && Processes.None(p => p.Id == process.Id) ? ProcessStatus.New : ProcessStatus.Running)
				.ToObservableCollection();

			newProcesses.AddRange(
				Processes
					.Where(process => process.Status != ProcessStatus.Terminated)
					.Where(process => newProcesses.None(p => p.Id == process.Id))
					.Select(process => new ProcessModel(process))
					.Each(process => process.Status = ProcessStatus.Terminated)
			);

			// Only update the list only, if it has changed.
			bool updated = false;
			if (newProcesses.Count == Processes.Count)
			{
				for (int i = 0; i < newProcesses.Count; i++)
				{
					if (!newProcesses[i].Equals(Processes[i]))
					{
						updated = true;
						break;
					}
				}
			}
			else
			{
				updated = true;
			}

			if (updated)
			{
				ProcessModel? newSelectedProcess = newProcesses.FirstOrDefault(process => process.Id == SelectedProcess?.Id);

				int oldSelectedIndex = Processes
					.OrderBy(process => process.Name, StringComparer.OrdinalIgnoreCase)
					.ThenBy(process => process.Id)
					.IndexOf(SelectedProcess);

				int newSelectedIndex = newProcesses
					.OrderBy(process => process.Name, StringComparer.OrdinalIgnoreCase)
					.ThenBy(process => process.Id)
					.IndexOf(newSelectedProcess);

				View.Dispatch(() =>
				{
					int oldScrollOffset = View.ProcessListScrollOffset;

					SelectedProcess = null;
					Processes = newProcesses;
					SelectedProcess = newSelectedProcess;

					if (newSelectedProcess != null)
					{
						View.ProcessListScrollOffset = oldScrollOffset + newSelectedIndex - oldSelectedIndex;
					}
				});
			}
		});
	}

	private async void InjectCommand_Execute(ProcessModel process)
	{
		if (await ProcessList.Inject(process))
		{
			process.IsInjected = true;
		}

		await Update();
	}
	private async void DetachCommand_Execute(ProcessModel process)
	{
		if (await ProcessList.Detach(process))
		{
			process.IsInjected = false;
		}

		await Update();
	}
	private async void HideCommand_Execute(ProcessModel process)
	{
		if (await ProcessList.Hide(process))
		{
			process.IsHiddenById = true;
		}

		ConfigSystemUserControlViewModel.Singleton?.Update();
		await Update();
	}
	private async void UnhideCommand_Execute(ProcessModel process)
	{
		if (await ProcessList.Unhide(process))
		{
			process.IsHiddenById = false;
		}

		ConfigSystemUserControlViewModel.Singleton?.Update();
		await Update();
	}
}