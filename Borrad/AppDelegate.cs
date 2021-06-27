using AppKit;
using Foundation;

using Xamarin.Forms.Platform.MacOS;

namespace Borrad
{
  [Register("AppDelegate")]
  public class AppDelegate : NSApplicationDelegate
  {
    public AppDelegate()
    {
    }

    public override void DidFinishLaunching(NSNotification notification)
    {
      // Insert code here to initialize your application
    }

    public override void WillTerminate(NSNotification notification)
    {
      // Insert code here to tear down your application
    }
  }
}
