using Jammit.Model;
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
    MyBuffer _myBuffer;
    Windows.Media.MediaTimelineController _controller;

    ConcurrentQueue<IBuffer> _buffersQueue;
    TimeSpan _currentPosition = TimeSpan.Zero;

    public ClickMediaStreamSource(Model.JcfMedia media, Windows.Media.MediaTimelineController controller)
    {
      _media = media;
      _controller = controller;

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
      _myBuffer = new MyBuffer(_media);
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

    int lastSecond = 0;
    private IBuffer GetBuffer()
    {
#if false
      IBuffer buffer;
      bool dequeued = _buffersQueue.TryDequeue(out buffer);
      if (!dequeued)
      {
        // FFmpegInterop::UncompressedSampleProvider::CreateNextSampleBuffer~69 suggests 256, but disrupts playback.
        //buffer = new Buffer(4096);// works-ish
        //buffer = new Buffer(5292);// why?? = 176400 / 33.3333...
        //buffer = new Buffer(176400);// https://www.alsa-project.org/wiki/FramesPeriods#:~:text=e.g.,PCM%20stream%20is%2012%20bytes.
        //                            // channels * sampleSize * analogRate * = 2ch * 2B/smpl * 44100 smpl/s
        //                            // TODO: Infer from audio format

        // Copy at current song position.
        byte[] bytes = new byte[176400];

        //_dialTone[2 * i] = (byte)(_click[i % beatIntervalInSamples] & 0XFF);
        //_dialTone[2 * i + 1] = (byte)((_click[i % beatIntervalInSamples] >> 8) & 0xFF);

        //if (lastSecond != _controller.Position.Seconds)
        //{
        //  lastSecond = _controller.Position.Seconds;
        //  //Array.Copy(_click, bytes, _click.Length);
        //  for (int i = 0; i < _click.Length; i++)
        //  {
        //    bytes[i + 0] = (byte)(_click[i] & 0xFF);
        //    bytes[i + 1] = (byte)((_click[i] >> 8) & 0xFF); // Clear mask needed?
        //  }
        //}

        buffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(bytes);

        buffer.Length = buffer.Capacity;
      }

      return buffer;
#else
      return _myBuffer.GetBuffer();
#endif
    }

    private void OnStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
    {
      //args.Request.SetActualStartPosition(args.Request.StartPosition ?? TimeSpan.Zero);
      args.Request.SetActualStartPosition(_currentPosition);
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
          sample = MediaStreamSample.CreateFromBuffer(buffer, _currentPosition);
          //sample = MediaStreamSample.CreateFromBuffer(buffer, _controller.Position);
          sample.Processed += OnSampleProcessed;

          // 10000000 / 44100 * 64 => 10^-7s
          // Why 32?
          //sample.Duration = TimeSpan.FromTicks(10000000 / 44100 * 32);// TODO: Infer from audio format
          sample.Duration = TimeSpan.FromTicks(10000000 / 44100 * 32 * 2);// TODO: Infer from audio format

          System.Diagnostics.Debug.WriteLine($"SP: {_controller.Position}\nCP: {_currentPosition}");

          _currentPosition += sample.Duration;
        }
        else
        {
          _currentPosition = TimeSpan.Zero;
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
      _currentPosition = TimeSpan.Zero;
    }

    private void OnSampleProcessed(MediaStreamSample sender, object args)
    {
      _buffersQueue.Enqueue(sender.Buffer);
      sender.Processed -= OnSampleProcessed;
    }
  }

  public class MyBuffer
  {
    byte[] _dialTone;
    IBuffer _dialToneBuffer;
    JcfMedia _media;

    public MyBuffer(JcfMedia media)
    {
      //_dialTone = new byte[2 * 44100 * 60]; // 1 minute!
      _dialTone = new byte[2 * 44100 * (int)media.Length.TotalSeconds]; // More like 2 * freq * seconds!
      FillDialTone();
      SetupBuffer();
    }

    public IBuffer GetBuffer()
    {
      return _dialToneBuffer;
    }

    private void FillDialTone()
    {
      // Create a pilot tone: sin wave at 440 Hz

      // move where best fits
      short[] _click;

      // Expose this as a param, and/or with a function to compute it.
      int bpm = 60;

      _click = new short[Forms.Resources.Assets.Stick.Length / 2];
      System.Buffer.BlockCopy(Forms.Resources.Assets.Stick, 0, _click, 0, Forms.Resources.Assets.Stick.Length);

      int samplingFrequency = 44100;

      // TODO: Use timespans for precise values, or floats may suffice
      int beatIntervalInSamples = (int) (samplingFrequency / ((float)bpm / 60 ));


#if true
      for (int i = 0; i < _dialTone.Length / 2; i++)
      {

        // Compute and append sin wave on both channels.
        // This is where things may be wrong, not sure what dialTone is: mono signal duplicated or dual channel signal
        //short value = (short)(16000 * Math.Sin(2 * PI * i / samplesPerPeriod));
        //_dialTone[2 * i] = (byte)(value & 0XFF);
        //_dialTone[2 * i + 1] = (byte)((value >> 8) & 0xFF);

        // Add the ticking signal
        if (i % beatIntervalInSamples < Forms.Resources.Assets.Stick.Length / 2 - 1)
        {
          _dialTone[2 * i] = (byte)(_click[i % beatIntervalInSamples] & 0xFF);
          _dialTone[2 * i + 1] = (byte)((_click[i % beatIntervalInSamples] >> 8) & 0xFF);
        }
      }
#else
      foreach(var beat in _media.Beats)
      {
        if (beat.IsGhostBeat)
          continue;


      }
#endif
    }

    private void SetupBuffer()
    {
      _dialToneBuffer = new Buffer((uint) _dialTone.Length * 2);
      
      using (System.IO.Stream stream = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.AsStream(_dialToneBuffer))
      {
        stream.Write(_dialTone, 0, _dialTone.Length);
      }
      _dialToneBuffer.Length = _dialToneBuffer.Capacity / 2;
    }
  }
}
