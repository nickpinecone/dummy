using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Dummy;

public class UIManager
{
    private Label _status;
    private Image _image;

    public TextBlock Answer { get; private set; }
    public DockPanel Stack { get; private set; }

    public UIManager()
    {
        _status = new Label() {
            FontSize = 32,
            Padding = new Thickness(16),
        };

        _image = new Image() {
            Stretch = Stretch.Uniform,
            Width = 600,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        Answer = new TextBlock() {
            FontSize = 24,
            TextWrapping = TextWrapping.Wrap,
            Padding = new Thickness(16),
        };

        Stack = new DockPanel() { VerticalAlignment = VerticalAlignment.Stretch };

        DockPanel.SetDock(_status, Dock.Top);
        DockPanel.SetDock(Answer, Dock.Top);
        DockPanel.SetDock(_image, Dock.Top);

        Stack.Children.Add(_status);
        Stack.Children.Add(Answer);
        Stack.Children.Add(_image);
    }

    public void SetState(State state, string? text = null)
    {
        if (text != null)
        {
            Answer.Text = text;
        }

        if (state == State.Wait)
        {
            _status.Content = "Ждет вопроса";
            Bitmap bitmap = new Bitmap("Static/wait.jpg");
            _image.Source = bitmap;
        }
        else if (state == State.Record)
        {
            _status.Content = "Слушает";
            var bitmap = new Bitmap("Static/record.jpg");
            _image.Source = bitmap;
        }
        else if (state == State.Generate)
        {
            _status.Content = "Думает...";
            var bitmap = new Bitmap("Static/generate.jpg");
            _image.Source = bitmap;
        }
        else if (state == State.Ready)
        {
            _status.Content = "Готов!";
            var bitmap = new Bitmap("Static/ready.jpg");
            _image.Source = bitmap;
        }
        else if (state == State.Speak)
        {
            _status.Content = "Говорит";
            var bitmap = new Bitmap("Static/speak.jpg");
            _image.Source = bitmap;
        }
    }
}
