using BytecodeApi.Wpf;
using TestConsole.Model;

namespace TestConsole;

public sealed class ConfigSystemEntryDialogViewModel : ViewModel
{
	public ConfigSystemEntryDialog View { get; set; }

	public DelegateCommand SaveCommand => field ??= new(SaveCommand_Execute);

	public bool IsCreate { get; set => Set(ref field, value); }
	public string? Name { get; set => Set(ref field, value); }
	public string? Value { get; set => Set(ref field, value); }

	public ConfigSystemEntryDialogViewModel(ConfigSystemEntryDialog view)
	{
		View = view;
	}

	private void SaveCommand_Execute()
	{
		View.DialogResult = true;
	}
}