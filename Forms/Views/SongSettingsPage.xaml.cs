using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Jammit.Forms.Views
{
  public partial class SongSettingsPage : ContentPage
  {
    public SongSettingsPage()
    {
      InitializeComponent();
    }

    protected override void OnAppearing()
    {
      BackButton.Text = "⬅️ " + BackButton.Text;

      base.OnAppearing();
    }

    async void BackButton_Clicked(object sender, System.EventArgs e)
    {
      await Navigation.PopModalAsync(animated: true);
    }
  }
}

