using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using static System.Console;

namespace StandUpCore;

internal static class Program
{
  static NotifyIcon _notifyIcon = new NotifyIcon();
  static ContextMenuStrip _cms = new ContextMenuStrip();
  static System.Threading.Timer? _timer = null;
  static ApplicationContext? _context;
  static int _counter = 0;
  static Settings? _settings;
  static JsonSerializerOptions _serializerOptions = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  /// <summary>
  ///  The main entry point for the application.
  /// </summary>
  [STAThread]
  static async Task Main()
  {

    await ReadSettings();

    // Used for debugging only
#if DEBUG
    ShowNotification();
    AllocConsole();
#endif

    ApplicationConfiguration.Initialize();

    _notifyIcon.Icon = new Icon("Assets/favicon.ico");
    _notifyIcon.Text = "Notify";

    foreach (var value in new[] { 60, 45, 30, 25, 20, 15, 10, 5 })
    {
      var item = new ToolStripMenuItem()
      {
        Text = $"Remind me every {value} mins",
        Tag = value
      };
      item.Click += OnIntervalChangedClicked;
      _cms.Items.Add(item);
    }
    _cms.Items.Add(new ToolStripSeparator());
    _cms.Items.Add(new ToolStripMenuItem("Restart Timer", null, new EventHandler(OnRestartTimer)));
    _cms.Items.Add(new ToolStripSeparator());
    _cms.Items.Add(new ToolStripMenuItem("Quit", null, new EventHandler(OnQuit), "Quit"));

    _notifyIcon.ContextMenuStrip = _cms;
    _notifyIcon.Visible = true;

    _timer = new System.Threading.Timer(OnTimerTick, null, TimeInMilliseconds(_settings!.TimerGranularity),
      TimeInMilliseconds(_settings!.TimerGranularity));

    UpdateIconText();
    CheckRightInterval();

    WriteLine("Starting Timer");

    // Create an ApplicationContext and run a message loop
    // on the context.
    _context = new ApplicationContext();
    Application.Run(_context);

    // Hide notify icon on quit
    _notifyIcon.Visible = false;
  }

  static async Task ReadSettings()
  {
    var json = await File.ReadAllTextAsync("settings.json");
    _settings = JsonSerializer.Deserialize<Settings>(json, _serializerOptions);
    if (_settings?.CurrentInterval == 0 || _settings?.CurrentInterval == 0)
    {
      throw new InvalidOperationException("Could not read the settings.json file");
    }
  }

  static void SaveSettings()
  {
    var json = JsonSerializer.Serialize(_settings, _serializerOptions);
    File.WriteAllText("settings.json", json);
  }


  static int TimeInMilliseconds(int minutes) =>
    (int)TimeSpan.FromSeconds(minutes).TotalMilliseconds;

  static void CheckRightInterval()
  {
    foreach (var menu in _cms.Items)
    {
      if (menu is ToolStripMenuItem menuItem)
      {
        menuItem.Checked = (menuItem.Tag is int value &&
          value == _settings!.CurrentInterval);
      }
    }
  }

  static void RestartTimer()
  {
    _timer?.Change(_settings!.TimerInMilliseconds(),
      _settings!.TimerInMilliseconds());

  }

  static void UpdateIconText()
  {
    var timeLeft = TimeSpan.FromMinutes(_settings!.CurrentInterval) - TimeSpan.FromSeconds(_counter);
    _notifyIcon.Text = $"{timeLeft.Minutes}:{timeLeft.Seconds} left";
  }

  /*************** Events *******************/

  static void OnIntervalChangedClicked(object? sender, EventArgs e)
  {
    if (sender is ToolStripMenuItem item)
    {
      var interval = (int)item.Tag!;
      _settings!.CurrentInterval = interval;
      SaveSettings();
      CheckRightInterval();
      _counter = 0;
      RestartTimer();
    }
  }

  static void OnRestartTimer(object? sender, EventArgs e)
  {
    _counter = 0;
    RestartTimer();
  }

  static void OnTimerTick(object? state)
  {
    WriteLine("Timer firing");

    // Increment
    _counter += _settings!.TimerGranularity;  // how many seconds that we get a tick

    UpdateIconText();

    if (_counter >= _settings!.CurrentInterval * 60) // Minutes
    {
      _counter = 0;
      ShowNotification();
    }
  }

  static void ShowNotification()
  {
    WriteLine("Notification Shown");
    new ToastContentBuilder()
          .AddHeader("standup", "It's Time!", "")
          .AddText($"{_settings!.Name}, it's time to move!")
          .Show(opt => opt.ExpirationTime = DateTime.Now.AddMinutes(_settings.CurrentInterval));
  }

  static void OnQuit(object? sender, System.EventArgs e)
  {
    // End application though ApplicationContext
    _timer?.Dispose();
    _context!.ExitThread();
  }

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  static extern bool AllocConsole();

}

