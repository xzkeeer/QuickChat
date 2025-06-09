using System.Windows;
using System.Windows.Controls;

public class InputDialog : Window
{
    public string Answer { get; private set; }

    public InputDialog(string title, string prompt)
    {
        Title = title;
        Width = 300;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var stackPanel = new StackPanel { Margin = new Thickness(10) };

        stackPanel.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10) });

        var textBox = new TextBox();
        stackPanel.Children.Add(textBox);

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };

        var okButton = new Button { Content = "OK", IsDefault = true, Width = 80 };
        okButton.Click += (_, __) => { Answer = textBox.Text; DialogResult = true; };
        buttonPanel.Children.Add(okButton);

        var cancelButton = new Button { Content = "Cancel", IsCancel = true, Width = 80, Margin = new Thickness(5, 0, 0, 0) };
        buttonPanel.Children.Add(cancelButton);

        stackPanel.Children.Add(buttonPanel);

        Content = stackPanel;
    }
}