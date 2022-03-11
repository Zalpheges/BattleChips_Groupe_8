using UnityEngine.UI;
using UnityEngine;
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

	private static UIManager _instance;

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

	[SerializeField]
	private GameObject _boardParent;

	#region Play

	[SerializeField]
	private GameObject _gameParent;

	[SerializeField]
	private TextMeshProUGUI _playerName;

	[SerializeField]
	private TextMeshProUGUI _turnText;

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

    private void Awake()
    {
        _instance = this;
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
		_instance._playerCount.text = $"{ready} / {total} joueurs prêts.";
	}

	public static void SetTurn(string username)
	{
		_instance._turnText.text = $"Tour de {username}";

		ShowMenu(Menu.Play);
	}

	public static void ShowMenu(Menu menu)
    {
		_instance._connectionParent.SetActive(menu == Menu.Connect);
		_instance._readyParent.SetActive(menu == Menu.Ready);
		_instance._boardParent.SetActive(menu == Menu.Board);
		_instance._gameParent.SetActive(menu == Menu.Play);
		_instance._shootParent.SetActive(menu == Menu.Shoot);
		_instance._endParent.SetActive(menu == Menu.End);

		_instance._background.SetActive(menu != Menu.Play && menu != Menu.Shoot);
	}

	public static void ShowPlayerName(string playerName)
    {
		_instance._playerName.text = playerName;
    }

	public static void ShowShoot(string from, string to)
    {
		_instance._shootText.text = $"{from} tire sur {to}.";

		ShowMenu(Menu.Shoot);
	}

	public static void ShowEnd(int id, string[] usernames)
	{
		string winner = string.Empty;
		string[] loosers = new string[usernames.Length];

		for (int i = 0; i < usernames.Length; ++i)
        {
			if (i == id)
				winner = usernames[i];
			else
				loosers[i < id ? i : i - 1] = usernames[i];
        }

		_instance._winnerText.text = $"Vainqueur :\n{winner}";
		_instance._loosersText.text = string.Join("\n", loosers);

		ShowMenu(Menu.End);
	}
}
