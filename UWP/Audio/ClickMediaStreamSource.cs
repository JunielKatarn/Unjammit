using System;
using System.Collections.Concurrent;

using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

using Buffer = Windows.Storage.Streams.Buffer;
using MediaPlaybackSession = Windows.Media.Playback.MediaPlaybackSession;

namespace Jammit.Audio
{
  public class ClickMediaStreamSource
  {
    readonly Model.JcfMedia _media;
    readonly AudioStreamDescriptor _descriptor;
    readonly short[] _click;
    ConcurrentQueue<IBuffer> _buffersQueue;
    MediaPlaybackSession _session;
    TimeSpan _actualPosition = TimeSpan.Zero;

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
      MediaStreamSource.Closed += OnClosed;

      _buffersQueue = new ConcurrentQueue<IBuffer>();

      _click = new short[Forms.Resources.Assets.Stick.Length / 2];
      System.Buffer.BlockCopy(Forms.Resources.Assets.Stick, 0, _click, 0, Forms.Resources.Assets.Stick.Length);
    }

    public MediaStreamSource MediaStreamSource { get; }

    public MediaPlaybackSession PlaybackSession
    {
      get => _session;

      set
      {
        if (_session != null)
        {
          _session.PositionChanged -= OnSessionPositionChanged;
        }

        _session = value;

        if (value != null)
        {
          _session.PositionChanged += OnSessionPositionChanged;
        }
      }
    }

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

    private IBuffer GetBuffer()
    {
      IBuffer buffer;
      bool dequeued = _buffersQueue.TryDequeue(out buffer);
      if (!dequeued)
      {
        // FFmpegInterop::UncompressedSampleProvider::CreateNextSampleBuffer~69 suggests 256, but disrupts playback.
        //buffer = new Buffer(4096);// works-ish
        //buffer = new Buffer(5292);// why?? = 176400 / 33.3333...
        buffer = new Buffer(176400);// https://www.alsa-project.org/wiki/FramesPeriods#:~:text=e.g.,PCM%20stream%20is%2012%20bytes.
                                    // channels * sampleSize * analogRate * = 2ch * 2B/smpl * 44100 smpl/s
                                    // TODO: Infer from audio format
        buffer.Length = buffer.Capacity;
      }

      return buffer;
    }

    private void OnStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
    {
      //args.Request.SetActualStartPosition(args.Request.StartPosition ?? TimeSpan.Zero);
      args.Request.SetActualStartPosition(_actualPosition);
    }

    private void OnSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
    {
      MediaStreamSample sample = null;
      if (_descriptor == args.Request.StreamDescriptor)
      {
        //176,400 bytes per second => 44.1kHz * 16bits * 2channels = 44100 * 32 b/s = 1411200b/s / 8b/B = 176400 B/s
        var buffer = GetBuffer();

        if (buffer.Length > 0)
        {
          // NextSample()
          // From FFmpegInterpoX: pts ~= 484033; dur = 64; nextPts = pts + dur
          //                      aBuffSz = 256; rsampledDataSz = 64; sz = min(aBuffSz, rsmplDtSz * 2ch * 2BxSmpl) = 256
          //                      actDur = avFr.nbSmpl(=64) * avStrm.tBase.den(=44100) / (_osr(=44100) * avStrm.tBase.num(=1) ) = 64
          //                      pts @20s ~= 915649; @40 ~= 1811969 => 45299.225 ~> 44100 pts/s
          //                      _tbf = av_q2d(avStr.tBase) * 10000000 = 226.75736961451247 =>
          //                        (tBase.num / tBase.den => 1 / 441000) * 10^7 = 10000000 / 44100 = 226.75736961451247
          //                      Pos = _tbf * pts - _sOff(=0)
          //                      Dur = _tbf * dur(=64) // A time period expressed in 100-nanosecond units (ticks) => 1s * 10^-7 => 1 / 1000000
          sample = MediaStreamSample.CreateFromBuffer(buffer, _actualPosition);
          sample.Processed += OnSampleProcessed;

          // 10000000 / 44100 * 64 => 10^-7s
          sample.Duration = TimeSpan.FromTicks(10000000 / 44100 * 64);// TODO: Infer from audio format

          _actualPosition += sample.Duration;
        }
        else
        {
          _actualPosition = TimeSpan.Zero;
        }
      }

      args.Request.Sample = sample;
    }

    private void OnSwitchStreamsRequested(MediaStreamSource sender, MediaStreamSourceSwitchStreamsRequestedEventArgs args)
    {
      //throw new NotImplementedException();
    }

    private void OnClosed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
    {
      _actualPosition = TimeSpan.Zero;
    }

    private void OnSampleProcessed(MediaStreamSample sender, object args)
    {
      _buffersQueue.Enqueue(sender.Buffer);
      sender.Processed -= OnSampleProcessed;
    }

    TimeSpan _x = TimeSpan.Zero;
    private void OnSessionPositionChanged(MediaPlaybackSession sender, object args)
    {
      // Not triggered unless _actualPosition is manually updated on RequestSample. Else, huge mem. leak
      // TODO: Figure out how to reliably get position from session/player/controller.
      if (sender.Position > TimeSpan.Zero)
      {
        //_actualPosition = sender.Position;
        _x = sender.Position;
      }
    }
  }
}
