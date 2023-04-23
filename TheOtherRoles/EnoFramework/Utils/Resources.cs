using System;
using System.Collections.Generic;
using System.Reflection;
using TheOtherRoles.EnoFramework.Kernel;
using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.EnoFramework.Utils;

public static class Resources
{
    public static readonly Dictionary<string, Sprite> CachedSprites = new();

    public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            var texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
            throw new KernelException($"Unable to load sprite {path}");
        }
    }

    public static unsafe Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            if (stream == null) return texture;
            var length = stream.Length;
            var byteTexture = new Il2CppStructArray<byte>(length);
            var read = stream.Read(new Span<byte>(
                IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(),
                (int) length));
            if (read <= 0)
            {
                throw new KernelException($"Unable to load texture {path}");
            }
            ImageConversion.LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from resources: " + path);
            throw new KernelException($"Unable to load texture {path}");
        }
    }

    public static AudioClip? LoadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
    {
        // must be "raw (headerless) 2-channel signed 32 bit pcm (le)" (can e.g. use Audacity® to export)
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            if (stream != null)
            {
                var byteAudio = new byte[stream.Length];
                _ = stream.Read(byteAudio, 0, (int) stream.Length);
                var samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
                int offset;
                for (var i = 0; i < samples.Length; i++)
                {
                    offset = i * 4;
                    samples[i] = (float) BitConverter.ToInt32(byteAudio, offset) / Int32.MaxValue;
                }

                var channels = 2;
                var sampleRate = 48000;
                var audioClip = AudioClip.Create(clipName, samples.Length, channels, sampleRate, false);
                audioClip.SetData(samples, 0);
                return audioClip;
            }
        }
        catch
        {
            System.Console.WriteLine("Error loading AudioClip from resources: " + path);
        }

        return null;

        /* Usage example:
        AudioClip exampleClip = Helpers.loadAudioClipFromResources("TheOtherRoles.Resources.exampleClip.raw");
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
        */
    }
}