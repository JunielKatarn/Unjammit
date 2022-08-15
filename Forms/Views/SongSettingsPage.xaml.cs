using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Jammit.Forms.Views
{
  public partial class SongSettingsPage : ContentPage
  {
    public SongSettingsPage(Model.SongInfo song)
    {
      InitializeComponent();
      Song = song;
      CountinEntry.Text = Settings.Get(Settings.CountInKey(Song), 0).ToString();
    }

    private Model.SongInfo Song { get; }

    protected override void OnAppearing()
    {
      BackButton.Text = "⬅️ " + BackButton.Text;

      base.OnAppearing();
    }

    async void BackButton_Clicked(object sender, EventArgs e)
    {
      await Navigation.PopModalAsync(animated: true);
    }

    //TODO: Read https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/validation
    //TODO: Read https://xamgirl.com/validation-snippets-in-xamarin-forms/
    private void SongSettingsPage_Disappearing(object sender, EventArgs e)
    {
      Settings.Set(Settings.CountInKey(Song), uint.Parse(CountinEntry.Text));
    }
  }
}
