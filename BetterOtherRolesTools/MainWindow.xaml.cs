using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using BetterOtherRolesTools.Client;

namespace BetterOtherRolesTools;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public readonly BorClient Client;

    public MainWindow()
    {
        InitializeComponent();
        Log("BetterOtherRoles Tools successfully started");
        Client = new BorClient(this);
        Client.Start();
    }

    public void Log(string data)
    {
        var text = $"[{DateTime.Now.ToLongTimeString()}] {data}";
        if (Debug.Text == string.Empty) Debug.Text = text;
        else Debug.Text += $"\n\n{text}";
    }

    private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        await Client.Client.SendAsync(Encoding.UTF8.GetBytes("message"));
        Log("message sent");
        var email = EmailInput.Text;
        var password = PasswordInput.Password;
        if (email == string.Empty || password == string.Empty) return;
        if (new EmailAddressAttribute().IsValid(email))
        {
            //EnableLoginForm(false);
            //Client.Login(email, password);
        }
        /*
        var result = MessageBox.Show("Invalid email or password", "Invalid credentials", MessageBoxButton.OK, MessageBoxImage.Error);
        if (result == MessageBoxResult.OK)
        {
            EnableLoginForm(true);
        }
        */
    }

    private void EnableLoginForm(bool value)
    {
        EmailInput.IsEnabled = value;
        PasswordInput.IsEnabled = value;
        LoginButton.IsEnabled = value;
    }
}