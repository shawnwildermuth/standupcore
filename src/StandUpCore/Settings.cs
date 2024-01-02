namespace StandUpCore;

public class Settings
{
  public int CurrentInterval { get; set; }
  public int TimerGranularity { get; set; }
  public string Name { get; set; } = "";

  public int TimerInMilliseconds() => TimerGranularity * 1000;
}

