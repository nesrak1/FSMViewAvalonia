

namespace FSMViewAvalonia2;

public static class MessageBoxUtil
{
    public static async Task<ButtonResult> ShowDialog(Window window, string header, string message) => await ShowDialog(window, header, message, ButtonEnum.Ok);

    public static async Task<ButtonResult> ShowDialog(Window window, string header, string message, ButtonEnum buttons) => await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
    {
        ButtonDefinitions = buttons,
        Style = Style.Windows,
        ContentHeader = header,
        ContentMessage = message
    }).ShowDialog(window);

    public static async Task<string> ShowDialogCustom(Window window, string header, string message, params string[] buttons)
    {
        var definitions = new ButtonDefinition[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            definitions[i] = new ButtonDefinition { Name = buttons[i], Type = ButtonType.Default };
        }

        return await MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxCustomParams
        {
            Style = Style.Windows,
            ContentHeader = header,
            ContentMessage = message,
            ButtonDefinitions = definitions
        }).ShowDialog(window);
    }
}
