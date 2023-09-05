using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace KungFuArchiveEditor.ViewModels;

public class MessageTipViewModel : BagViewModel
{
    public string Title { get; set; } = "提示";
    public string Message { get; set; } = "hello";
    public IBrush TextColor { get; set; } = Brushes.Black;
    public string TextColorHex
    {
        set
        {
            var color = Color.Parse(value);
            var brush = new ImmutableSolidColorBrush(color);
            TextColor = brush;
        }
    }
}
