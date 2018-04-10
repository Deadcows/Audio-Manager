using System.Collections.Generic;
using UnityEngine;

public class FootstepCaster : MonoBehaviour
{

	[Space, SerializeField]
	private Sound _indoorSteps = Sound.WithType("Footsteps/Indoor");
	[Range(0, 1), SerializeField]
	private float _indoorStepVolume = 1;
	
	
	/// <summary>
	/// Called by animation
	/// </summary>
	// ReSharper disable once UnusedMember.Local
	private void CastStep()
	{
		var sound = GetSteps();
		AudioManager.Instance.Play(sound.Key, Random.Range(sound.Value - .05f, sound.Value + .05f), Random.Range(.8f, 1));
    }
	
	//private void OnTriggerEnter2D(Collider2D other)
	//{
	//	var floor = other.GetComponent<Floor>();
	//	if (floor != null)
	//	{
	//		_latestType = floor.FloorType;

	//		SwitchAmbientTheme(floor.FloorType);
	//	}
	//}

	//private void SwitchAmbientTheme(FloorType type)
	//{
	//	// If current ambient theme is not standard = we wont change it automatically in footstep caster
	//	if (GameManager.CurrentScene.AmbientTheme != AmbientThemeType.Indoor &&
	//		GameManager.CurrentScene.AmbientTheme != AmbientThemeType.Outdoor) return;

	//	if (IsIndoorFloor(type)) GameManager.CurrentScene.AmbientTheme = AmbientThemeType.Indoor;
	//	if (IsOutdoorFloor(type)) GameManager.CurrentScene.AmbientTheme = AmbientThemeType.Outdoor;
	//}

	//private bool IsIndoorFloor(FloorType type)
	//{
	//	return type == FloorType.Indoor;
	//}

	//private bool IsOutdoorFloor(FloorType type)
	//{
	//	if (type == FloorType.Outdoor) return true;
	//	if (type == FloorType.Snow) return true;
	//	if (type == FloorType.Sand) return true;

	//	return false;
	//}
	
	private KeyValuePair<Sound, float> GetSteps()
	{
		return new KeyValuePair<Sound, float>(_indoorSteps, _indoorStepVolume);
	}
}
