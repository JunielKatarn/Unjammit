using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Jammit.Audio;
using Jammit.Model;
using System.Threading.Tasks;

namespace Jammit.Forms
{
  public partial class App : Application
  {
    public App()
    {
      //TODO: Remove when Shapes leave experimental state.
      Device.SetFlags(new string[] { "Shapes_Experimental", "RadioButton_Experimental" });

      InitializeComponent();

      App.Client = new Client.BasicHttpClient();

      if (string.IsNullOrEmpty(App.DataDirectory))
        App.DataDirectory = Xamarin.Essentials.FileSystem.AppDataDirectory;

      App.Library = new FolderLibrary(DataDirectory, Client);

      LocalizationResourceManager.Instance.SetCulture(System.Globalization.CultureInfo.GetCultureInfo(Settings.Culture));

      MainPage = new Jammit.Forms.Views.MainPage();
    }

    #region Properties

    public static Jammit.Client.IClient Client { get; /*private*/ set; }

    public static ILibrary Library { get; /*private*/ set; }

    public static string DataDirectory { get; /*private*/ set; }

    public static Func<JcfMedia, Task<IJcfPlayer>> PlayerFactory { get; /*private*/ set; }

    public static IJcfLoader MediaLoader { get; /*private*/ set; }

    public static string[] AllowedFileTypes { get; set; } = new string[] { ".zip" };

    #endregion

    protected override void OnStart()
    {
      // Handle when your app starts
    }

    protected override void OnSleep()
    {
      // Handle when your app sleeps
    }

    protected override void OnResume()
    {
      // Handle when your app resumes
    }
  }
}
