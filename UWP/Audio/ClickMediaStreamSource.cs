using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;

namespace Jammit.Audio
{
  public class ClickMediaStreamSource
  {
    Model.JcfMedia _media;
    AudioStreamDescriptor _descriptor;

    public ClickMediaStreamSource(Model.JcfMedia media)
    {
      _media = media;

      _descriptor = new AudioStreamDescriptor(AudioEncodingProperties.CreatePcm(44100, 2, 16));
      MediaStreamSource = new MediaStreamSource(_descriptor);
      MediaStreamSource.Starting += OnStarting;
      MediaStreamSource.SampleRequested += OnSampleRequested;
      MediaStreamSource.SwitchStreamsRequested += OnSwitchStreamsRequested;
    }

    public MediaStreamSource MediaStreamSource { get; }

    private void OnStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
    {
      throw new NotImplementedException();
    }

    private void OnSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
    {
      throw new NotImplementedException();
    }

    private void OnSwitchStreamsRequested(MediaStreamSource sender, MediaStreamSourceSwitchStreamsRequestedEventArgs args)
    {
      throw new NotImplementedException();
    }

  }
}
