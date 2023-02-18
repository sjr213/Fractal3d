using System.Windows.Media;
using System.Windows;

namespace BasicWpfLibrary;

public static class TreeUtil
{
    public static T? FindParentOfType<T>(this DependencyObject child) where T : DependencyObject
    {
        DependencyObject parentDepObj = child;
        do
        {
            parentDepObj = VisualTreeHelper.GetParent(parentDepObj);
            T? parent = parentDepObj as T;
            if (parent != null)
                return parent;
        }
        while (parentDepObj != null);
        return null;
    }
}

