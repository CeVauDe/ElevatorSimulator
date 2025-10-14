using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ElevatorSimulator.ViewModels;

namespace ElevatorSimulator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.AllVisiblePersons.CollectionChanged += OnPersonsCollectionChanged;
        }
    }

    private void OnPersonsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Clear and redraw all persons
        PersonsCanvas.Children.Clear();

        if (DataContext is MainWindowViewModel viewModel)
        {
            foreach (var person in viewModel.AllVisiblePersons)
            {
                var ellipse = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Fill = Brushes.Orange,
                    Stroke = Brushes.DarkOrange,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(ellipse, person.X);
                Canvas.SetTop(ellipse, person.Y);

                ToolTip.SetTip(ellipse, $"Person {person.Id}");

                PersonsCanvas.Children.Add(ellipse);

                // Add text label showing target floor
                var label = new TextBlock
                {
                    Text = person.TargetFloor.ToString(),
                    FontSize = 10,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                Canvas.SetLeft(label, person.X + 5);
                Canvas.SetTop(label, person.Y + 3);

                PersonsCanvas.Children.Add(label);
            }
        }
    }
}