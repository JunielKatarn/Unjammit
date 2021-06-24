using AppKit;

namespace Jammit.macOS
{
  static class MainClass
  {
    static void Main(string[] args)
    {
      NSApplication.Init();
      //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/platform/other/mac
      NSApplication.SharedApplication.Delegate = new AppDelegate();
      NSApplication.Main(args);
    }
  }
}
