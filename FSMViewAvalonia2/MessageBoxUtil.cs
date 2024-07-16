

using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace FSMViewAvalonia2;

public static class MessageBoxUtil
{
    public static async Task<ButtonResult> ShowDialog(Window window, string header, string message) => await ShowDialog(window, header, message, ButtonEnum.Ok);

    public static async Task<ButtonResult> ShowDialog(Window window, string header, string message, ButtonEnum buttons) => await MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
    {
        ButtonDefinitions = buttons,
        ContentHeader = header,
        ContentMessage = message
    }).ShowWindowDialogAsync(window);

    public static async Task<string> ShowDialogCustom(Window window, string header, string message, params string[] buttons)
    {
        var definitions = new ButtonDefinition[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            definitions[i] = new ButtonDefinition { Name = buttons[i]};
        }

        return await MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentHeader = header,
            ContentMessage = message,
            ButtonDefinitions = definitions
        }).ShowWindowDialogAsync(window);
    }
}
