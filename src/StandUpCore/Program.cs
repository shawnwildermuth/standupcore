using System;
using System.Windows.Forms;

namespace StandUpCore;

internal static class Program
{
  private static NotifyIcon? notifyIcon;
  private static ContextMenuStrip? cms;
  private static ApplicationContext? context;
  const int TimeOutInMinutes = 25;

  /// <summary>
  ///  The main entry point for the application.
  /// </summary>
  [STAThread]
  static void Main()
  {
    // To customize application configuration such as set high DPI settings or default font,
    // see https://aka.ms/applicationconfiguration.
    ApplicationConfiguration.Initialize();

    notifyIcon = new NotifyIcon();
    notifyIcon.Icon = new Icon("Assets/favicon.ico");
    notifyIcon.Text = "Notify";

    cms = new ContextMenuStrip();

    cms.Items.Add(new ToolStripMenuItem("Reconnect", null, new EventHandler(Reconnect_Click)));
    cms.Items.Add(new ToolStripSeparator());
    cms.Items.Add(new ToolStripMenuItem("Quit", null, new EventHandler(Quit_Click), "Quit"));

    notifyIcon.ContextMenuStrip = cms;
    notifyIcon.Visible = true;

    int timerLength = (int)TimeSpan.FromMinutes(TimeOutInMinutes).TotalMilliseconds;

    var timer = new System.Threading.Timer(OnTimeOut, null, timerLength, timerLength);

    // Create an ApplicationContext and run a message loop
    // on the context.
    context = new ApplicationContext();
    Application.Run(context);

    // Hide notify icon on quit
    notifyIcon.Visible = false;

  }

  private static void OnTimeOut(object? state)
  {
    MessageBox.Show("Time to stand up!");
  }

  static void Reconnect_Click(object? sender, System.EventArgs e)
  {
    MessageBox.Show("Hello World!");
  }

  static void Quit_Click(object? sender, System.EventArgs e)
  {
    // End application though ApplicationContext
    context!.ExitThread();
  }
}