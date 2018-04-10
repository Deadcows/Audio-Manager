using UnityEngine;

public class AmbientThemeTransmtter : MonoBehaviour
{
	public AreaTheme Theme;

	public void ChangeTheme()
	{
		AudioManager.Instance.SetTheme(Theme.Name);
	}
}
