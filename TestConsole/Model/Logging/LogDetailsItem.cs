namespace TestConsole.Model;

public sealed class LogDetailsItem : LogItem
{
	public string Text { get; }

	public LogDetailsItem(string text)
	{
		Text = text;
	}

	public override string ToString()
	{
		return Text;
	}
}