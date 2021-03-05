using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Security.Cryptography;
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
      MediaStreamSource.CanSeek = true;

      MediaStreamSource.Starting += OnStarting;
      MediaStreamSource.SampleRequested += OnSampleRequested;
      MediaStreamSource.SwitchStreamsRequested += OnSwitchStreamsRequested;

      _click = new short[Forms.Resources.Assets.Stick.Length / 2];
      System.Buffer.BlockCopy(Forms.Resources.Assets.Stick, 0, _click, 0, Forms.Resources.Assets.Stick.Length);
    }

    public MediaStreamSource MediaStreamSource { get; }

    private Model.Beat FindBeat(double totalSeconds, int start, int end)
    {
      int mid = (start + end) / 2;
      if (mid == start)
      {
        return _media.Beats[mid];
      }
      else if (_media.Beats[mid].Time < totalSeconds)
      {
        return FindBeat(totalSeconds, mid, end);
      }
      else if (_media.Beats[mid].Time > totalSeconds)
      {
        // If [mid] is the very next major element, finish.
        if (_media.Beats[mid - 1].Time <= totalSeconds)
        {
          return _media.Beats[mid - 1];
        }

        return FindBeat(totalSeconds, start, mid);
      }
      else
      {
        // Unlikely, double equality.
        return _media.Beats[mid];
      }
    }

    byte[] _erasme = new byte[5292];

    private MediaStreamSample NextSample()
    {
      //sample duration = 64
      //sample "size" 5292
      //{00:00:08.7000000} => 1534680
      //{00:02:36.5910000} => 27926527
      //
      // {00:00:35.374716} => (6221096 / 4 + 4751) / 44100
      // "DELTA" = 5292 => _samplePos * 4
      IBuffer buffer = CryptographicBuffer.CreateFromByteArray(_erasme);

      var result = MediaStreamSample.CreateFromBuffer(buffer, TimeSpan.Zero);

      return result;
    }

    private void OnStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
    {
      var x = args.Request.StartPosition ?? TimeSpan.Zero;
    }

    /*
mutexGuard.lock();
if (mss != nullptr)
{
if (currentAudioStream && args->Request->StreamDescriptor == currentAudioStream->StreamDescriptor)
{
  args->Request->Sample = currentAudioStream->GetNextSample();
}
else if (currentVideoStream && args->Request->StreamDescriptor == currentVideoStream->StreamDescriptor)
{
  args->Request->Sample = currentVideoStream->GetNextSample();
}
else
{
  args->Request->Sample = nullptr;
}
}
mutexGuard.unlock();
 */

    private void OnSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
    {
      if (args.Request.StreamDescriptor == _descriptor)
        args.Request.Sample = NextSample();
      else
        throw new Exception("chiaaaaaale");
    }

    private void OnSwitchStreamsRequested(MediaStreamSource sender, MediaStreamSourceSwitchStreamsRequestedEventArgs args)
    {
      throw new NotImplementedException();
    }

  }
}
