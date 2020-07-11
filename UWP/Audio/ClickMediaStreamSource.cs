using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace Jammit.Audio
{
  public class ClickMediaStreamSource
  {
    readonly Model.JcfMedia _media;
    readonly AudioStreamDescriptor _descriptor;
    readonly short[] _click;

    public ClickMediaStreamSource(Model.JcfMedia media)
    {
      _media = media;

      _descriptor = new AudioStreamDescriptor(AudioEncodingProperties.CreatePcm(44100, 2, 16));
      MediaStreamSource = new MediaStreamSource(_descriptor);
      MediaStreamSource.Duration = media.Length;
      MediaStreamSource.Starting += OnStarting;
      MediaStreamSource.SampleRequested += OnSampleRequested;
      MediaStreamSource.SwitchStreamsRequested += OnSwitchStreamsRequested;

      _click = new short[Forms.Resources.Assets.stick.Length / 2];
      System.Buffer.BlockCopy(Forms.Resources.Assets.stick, 0, _click, 0, Forms.Resources.Assets.stick.Length);
    }

    public MediaStreamSource MediaStreamSource { get; }

    private MediaStreamSample NextSample()
    {
      IBuffer buffer = null;
      var result = MediaStreamSample.CreateFromBuffer(buffer, TimeSpan.Zero);

      return result;
    }

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
