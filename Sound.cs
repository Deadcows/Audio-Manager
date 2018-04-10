using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound
{
	//TODO: Add Min/Max distance in Sound Drawer on custom sounds (draw gizmos?)
	public string Type = "Custom";
	public List<AudioClip> Clips;

	public string Id = string.Empty;

	//TODO: WithType is not working purly through code?
	public static Sound WithType(string type)
	{
		return new Sound { Type = type };
	}

	public static Sound Wrap(AudioClip clip)
	{
		return new Sound { Type = "Custom", Clips = new List<AudioClip> { clip } };
	}

}

public static class SoundExtension
{
	/// <summary>
	/// Sound contains any clips to play
	/// </summary>
	public static bool IsNotEmpty(this Sound sound)
	{
		if (sound == null) return false;
		if (sound.Type == "Custom") return sound.Clips != null && sound.Clips.Count > 0 && sound.Clips[0] != null;
		return AudioManager.Instance.Base.GetSound(sound.Id) != null;
	}

}


