using BytecodeApi.Data;
using System.Collections.ObjectModel;

namespace TestConsole.Model;

public class TreeViewNode : ObservableObject
{
	public string Header { get; set => Set(ref field, value); } = "";
	public string? IconCollapsed { get; set => Set(ref field, value); }
	public string? IconExpanded { get; set => Set(ref field, value); }
	public bool IsSelected { get; set => Set(ref field, value); }
	public bool IsExpanded { get; set => Set(ref field, value && Children.Any()); }
	public ObservableCollection<TreeViewNode> Children { get; set => Set(ref field, value); } = [];

	public TreeViewNode(string header, string? icon) : this(header, icon, icon)
	{
	}
	public TreeViewNode(string header, string? iconCollapsed, string? iconExpanded)
	{
		Header = header;
		IconCollapsed = iconCollapsed;
		IconExpanded = iconExpanded;
	}
	public TreeViewNode(string header, string? icon, params IEnumerable<TreeViewNode> children) : this(header, icon, icon, children)
	{
	}
	public TreeViewNode(string header, string? iconCollapsed, string? iconExpanded, params IEnumerable<TreeViewNode> children) : this(header, iconCollapsed, iconExpanded)
	{
		Children = new(children);
		IsExpanded = Children.Any();
	}
}