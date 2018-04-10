using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{

	#region Toggle Sound


	public static bool SoundEnabled
	{
		get
		{
			if (!PlayerPrefs.HasKey(SoundEnabledTag))
				PlayerPrefs.SetInt(SoundEnabledTag, 1);
			return PlayerPrefs.GetInt(SoundEnabledTag) == 1;
		}
		set
		{
			PlayerPrefs.SetInt(SoundEnabledTag, value ? 1 : 0);
			AudioListener.pause = !value;
			AudioListener.volume = value ? SoundVolume : 0;
		}
	}

	public static float SoundVolume
	{
		get
		{
			if (!PlayerPrefs.HasKey(SoundVolumeTag))
				PlayerPrefs.SetFloat(SoundVolumeTag, 1);
			return PlayerPrefs.GetFloat(SoundVolumeTag);
		}
		set
		{
			value = Mathf.Clamp01(value);
			PlayerPrefs.SetFloat(SoundVolumeTag, value);
			AudioListener.volume = value;
		}
	}

	private const string SoundVolumeTag = "_soundVolume";
	private const string SoundEnabledTag = "_soundEnabled";

	#endregion



	public SoundsBase Base;

	public static AudioManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	public void StopAllSounds()
	{
		foreach (Transform child in SourceParent)
		{
			if (!child.gameObject.activeSelf) continue;
			child.GetComponent<AudioSource>().Stop();
		}
	}


	public void SetAmbientVolume(float factor)
	{
		CurrentAmbientVolume = Mathf.Clamp(factor, 0, 1);
	}
	public float CurrentAmbientVolume { get; private set; } = 1;


	/// <summary>
	/// Dump Ambient volume to specific rate to restore it over time
	/// </summary>
	public void DumpVolume(float value, float time = 2f)
	{
		if (_dumpVolumeCoroutine != null) StopCoroutine (_dumpVolumeCoroutine);
		CurrentAmbientVolume = value;

		_dumpVolumeCoroutine = StartCoroutine(DumpVolumeCoroutine(time));
	}
	private Coroutine _dumpVolumeCoroutine;

	private IEnumerator DumpVolumeCoroutine(float restoreTime)  
	{
		float elapsed = 0;
		float tweenFrom = CurrentAmbientVolume;
		float tweenDiff = 1 - CurrentAmbientVolume;
		while (elapsed <= restoreTime)
		{
			yield return null;
			CurrentAmbientVolume = tweenFrom + tweenDiff * elapsed / restoreTime;
			elapsed += Time.deltaTime;
		}
	}


	#region Play Ambient

	private readonly HashSet<AreaTheme> _themes = new HashSet<AreaTheme>();
	private readonly HashSet<AreaTheme> _alreadyPlayingThemes = new HashSet<AreaTheme>();
	private float _themeFadeFactor = 0.5f;

	public float ThemeFadeFactor
	{
		get { return _themeFadeFactor; }
		set { _themeFadeFactor = Mathf.Clamp01(value); }
	}

	private AreaTheme ActiveTheme { get; set; }

	public void AddThemes(params AreaTheme[] themes)
	{
		_themes.Clear();
		_alreadyPlayingThemes.Clear();
		ActiveTheme = null;
		foreach (var theme in themes)
		{
			if (theme == null || !theme.AmbientSound.IsNotEmpty())
			{
				Debug.LogError("AddTheme caused: theme is null or empty");
				return;
			}
			if (_themes.Any(t => t.Name == theme.Name))
			{
				Debug.LogError("AddTheme caused: Theme with name " + theme.Name + " already exists");
				return;
			}

			_themes.Add(theme);
		}
	}

	public void SetTheme(string themeName)
	{
		StartCoroutine(ActivateTheme(themeName));
	}
	

	private IEnumerator ActivateTheme(string themeName)
	{
		yield return null;

		// If null then fade all themes
		if (themeName == null || themeName.Length == 0)
		{
			ActiveTheme = null;
			yield break;
		}

		AreaTheme theme = _themes.FirstOrDefault(t => t.Name == themeName);

		// If there is no such theme - do nothing
		if (theme == null)
		{
			yield break;
		}

		// If this theme is already active - do nothing
		if (ActiveTheme != null && theme.Name == ActiveTheme.Name)
		{
			yield break;
		}

		// Make this theme active
		ActiveTheme = theme;

		// If theme is already playing - do nothing
		if (!_alreadyPlayingThemes.Add(theme))
		{
			yield break;
		}

		// Sarting theme playing
		AudioClip clip = GetClipFromSound(theme.AmbientSound);

		var listener = FindObjectOfType<AudioListener>().transform;
		AudioSource source = GetAudioSource(listener, listener.position);

		source.clip = clip;
		source.loop = true;
		source.volume = 0;
		source.Play();


		Dictionary<RandomSound, float> randomSoundDelays =
			theme.RandomSounds
			.Where(r => r.Sound.IsNotEmpty())
			.ToDictionary(k => k, v => Random.Range(5f, v.MaxSecondsDelay));

		float activeRaito = theme.Name == ActiveTheme.Name ? 1 : 0;

		// Store all currently playing theme sounds to be able change their volume
		var activeAudioSources = new HashSet<AudioSource>(new[] { source });

		while (true)
		{
			yield return null;

			bool isActive = ActiveTheme.Name == theme.Name;
			activeRaito = Mathf.Lerp(activeRaito, isActive ? 1 : 0, Time.deltaTime * ThemeFadeFactor);

			float actualVolume = theme.BaseVolume * activeRaito * CurrentAmbientVolume;

			// Remove all sounds that are already finished playing
			activeAudioSources.RemoveWhere(a => a != source && !a.isPlaying);

			// Change all sounds volume depending to the actual one
			foreach (AudioSource audioSource in activeAudioSources)
			{
				audioSource.volume = actualVolume;
			}

			// Play random sounds
			foreach (RandomSound sound in randomSoundDelays.Keys.ToArray())
			{
				randomSoundDelays[sound] -= Time.deltaTime;

				if (randomSoundDelays[sound] <= 0)
				{
					AudioSource audioSource = Play(sound.Sound, actualVolume);

					// Store sound source for later use
					activeAudioSources.Add(audioSource);

					randomSoundDelays[sound] = Random.Range(sound.MinSecondsDelay, sound.MaxSecondsDelay);
				}
			}
		}
	}

	#endregion


	/// <summary>
	/// Play 3d sound with min and max distance at point
	/// </summary>
	public AudioSource Play3D(Sound sound, Vector3 point, float minDistance, float maxDistance, float volume = 1, float pitch = 1)
	{
		return PlaySound(sound, point, volume, pitch, minDistance, maxDistance);
	}

	//TODO: !! We need static reference to Player View
	/// <summary>
	/// Play sound
	/// </summary>
	public AudioSource Play(Sound sound, float volume = 1, float pitch = 1, bool loop = false)
	{
		if (_listener == null) _listener = FindObjectOfType<AudioListener>().gameObject;
		
		return PlaySound(sound, _listener.transform.position, volume, pitch, -1, -1, loop);
	}
	private GameObject _listener;


	private AudioSource PlaySound(Sound sound, Vector3 point, float volume, float pitch, float minDistance = -1, float maxDistanve = -1, bool loop = false)
	{
		AudioClip clip = GetClipFromSound(sound);

		AudioSource source = GetAudioSource(SourceParent, point);

		source.clip = clip;
		source.volume = volume;
		source.pitch = pitch;

		if (minDistance >= 0)
		{
			source.minDistance = minDistance;
			source.maxDistance = maxDistanve;
			source.spatialBlend = 1;
		}

		if (loop) source.loop = true;

		source.Play();

		if (!loop) StartCoroutine(DestroyOnPlayed(source));

		return source;
	}


	#region Get Clip

	/// <summary>
	/// Get random AudioClip from sound
	/// </summary>
	private AudioClip GetClipFromSound(Sound sound)
	{
		if (sound == null)
		{
			Debug.LogError("Sound is null", this);
			return null;
		}

		if (sound.Type == "Custom")
		{
			if (sound.Clips == null || sound.Clips.Count == 0 || sound.Clips[0] == null)
			{
				Debug.LogError("Cusnom sound is empty", this);
				return null;
			}
			return sound.Clips[0];
		}
		else
		{
			var searched = Base.GetSound(sound.Id);
			if (searched == null)
			{
				Debug.LogError("Sound of type " + sound.Type + " not found", this);
				return null;
			}
			return searched.Clips.ElementAt(Random.Range(0, searched.Clips.Count));
		}

	}

	#endregion


	#region Get & Destroy (Pool realization)

	/// <summary>
	/// Get GO with AudioSource;
	/// </summary>
	private AudioSource GetAudioSource(Transform parent, Vector3 position)
	{
		AudioSource source;
		if (_pool.Count == 0)
		{
			source = NewAudioSource();
		}
		else
		{
			source = _pool.Pop();
			if (source == null) source = NewAudioSource();
			source.gameObject.SetActive(true);
		}

		source.volume = 1;
		source.pitch = 1;
		source.spatialBlend = 0;
		source.maxDistance = 50;
		source.minDistance = 5;
		source.spread = 150;
		source.rolloffMode = AudioRolloffMode.Linear;

		source.transform.SetParent(parent);
		source.transform.position = position;

		return source;
	}

	private Transform SourceParent
	{
		get
		{
			if (_sourceParent == null)
			{
				_sourceParent = new GameObject("__AudioPool");
				DontDestroyOnLoad(_sourceParent);
			}

			return _sourceParent.transform;
		}
	}
	private GameObject _sourceParent;

	private AudioSource NewAudioSource()
	{
		var go = new GameObject("Audio Source");
		go.transform.SetParent(SourceParent);

		var source = go.AddComponent<AudioSource>();

		return source;
	}
	private readonly Stack<AudioSource> _pool = new Stack<AudioSource>();


	/// <summary>
	/// Move AudioSource to pool on played
	/// </summary>
	IEnumerator DestroyOnPlayed(AudioSource source)
	{
		float length = source.clip.length;

		yield return new WaitForSeconds(length);

		source.gameObject.SetActive(false);
		_pool.Push(source);
	}

	#endregion

}


public static class AudioSourceExtension
{
	public static float TimeOfFinish(this AudioSource source)
	{
		var currentTime = Time.time;
		return currentTime + source.clip.length;
	}
}