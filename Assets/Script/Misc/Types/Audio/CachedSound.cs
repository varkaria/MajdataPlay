﻿using System.Collections.Generic;
using NAudio.Wave;
using System.Linq;
using NAudio.Wave.SampleProviders;
using System.Runtime.InteropServices;
using System;

namespace MajdataPlay.Types
{
    public unsafe class CachedSound: IDisposable
    {
        bool isDestroyed = false;
        public int Length { get; private set; }
        public Span<float> AudioData => new Span<float>(audioData, Length);
        float* audioData;
        public WaveFormat WaveFormat { get; private set; }
        //this might take time
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                
                var resampler = new WdlResamplingSampleProvider(audioFileReader, GameManager.Instance.Setting.Audio.Samplerate);
                WaveFormat = resampler.WaveFormat;
                Length = 0;
                var readBuffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
                var buffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
                int totalRead = 0;
                int samplesRead;
                int pos = 0;
                while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    var startIndex = totalRead;
                    totalRead += samplesRead;
                    if(totalRead > buffer.Length)
                    {
                        var _buffer = new float[totalRead];
                        buffer.CopyTo(_buffer.AsSpan());
                        buffer = _buffer;
                    }
                    for (int i = startIndex; i < totalRead; i++)
                        buffer[i] = readBuffer[i - startIndex];
                }
                Length = buffer.Length;
                audioData = (float*)Marshal.AllocHGlobal(sizeof(float) * Length);
                buffer.CopyTo(AudioData);
            }
        }
        ~CachedSound() => Dispose();
        public void Dispose()
        {
            if (isDestroyed)
                return;
            isDestroyed = true;
            Marshal.FreeHGlobal((IntPtr)audioData);
        }
    }
}