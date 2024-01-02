using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using static System.Console;

namespace StandUpCore;

internal static class Program
{
  static NotifyIcon notifyIcon = new NotifyIcon();
  static ContextMenuStrip cms = new ContextMenuStrip();
  static System.Threading.Timer? timer = null;
  static ApplicationContext? context;
  static int counter = 0;
  static Settings? settings;
  static JsonSerializerOptions serializerOptions = new()
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
    // Used for debugging only
#if DEBUG
    AllocConsole();
#endif

    ApplicationConfiguration.Initialize();

    await ReadSettings();

    notifyIcon.Icon = new Icon("Assets/favicon.ico");
    notifyIcon.Text = "Notify";

    foreach (var value in new[] { 60, 45, 30, 25, 20, 15, 10, 5 })
    {
      var item = new ToolStripMenuItem()
      {
        Text = $"Remind me every {value} mins",
        Tag = value
      };
      item.Click += OnIntervalChangedClicked;
      cms.Items.Add(item);
    }
    cms.Items.Add(new ToolStripSeparator());
    cms.Items.Add(new ToolStripMenuItem("Restart Timer", null, new EventHandler(OnRestartTimer)));
    cms.Items.Add(new ToolStripSeparator());
    cms.Items.Add(new ToolStripMenuItem("Quit", null, new EventHandler(OnQuit), "Quit"));

    notifyIcon.ContextMenuStrip = cms;
    notifyIcon.Visible = true;

    timer = new System.Threading.Timer(OnTimerTick, null, TimeInMilliseconds(settings!.TimerGranularity),
      TimeInMilliseconds(settings!.TimerGranularity));

    UpdateIconText();
    CheckRightInterval();

    WriteLine("Starting Timer");

    // Create an ApplicationContext and run a message loop
    // on the context.
    context = new ApplicationContext();
    Application.Run(context);

    // Hide notify icon on quit
    notifyIcon.Visible = false;
  }

  static async Task ReadSettings()
  {
    var json = await File.ReadAllTextAsync("settings.json");
    settings = JsonSerializer.Deserialize<Settings>(json, serializerOptions);
    if (settings?.CurrentInterval == 0 || settings?.CurrentInterval == 0)
    {
      throw new InvalidOperationException("Could not read the settings.json file");
    }
  }

  static void SaveSettings()
  {
    var json = JsonSerializer.Serialize(settings, serializerOptions);
    File.WriteAllText("settings.json", json);
  }


  static int TimeInMilliseconds(int minutes) =>
    (int)TimeSpan.FromSeconds(minutes).TotalMilliseconds;

  static void CheckRightInterval()
  {
    foreach (var menu in cms.Items)
    {
      if (menu is ToolStripMenuItem menuItem)
      {
        menuItem.Checked = (menuItem.Tag is int value &&
          value == settings!.CurrentInterval);
      }
    }
  }

  static void RestartTimer()
  {
    timer?.Change(settings!.TimerInMilliseconds(),
      settings!.TimerInMilliseconds());

  }

  /*************** Events *******************/

  static void UpdateIconText()
  {
    var timeLeft = TimeSpan.FromMinutes(settings!.CurrentInterval) - TimeSpan.FromSeconds(counter);
    notifyIcon.Text = $"{timeLeft.Minutes}:{timeLeft.Seconds} left";
  }

  static void OnIntervalChangedClicked(object? sender, EventArgs e)
  {
    if (sender is ToolStripMenuItem item)
    {
      var interval = (int)item.Tag!;
      settings!.CurrentInterval = interval;
      SaveSettings();
      CheckRightInterval();
      counter = 0;
      RestartTimer();
    }
  }

  static void OnRestartTimer(object? sender, EventArgs e)
  {
    counter = 0;
    RestartTimer();
  }

  static void OnTimerTick(object? state)
  {
    WriteLine("Timer firing");

    // Increment
    counter += settings!.TimerGranularity;  // how many seconds that we get a tick

    UpdateIconText();

    if (counter >= settings!.CurrentInterval * 60) // Minutes
    {
      WriteLine("Notification Shown");
      new ToastContentBuilder()
            .AddText("It's Time!")
            .AddText("Time to get up and stretch!")
            .Show();
    }
  }

  static void OnQuit(object? sender, System.EventArgs e)
  {
    // End application though ApplicationContext
    timer?.Dispose();
    context!.ExitThread();
  }

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  static extern bool AllocConsole();

}

