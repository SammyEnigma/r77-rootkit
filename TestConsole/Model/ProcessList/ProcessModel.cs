using BytecodeApi.Data;
using BytecodeApi.IO;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace TestConsole.Model;

public sealed class ProcessModel : ObservableObject, IEquatable<ProcessModel>
{
	/// <summary>
	/// The process ID.
	/// </summary>
	public int Id { get; set => Set(ref field, value); }
	/// <summary>
	/// The name of the process.
	/// </summary>
	public string Name { get; set => Set(ref field, value); } = "";
	/// <summary>
	/// A <see cref="bool" /> value, indicating whether the process is 64-bit or 32-bit.
	/// If this value is <see langword="null" />, the bitness could not be determined.
	/// </summary>
	public bool? Is64Bit { get; set => Set(ref field, value); }
	/// <summary>
	/// The integrity level of the process.
	/// If this value is <see langword="null" />, the integrity level could not be determined.
	/// </summary>
	public ProcessIntegrityLevel? IntegrityLevel { get; set => Set(ref field, value); }
	/// <summary>
	/// The username of the process.
	/// If this value is <see langword="null" />, the username could not be determined.
	/// </summary>
	public string User { get; set => Set(ref field, value); } = "";
	/// <summary>
	/// The icon of the executable file.
	/// </summary>
	public Icon? Icon { get; set => Set(ref field, value); }
	/// <summary>
	/// A <see cref="bool" /> value, indicating whether the process can be injected.
	/// </summary>
	public bool CanInject { get; set => Set(ref field, value); }
	/// <summary>
	/// A <see cref="bool" /> value, indicating whether the process is injected.
	/// </summary>
	public bool IsInjected { get; set => Set(ref field, value); }
	/// <summary>
	/// A <see cref="bool" /> value, indicating whether the process is the r77 service process.
	/// </summary>
	public bool IsR77Service { get; set => Set(ref field, value); }
	/// <summary>
	/// A <see cref="bool" /> value, indicating whether the process is an r77 helper process.
	/// </summary>
	public bool IsHelper { get; set => Set(ref field, value); }
	/// <summary>
	/// A <see cref="bool" /> value, indicating whether the process is hidden by ID.
	/// </summary>
	public bool IsHiddenById { get; set => Set(ref field, value); }
	/// <summary>
	/// A <see cref="ProcessStatus" /> value, indicating whether the process is recently created, or recently terminated.
	/// </summary>
	public ProcessStatus Status { get; set => Set(ref field, value); } = ProcessStatus.Running;
	public string SortKey => $"{Name}_{Id}";

	/// <summary>
	/// Initializes a new instance of the <see cref="ProcessModel" /> class.
	/// </summary>
	public ProcessModel()
	{
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="ProcessModel" /> class, and copies all properties from <paramref name="process" />.
	/// </summary>
	/// <param name="process">The <see cref="ProcessModel" /> to copy.</param>
	public ProcessModel(ProcessModel process) : this()
	{
		Id = process.Id;
		Name = process.Name;
		Is64Bit = process.Is64Bit;
		IntegrityLevel = process.IntegrityLevel;
		User = process.User;
		Icon = process.Icon;
		CanInject = process.CanInject;
		IsInjected = process.IsInjected;
		IsR77Service = process.IsR77Service;
		IsHelper = process.IsHelper;
		IsHiddenById = process.IsHiddenById;
		Status = process.Status;
	}

	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj is ProcessModel process && Equals(process);
	}
	public bool Equals([NotNullWhen(true)] ProcessModel? other)
	{
		return
			other != null &&
			Id == other.Id &&
			Name == other.Name &&
			Is64Bit == other.Is64Bit &&
			IntegrityLevel == other.IntegrityLevel &&
			User == other.User &&
			CanInject == other.CanInject &&
			IsInjected == other.IsInjected &&
			IsR77Service == other.IsR77Service &&
			IsHelper == other.IsHelper &&
			IsHiddenById == other.IsHiddenById &&
			Status == other.Status;
	}
	public override int GetHashCode()
	{
		return HashCode.Combine(Id, Name, Is64Bit, IntegrityLevel, User, CanInject, IsInjected, HashCode.Combine(IsR77Service, IsHelper, IsHiddenById, Status));
	}
}