using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace MonoGameLibrary.Audio;

public class AudioController : IDisposable
{
    // Tracks sound effect instances created so they can be paused, unpaused, and/or disposed.
    private readonly List<SoundEffectInstance> _activeSoundEffectInstances = [];

    // Tracks the volume for song playback when muting and unmuting.
    private float _previousSongVolume;

    // Tracks the volume for sound effect playback when muting and unmuting.
    private float _previousSoundEffectVolume;

    /// <summary>
    /// Gets a value that indicates if audio is muted.
    /// </summary>
    public bool IsMuted { get; private set; }

    /// <summary>
    /// Gets or Sets the global volume of songs. Min 0.0f, Max 1.0f.
    /// </summary>
    /// <remarks>
    /// If IsMuted is true, the getter will always return back 0.0f and the
    /// setter will ignore setting the volume.
    /// </remarks>
    public float SongVolume
    {
        get
        {
            if(IsMuted)
                return 0.0f;

            return MediaPlayer.Volume;
        }
        set
        {
            if(IsMuted)
                return;

            MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
        }
    }

    /// <summary>
    /// Gets or Sets the global volume of sound effects. Min 0.0f, Max 1.0f.
    /// </summary>
    /// <remarks>
    /// If IsMuted is true, the getter will always return back 0.0f and the
    /// setter will ignore setting the volume.
    /// </remarks>
    public float SoundEffectVolume
    {
        get
        {
            if(IsMuted)
                return 0.0f;

            return SoundEffect.MasterVolume;
        }
        set
        {
            if(IsMuted)
                return;

            SoundEffect.MasterVolume = Math.Clamp(value, 0.0f, 1.0f);
        }
    }    

    /// <summary>
    /// Gets a value that indicates if this audio controller has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    // Finalizer called when object is collected by the garbage collector.
    ~AudioController() => Dispose(false);

    /// <summary>
    /// Updates this audio controller.
    /// </summary>
    public void Update()
    {
        List<SoundEffectInstance> toRemove = [];
        // possible a regular for loop is better because then we can use RemoveAt(index)
        foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances)
        {
            if (soundEffectInstance.State is SoundState.Stopped)
            {
                soundEffectInstance.Dispose();
                toRemove.Add(soundEffectInstance);
            }
        }
        // only remove after the loop so the enumerator doesn't break
        toRemove.ForEach(s => _activeSoundEffectInstances.Remove(s));
    }

    /// <summary>
    /// Plays the given sound effect.
    /// </summary>
    /// <param name="soundEffect">The sound effect to play.</param>
    /// <returns>The sound effect instance created by this method.</returns>
    public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect)
    {
        return PlaySoundEffect(soundEffect, 1.0f, 0.0f, 0.0f, false);
    }

    /// <summary>
    /// Plays the given sound effect with the specified properties.
    /// </summary>
    /// <param name="soundEffect">The sound effect to play.</param>
    /// <param name="volume">The volume, ranging from 0.0 (silence) to 1.0 (full volume).</param>
    /// <param name="pitch">The pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).</param>
    /// <param name="pan">The panning, ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).</param>
    /// <param name="isLooped">Whether the the sound effect should loop after playback.</param>
    /// <returns>The sound effect instance created by playing the sound effect.</returns>
    /// <returns>The sound effect instance created by this method.</returns>
    public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan, bool isLooped)
    {
        // Create an instance from the sound effect given.
        SoundEffectInstance soundEffectInstance = soundEffect.CreateInstance();

        // Apply the volume, pitch, pan, and loop values specified.
        soundEffectInstance.Volume = volume;
        soundEffectInstance.Pitch = pitch;
        soundEffectInstance.Pan = pan;
        soundEffectInstance.IsLooped = isLooped;

        // Tell the instance to play
        soundEffectInstance.Play();

        // Add it to the active instances for tracking
        _activeSoundEffectInstances.Add(soundEffectInstance);

        return soundEffectInstance;
    }

    /// <summary>
    /// Plays the given song. Will cancel the ongoing song first.
    /// </summary>
    /// <param name="song">The song to play.</param>
    /// <param name="isRepeating">Optionally specify if the song should repeat.  Default is true.</param>
    public void PlaySong(Song song, bool isRepeating = true)
    {
        // Check if the media player is already playing, if so, stop it.
        // If we do not stop it, this could cause issues on some platforms
        if (MediaPlayer.State is MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        MediaPlayer.Play(song);
        MediaPlayer.IsRepeating = isRepeating;
    }

    /// <summary>
    /// Pauses all audio.
    /// </summary>
    public void PauseAudio()
    {
        // Pause any active songs playing.
        MediaPlayer.Pause();

        // Pause any active sound effects.
        foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances)
        {
            soundEffectInstance.Pause();
        }
    }

    /// <summary>
    /// Resumes play of all previous paused audio.
    /// </summary>
    public void ResumeAudio()
    {
        // Resume paused music
        MediaPlayer.Resume();

        // Resume any active sound effects.
        foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances)
        {
            soundEffectInstance.Resume();
        }
    }

    /// <summary>
    /// Mutes all audio.
    /// </summary>
    public void MuteAudio()
    {
        // Store the volume so they can be restored during ResumeAudio
        _previousSongVolume = MediaPlayer.Volume;
        _previousSoundEffectVolume = SoundEffect.MasterVolume;

        // Set all volumes to 0
        MediaPlayer.Volume = 0.0f;
        SoundEffect.MasterVolume = 0.0f;

        IsMuted = true;
    }

    /// <summary>
    /// Unmutes all audio to the volume level prior to muting.
    /// If audio is not currently muted, does nothing.
    /// </summary>
    public void UnmuteAudio()
    {
        if (IsMuted is false)
            return;

        // Restore the previous volume values.
        MediaPlayer.Volume = _previousSongVolume;
        SoundEffect.MasterVolume = _previousSoundEffectVolume;

        IsMuted = false;
    }

    /// <summary>
    /// Toggles the current audio mute state.
    /// </summary>
    public void ToggleMute()
    {
        if (IsMuted)
            UnmuteAudio();
        else
            MuteAudio();
    }

    /// <summary>
    /// Disposes of this audio controller and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes this audio controller and cleans up resources.
    /// Microsoft wants this separate method. I guess the idea is
    /// the finalizer (~AudioController) calls with false because
    /// garbage collection will end up calling dispose on the managed
    /// resources anyways.
    /// https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose#the-disposebool-method-overload
    /// </summary>
    /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if(IsDisposed)
            return;

        if (disposing)
        {
            foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstances)
            {
                soundEffectInstance.Dispose();
            }
            _activeSoundEffectInstances.Clear();
        }

        IsDisposed = true;
    }
}