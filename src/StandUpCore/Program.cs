using System;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace StandUpCore;

internal static class Program
{
  private static NotifyIcon notifyIcon = new NotifyIcon();
  private static ContextMenuStrip cms = new ContextMenuStrip();
  private static System.Threading.Timer? timer = null;
  private static ApplicationContext? context;
  const int TIMEOUTINMINUTES = 25;
#if DEBUG
  static readonly int TimeOutInMilliseconds = 5000;
#else
  static readonly int TimeOutInMilliseconds = (int) TimeSpan.FromMinutes(TIMEOUTINMINUTES).TotalMilliseconds;
#endif

  /// <summary>
  ///  The main entry point for the application.
  /// </summary>
  [STAThread]
  static void Main()
  {
    // To customize application configuration such as set high DPI settings or default font,
    // see https://aka.ms/applicationconfiguration.
    ApplicationConfiguration.Initialize();

    notifyIcon.Icon = new Icon("Assets/favicon.ico");
    notifyIcon.Text = "Notify";

    cms.Items.Add(new ToolStripMenuItem("Restart Timer", null, new EventHandler(OnRestartTimer)));
    cms.Items.Add(new ToolStripSeparator());
    cms.Items.Add(new ToolStripMenuItem("Quit", null, new EventHandler(OnQuit), "Quit"));

    notifyIcon.ContextMenuStrip = cms;
    notifyIcon.Visible = true;
    
    timer = new System.Threading.Timer(OnTimeOut, null, TimeOutInMilliseconds, TimeOutInMilliseconds);

    // Create an ApplicationContext and run a message loop
    // on the context.
    context = new ApplicationContext();
    Application.Run(context);

    // Hide notify icon on quit
    notifyIcon.Visible = false;

  }

  static void OnRestartTimer(object? sender, EventArgs e)
  {
    timer?.Change(TimeOutInMilliseconds, TimeOutInMilliseconds);
  }

  static void OnTimeOut(object? state)
  {
    new ToastContentBuilder()
      .AddText("It's Time!")
      .AddText("Time to get up and stretch!")
      .Show();
  }

  static void OnQuit(object? sender, System.EventArgs e)
  {
    // End application though ApplicationContext
    timer?.Dispose();
    context!.ExitThread();
  }
}