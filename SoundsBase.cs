using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SoundsBase : ScriptableObject
{
	public List<Sound> Sounds = new List<Sound>();



	/// <summary>
	/// Use in playmode to get sound by id 
	/// </summary>
	public Sound GetSound(string id)
	{
		if (Application.isPlaying && _cachedBase == null || !Application.isPlaying)
			_cachedBase = Sounds.ToDictionary(s => s.Id, s => s);
		
		if (_cachedBase.ContainsKey(id))
			return _cachedBase[id];

		Debug.LogError("Sound with id " + id + " not found in base");
		return null;
	}

	private Dictionary<string, Sound> _cachedBase;

}
