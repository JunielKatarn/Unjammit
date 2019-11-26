using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Jammit.Forms.Views.HorizontalFillImage), typeof(Jammit.Forms.Renderers.UWPHorizontalFillImageRenderer))]
namespace Jammit.Forms.Renderers
{
  public class UWPHorizontalFillImageRenderer : ImageRenderer
  {

    #region ViewRenderer overrides

    protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
    {
      base.OnElementChanged(e);

      if (Control != null)
      {
        //TODO: Make horizontal fill to size and truncate vertically from the bottom up.
      }
    }

    #endregion ViewRenderer overrides
  }
}
