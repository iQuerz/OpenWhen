using Microsoft.EntityFrameworkCore;
using Open_When.Models;
using Open_When.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Open_When
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AppDbContext _dbContext { get; set; }
        private bool _canOpenLetter { get; set; }
        public bool CanOpenLetter
        {
            get { return _canOpenLetter; }
            set
            {
                _canOpenLetter = value;
                renderData();
            }
        }

        public Settings _settings { get; set; }

        private void setupDb()
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            _dbContext.Letters.Load();
            _dbContext.Settings.Load();
            _settings = _dbContext.Settings.FirstOrDefault();
        }

        public MainWindow()
        {
            InitializeComponent();
            _canOpenLetter = false;
            areOpenedLettersVisible = false;
        }
        private void startTimer()
        {
            timer_tick(null, new());
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_tick;
            timer.Start();
        }
        private void timer_tick(object? sender, EventArgs e)
        {
            var shouldBeOpenDate = _settings.LastDocOpenDate + _settings.DocTimeout;
            var now = DateTime.Now;

            if (shouldBeOpenDate < now)
            {
                AvailableLabel.Text = "Available now";
                AvailableLabel.Foreground = Brushes.ForestGreen;
                if (!CanOpenLetter) CanOpenLetter = true;
            }
            else
            {
                AvailableLabel.Text = getTimespanText(shouldBeOpenDate - now);
                AvailableLabel.Foreground = Brushes.MediumVioletRed;
                if (CanOpenLetter) CanOpenLetter = false;
            }
        }

        private void renderData()
        {
            MainWrapPanel.Children.Clear();
            var docs = _dbContext.Letters;
            foreach (var doc in docs)
            {
                DocumentCard card = new DocumentCard()
                {
                    Title = doc.Title,
                    Letter = doc,
                    MainWindow = this,
                    Margin = new Thickness(20)
                };
                if (doc.Opened && areOpenedLettersVisible)
                {
                    MainWrapPanel.Children.Add(card);
                }
                else if (!doc.Opened && areClosedLettersVisible)
                {
                    if (!CanOpenLetter)
                    {
                        card.IsEnabled = false;
                        card.Opacity = 0.5;
                    }
                    MainWrapPanel.Children.Add(card);
                }
            }
        }

        public void openLetter(Letter letter)
        {
            if (areClosedLettersVisible)
            {
                var result = MessageBox.Show(this, $"Opening a letter will reset your cooldown. You will have to wait 7 days in order to open the next letter. Are you really sure you want to read \"{letter.Title}\"?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
            }

            LetterTitle.Text = letter.Title;
            LetterText.Text = letter.Text;

            DoubleAnimation animationX = new(0, 1, TimeSpan.FromMilliseconds(150));
            DoubleAnimation animationY = new(0, 1, TimeSpan.FromMilliseconds(150));
            LetterBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animationX);
            LetterBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animationY);

            DoubleAnimation opacityAnimation = new(0, 0.5, TimeSpan.FromMilliseconds(150));
            LetterBackground.BeginAnimation(OpacityProperty, opacityAnimation);

            Panel.SetZIndex(MainPanel, 1);
            Panel.SetZIndex(LetterPanel, 2);

            if (areOpenedLettersVisible) return; //ensures that we don't mess with data when we're looking at already opened letters

            letter.Opened = true;
            var settings = _dbContext.Settings.FirstOrDefault();
            settings.LastDocOpenDate = DateTime.Now;

            _dbContext.SaveChanges();
            _settings = _dbContext.Settings.FirstOrDefault();
        }

        public void closeLetter()
        {
            DoubleAnimation animationX = new(0, TimeSpan.FromMilliseconds(150));
            DoubleAnimation animationY = new(0, TimeSpan.FromMilliseconds(150));
            LetterBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animationX);
            LetterBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animationY);

            DoubleAnimation opacityAnimation = new(0.5, 0, TimeSpan.FromMilliseconds(150));
            LetterBackground.BeginAnimation(OpacityProperty, opacityAnimation);

            Panel.SetZIndex(MainPanel, 2);
            Panel.SetZIndex(LetterPanel, 1);
        }

        private void openWelcome(Letter letter)
        {
            LetterTitle.Text = letter.Title;
            LetterText.Text = letter.Text;

            DoubleAnimation animationX = new(0, 1, TimeSpan.FromMilliseconds(150));
            DoubleAnimation animationY = new(0, 1, TimeSpan.FromMilliseconds(150));
            LetterBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animationX);
            LetterBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animationY);

            DoubleAnimation opacityAnimation = new(0, 0.5, TimeSpan.FromMilliseconds(150));
            LetterBackground.BeginAnimation(OpacityProperty, opacityAnimation);

            Panel.SetZIndex(MainPanel, 1);
            Panel.SetZIndex(LetterPanel, 2);

            letter.Opened = true;

            var settings = _dbContext.Settings.FirstOrDefault();
            settings.IsFirstLaunch = false;

            _dbContext.SaveChanges();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            setupDb();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("reset"))
            {
                var letters = _dbContext.Letters;
                foreach(var letter in letters)
                    letter.Opened = false;

                var settings = _dbContext.Settings.FirstOrDefault();
                settings.IsFirstLaunch = true;
                settings.LastDocOpenDate = DateTime.MinValue;

                _dbContext.SaveChanges();
                Application.Current.Shutdown();
            }

            if (_settings.IsFirstLaunch)
            {
                var welcome = _dbContext.Letters.Where(letter => letter.Title == "Welcome letter").FirstOrDefault();
                openWelcome(welcome);
            }
            renderData();
            startTimer();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _dbContext?.Dispose();
        }

        private void LetterBackground_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border b = (Border)sender;
            if (b.Name == LetterBackground.Name)
            {
                closeLetter();
            }
        }

        private string getTimespanText(TimeSpan input)
        {
            string output = "";
            output += input.Days > 0 ? input.Days+"d " : "";
            output += input.Hours > 0 ? input.Hours+"h " : "";
            output += input.Minutes > 0 ? input.Minutes+"m" : "";

            output = input.Minutes == 0 ? input.Seconds + " seconds" : output;

            return output;
        }

        public bool areOpenedLettersVisible { get; set; }
        public bool areClosedLettersVisible
        {
            get
            {
                return !areOpenedLettersVisible;
            }
            set
            {
                areOpenedLettersVisible = !value;
            }
        }
        private void Toggle_Views_Click(object sender, RoutedEventArgs e)
        {
            areOpenedLettersVisible= !areOpenedLettersVisible;
            Button button= (Button)sender;

            if (areOpenedLettersVisible)
            {
                button.Content = "Opened Letters";
            }
            else
            {
                button.Content = "Unopened Letters";
            }
            renderData();
        }
    }
}
