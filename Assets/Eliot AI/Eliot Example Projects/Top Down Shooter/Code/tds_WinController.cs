using UnityEngine;

public class tds_WinController : MonoBehaviour
{

	public GameObject WinCanvas;
	public int NumberOfBandits;

	public void BanditIsDead()
	{
		NumberOfBandits--;
		if (NumberOfBandits <= 0)
		{
			Instantiate(WinCanvas);
		}
	}
}
