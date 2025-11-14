using AdonisUI.Controls;
namespace vFalcon.Utils;

public class Message
{
    public static void Info(string msg, string? title = null) => 
        MessageBox.Show(msg, title ?? "Info", MessageBoxButton.OK, MessageBoxImage.Information);

    public static void Error(string msg, string? title = null) =>
        MessageBox.Show(msg, title ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);

    public static bool Confirm(string msg, string? title = null)
    {
        return MessageBox.Show(msg, title ?? "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
