using System.Text;
using SU_Lore.Database.Models.Pages;

namespace SU_Lore.Data;

public class TreeNode
{
    public string Name { get; set; }
    public List<TreeNode> Children { get; set; } = new();
    public Page? Page { get; set; }
    
    public TreeNode FindOrAddChild(string name)
    {
        var child = Children.FirstOrDefault(c => c.Name == name);
        if (child == null)
        {
            child = new TreeNode(name);
            Children.Add(child);
        }

        SortChildrenAlphabetically();

        return child;
    }
    
    public TreeNode(string name)
    {
        Name = name;
    }
    
    
    public void SortChildrenAlphabetically()
    {
        Children.Sort((node1, node2) => string.Compare(node1.Name, node2.Name, StringComparison.Ordinal));
    }
    
    public static TreeNode BuildTree(List<Page> pages)
    {
        var root = new TreeNode("root");

        foreach (var page in pages)
        {
            var current = root;
            var parts = page.VirtualPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                current = current.FindOrAddChild(part);
            }
            current.Page = page;
        }

        return root;
    }

    public static void GetTree(TreeNode node, ref StringBuilder sb, string indent = "", bool isLast = true)
    {
        sb.Append(indent);
        if (node.Page != null && node.Page.Flags.HasFlag(PageFlagType.Unlisted))
        {
            sb.Append("[color=bloodred]");
        }
        
        if (isLast)
        {
            sb.Append("└──");
            indent += "   ";
        }
        else
        {
            sb.Append("├──");
            indent += "│  ";
        }

        if (node.Page != null)
        {
            sb.AppendLine($"[button={node.Page.VirtualPath};{node.Name}]");
        }
        else
        {
            sb.AppendLine(node.Name);
        }
        
        if (node.Page != null && node.Page.Flags.HasFlag(PageFlagType.Unlisted))
        {
            sb.Append("[/color]");
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            GetTree(node.Children[i], ref sb, indent, i == node.Children.Count - 1);
        }
    }
}