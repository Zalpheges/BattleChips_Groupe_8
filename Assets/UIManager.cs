using UnityEngine.UI;
using UnityEngine;
using System.Text;
using TMPro;

public class UIManager : MonoBehaviour
{
	public enum Menu
	{
		Connect,
		Ready,
		Board,
		Play,
		Shoot,
		End
	}

	private static UIManager instance;

	[SerializeField]
	private GameObject _background;

	#region Connection

	[SerializeField]
	private GameObject _connectionParent;

	[SerializeField]
	private Button _connectionButton;

	[SerializeField]
	private TMP_InputField _usernameInput;

	[SerializeField]
	private TMP_InputField _roomnameInput;

	#endregion

	#region Ready

	[SerializeField]
	private GameObject _readyParent;

	[SerializeField]
	private TextMeshProUGUI _playerCount;

	#endregion

	#region Shoot

	[SerializeField]
	private GameObject _shootParent;

	[SerializeField]
	private TextMeshProUGUI _shootText;

	#endregion

	#region End

	[SerializeField]
	private GameObject _endParent;

	[SerializeField]
	private TextMeshProUGUI _winnerText;

	[SerializeField]
	private TextMeshProUGUI _loosersText;

	#endregion

	#region Game

	[SerializeField]
	private GameObject _gameParent;

	[SerializeField]
	private TextMeshProUGUI _turnText;

	#endregion

	[SerializeField]
	private GameObject _boardParent;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
		ShowMenu(Menu.Connect);

		if (PlayerPrefs.HasKey("Username"))
			_usernameInput.text = PlayerPrefs.GetString("Username");

		if (PlayerPrefs.HasKey("Roomname"))
			_roomnameInput.text = PlayerPrefs.GetString("Roomname");
	}

	public void Connect()
	{
		if (_usernameInput.text.Length >= 3 && _roomnameInput.text.Length >= 3)
		{
			_connectionButton.interactable = false;

			PlayerPrefs.SetString("username", _usernameInput.text);
			PlayerPrefs.SetString("roomname", _roomnameInput.text);

			ClientManager.Connect(_usernameInput.text, _roomnameInput.text);
		}
	}

	public static void SetCount(int ready, int total)
    {
		instance._playerCount.text = $"{ready} / {total} joueurs prêts.";
	}

	public static void SetTurn(string username)
	{
		instance._turnText.text = $"Tour de {username}";
	}

	public static void ShowMenu(Menu menu)
    {
		instance._background.SetActive(menu != Menu.Play && menu != Menu.Shoot);
		instance._connectionParent.SetActive(menu == Menu.Connect);
		instance._boardParent.SetActive(menu == Menu.Board);
		instance._readyParent.SetActive(menu == Menu.Ready);
		instance._endParent.SetActive(menu == Menu.End);
		instance._gameParent.SetActive(menu == Menu.Play);
		instance._shootParent.SetActive(menu == Menu.Shoot);
	}

	public static void ShowShoot(string from, string to)
    {
		instance._shootText.text = $"{from} tire sur {to}.";

		ShowMenu(Menu.Shoot);
	}

	public static void ShowEnd(string winner, string[] loosers)
	{
		instance._winnerText.text = $"Vainqueur :\n{winner}";
		instance._loosersText.text = string.Join("\n", loosers);

		ShowMenu(Menu.End);
	}
}
