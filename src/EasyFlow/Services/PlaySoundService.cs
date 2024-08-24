﻿using EasyFlow.Data;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;
using System.IO;
using System.Linq;

namespace EasyFlow.Services;

public enum SoundType
{
    Break,
    Work,
}

public interface IPlaySoundService
{
    public void Play(SoundType type);
}
public sealed class PlaySoundService : IPlaySoundService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    public PlaySoundService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Play(SoundType type)
    {
        using var context = _contextFactory.CreateDbContext();
        var settings = context.GeneralSettings.FirstOrDefault();

        if (settings is null)
        {
            return;
        }

        var breakSounds = settings.IsBreakSoundEnabled;

        if (type == SoundType.Break && !breakSounds)
        {
            return;
        }

        var workSounds = settings.IsWorkSoundEnabled;

        if (type == SoundType.Work && !workSounds)
        {
            return;
        }

        var volume = settings.SoundVolume;
        if (volume == 0)
        {
            return;
        }

        var assets = "Assets";
        var fileName = GetFileName(type);
        var filePath = Path.Combine(App.BasePath, assets, fileName);

        WaveOutEvent outputDevice = new();
        AudioFileReader audioFile = new(filePath);

        outputDevice.Init(audioFile);
        outputDevice.Volume = volume / 100.0f; 
        outputDevice.Play();
    }

    private static string GetFileName(SoundType type) => type switch
    {
        SoundType.Break => "started_break.mp3",
        SoundType.Work => "started_work.mp3",
        _ => throw new System.NotImplementedException(),
    };
}
