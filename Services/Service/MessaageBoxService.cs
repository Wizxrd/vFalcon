using AdonisUI.Controls;
namespace vFalcon.Services.Service;

public static class MessageBoxService
{
    public static void Info(string t, string? title = null) =>
        MessageBox.Show(t, title ?? "Info", MessageBoxButton.OK, MessageBoxImage.Information);

    public static void Error(string t, string? title = null) =>
        MessageBox.Show(t, title ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);

    public static bool Confirm(string t, string? title = null) =>
        MessageBox.Show(t, title ?? "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
