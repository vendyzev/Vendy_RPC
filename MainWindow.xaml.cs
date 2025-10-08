using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DiscordRPC;
using DiscordRPC.Helper;

namespace CustomRPC.WPF
{
    public enum ActivityType
    {
        Playing,
        Listening,
        Watching,
        Competing
    }

    public partial class MainWindow : Window
    {
        private DiscordRpcClient? client;
        private bool isConnected = false;
        private AppSettings? settings;
        private bool isClosingFromButton = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            WinApi.UseImmersiveDarkMode(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            
            LoadSettings();
            
            ShowSection("Connection");
            
            SetupAutoSave();
            
            if (settings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void SetupAutoSave()
        {
            ApplicationIdTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            DetailsTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            StateTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            LargeImageUrlTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            LargeImageTextTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            SmallImageUrlTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            SmallImageTextTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            Button1TextTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            Button1UrlTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            Button2TextTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            Button2UrlTextBox.TextChanged += (s, e) => SaveCurrentSettings();
            
            BackgroundOnRadio.Checked += (s, e) => SaveCurrentSettings();
            BackgroundOffRadio.Checked += (s, e) => SaveCurrentSettings();
            StartMinimizedCheckBox.Checked += (s, e) => SaveCurrentSettings();
            StartMinimizedCheckBox.Unchecked += (s, e) => SaveCurrentSettings();
        }


        private void ShowSection(string sectionName)
        {
            ConnectionSection.Visibility = Visibility.Collapsed;
            PresenceSection.Visibility = Visibility.Collapsed;
            ImagesSection.Visibility = Visibility.Collapsed;
            ButtonsSection.Visibility = Visibility.Collapsed;
            SettingsSection.Visibility = Visibility.Collapsed;
            
            switch (sectionName)
            {
                case "Connection":
                    ConnectionSection.Visibility = Visibility.Visible;
                    ContentTitleTextBlock.Text = "연결";
                    break;
                case "Presence":
                    PresenceSection.Visibility = Visibility.Visible;
                    ContentTitleTextBlock.Text = "활동 상태 세부사항";
                    break;
                case "Images":
                    ImagesSection.Visibility = Visibility.Visible;
                    ContentTitleTextBlock.Text = "이미지 설정";
                    break;
                case "Buttons":
                    ButtonsSection.Visibility = Visibility.Visible;
                    ContentTitleTextBlock.Text = "버튼 설정";
                    break;
                case "Settings":
                    SettingsSection.Visibility = Visibility.Visible;
                    ContentTitleTextBlock.Text = "설정";
                    break;
            }
        }

        private void LoadSettings()
        {
            try
            {
                settings = Utils.LoadSettings();
                
                ApplicationIdTextBox.Text = settings.ApplicationId;
                DetailsTextBox.Text = settings.Details;
                StateTextBox.Text = settings.State;
                LargeImageUrlTextBox.Text = settings.LargeImageUrl;
                LargeImageTextTextBox.Text = settings.LargeImageText;
                SmallImageUrlTextBox.Text = settings.SmallImageUrl;
                SmallImageTextTextBox.Text = settings.SmallImageText;
                Button1TextTextBox.Text = settings.Button1Text;
                Button1UrlTextBox.Text = settings.Button1Url;
                Button2TextTextBox.Text = settings.Button2Text;
                Button2UrlTextBox.Text = settings.Button2Url;
                
                BackgroundOnRadio.IsChecked = settings.MinimizeToTray;
                BackgroundOffRadio.IsChecked = !settings.MinimizeToTray;
                StartMinimizedCheckBox.IsChecked = settings.StartMinimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                settings = new AppSettings();
            }
        }

        private void SaveCurrentSettings()
        {
            try
            {
                settings.ApplicationId = ApplicationIdTextBox.Text;
                settings.Details = DetailsTextBox.Text;
                settings.State = StateTextBox.Text;
                settings.LargeImageUrl = LargeImageUrlTextBox.Text;
                settings.LargeImageText = LargeImageTextTextBox.Text;
                settings.SmallImageUrl = SmallImageUrlTextBox.Text;
                settings.SmallImageText = SmallImageTextTextBox.Text;
                settings.Button1Text = Button1TextTextBox.Text;
                settings.Button1Url = Button1UrlTextBox.Text;
                settings.Button2Text = Button2TextTextBox.Text;
                settings.Button2Url = Button2UrlTextBox.Text;
                
                settings.MinimizeToTray = BackgroundOnRadio.IsChecked == true;
                settings.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
                
                Utils.SaveSettings(settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            SaveCurrentSettings();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ApplicationIdTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Application ID.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                MessageBox.Show("Please connect first.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                UpdatePresence();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Connect()
        {
            if (client != null && !client.IsDisposed)
            {
                client.Dispose();
            }

            client = new DiscordRpcClient(ApplicationIdTextBox.Text);
            client.OnReady += ClientOnReady;
            client.OnError += ClientOnError;
            client.OnConnectionFailed += ClientOnConnectionFailed;

            if (client.Initialize())
            {
                isConnected = true;
                UpdateUI();
                StatusTextBlock.Text = "연결 중...";
            }
            else
            {
                MessageBox.Show("Failed to initialize Discord RPC client.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect()
        {
            if (client != null)
            {
                client.Dispose();
            }

            isConnected = false;
            UpdateUI();
            StatusTextBlock.Text = "연결 끊김";
            UsernameTextBlock.Text = "";
        }

        private void UpdatePresence()
        {
            if (client == null || client.IsDisposed)
                return;

            var presence = new RichPresence()
            {
                Details = DetailsTextBox.Text.Length >= 2 ? DetailsTextBox.Text : null,
                State = StateTextBox.Text.Length >= 2 ? StateTextBox.Text : null,
                Assets = new Assets()
                {
                    LargeImageKey = !string.IsNullOrWhiteSpace(LargeImageUrlTextBox.Text) ? LargeImageUrlTextBox.Text : null,
                    LargeImageText = LargeImageTextTextBox.Text,
                    SmallImageKey = !string.IsNullOrWhiteSpace(SmallImageUrlTextBox.Text) ? SmallImageUrlTextBox.Text : null,
                    SmallImageText = SmallImageTextTextBox.Text
                }
            };

            var buttons = new System.Collections.Generic.List<DiscordRPC.Button>();
            
            if (!string.IsNullOrWhiteSpace(Button1TextTextBox.Text) && 
                !string.IsNullOrWhiteSpace(Button1UrlTextBox.Text) &&
                Button1TextTextBox.Text.Length >= 2)
            {
                buttons.Add(new DiscordRPC.Button()
                {
                    Label = Button1TextTextBox.Text,
                    Url = Button1UrlTextBox.Text
                });
            }
            
            if (!string.IsNullOrWhiteSpace(Button2TextTextBox.Text) && 
                !string.IsNullOrWhiteSpace(Button2UrlTextBox.Text) &&
                Button2TextTextBox.Text.Length >= 2)
            {
                buttons.Add(new DiscordRPC.Button()
                {
                    Label = Button2TextTextBox.Text,
                    Url = Button2UrlTextBox.Text
                });
            }

            if (buttons.Count > 0)
            {
                presence.Buttons = buttons.ToArray();
            }

            var activityType = ActivityType.Playing;
            
            try
            {
                var typeProperty = presence.GetType().GetProperty("Type");
                if (typeProperty != null)
                {
                    typeProperty.SetValue(presence, activityType);
                }
            }
            catch
            {
         
            }
            
            client.SetPresence(presence);
            StatusTextBlock.Text = "활동 상태 업데이트됨";
        }


        private void UpdateUI()
        {
            ConnectButton.IsEnabled = !isConnected;
            DisconnectButton.IsEnabled = isConnected;
            UpdateButton.IsEnabled = isConnected;
            ApplicationIdTextBox.IsEnabled = !isConnected;
        }

        private void ClientOnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            Dispatcher.Invoke(() =>
            {
                UsernameTextBlock.Text = args.User.Username;
                StatusTextBlock.Text = "연결됨";
            });
        }

        private void ClientOnError(object sender, DiscordRPC.Message.ErrorMessage args)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Discord RPC Error: {args.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Disconnect();
            });
        }

        private void ClientOnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Failed to connect to Discord. Make sure Discord is running.", "Connection Failed", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                Disconnect();
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (settings.MinimizeToTray && !isClosingFromButton)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
            else
            {
                SaveCurrentSettings();
                Disconnect();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            isClosingFromButton = true;
            
            if (settings.MinimizeToTray)
            {
                Hide();
            }
            else
            {
                Close();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ConnectionNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Connection");
        }

        private void PresenceNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Presence");
        }

        private void ImagesNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Images");
        }



        private void ButtonsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Buttons");
        }

        private void SettingsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection("Settings");
        }
    }
}