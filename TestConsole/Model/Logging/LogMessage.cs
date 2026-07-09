using BytecodeApi.Extensions;

namespace TestConsole.Model;

public sealed class LogMessage
{
	public DateTime TimeStamp { get; }
	public LogMessageType Type { get; }
	public LogItem[] Items { get; }
	public string Text { get; }

	public LogMessage(LogMessageType type, LogItem?[] items)
	{
		TimeStamp = DateTime.Now;
		Type = type;
		Items = items.ExceptNull().ToArray();
		Text = Items.Select(item => item.ToString() + (item.NoSpacing ? null : " ")).AsString();
	}
}