using System;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
[CreateAssetMenu]
public class AreaTheme : ScriptableObject
{
	[SerializeField] private Sound _ambient;
	[SerializeField] private RandomSound[] _randomSounds;
	[SerializeField, Range(0, 1)] private float _baseVolume = 1;

	public string Name => _ambient.Type;

    [NotNull]
    public Sound AmbientSound => _ambient;

	[NotNull]
    public RandomSound[] RandomSounds => _randomSounds;

	public float BaseVolume => _baseVolume;
}