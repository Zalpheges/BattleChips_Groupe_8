using System.Collections.Generic;
using PlayerIOClient;
using UnityEngine;
using TMPro;

public class ClientManager : MonoBehaviour
{
	private Queue<Message> messages;

	private Connection server;

	private static ClientManager instance;

	private int nCurrentPlayer = 0;

    private void Awake()
    {
		if (instance == null)
        {
            instance = this;
			DontDestroyOnLoad(gameObject);
        }
        else
			Destroy(gameObject);
    }

    private void Start()
    {
		messages = new Queue<Message>();

		if (PlayerPrefs.HasKey("Username"))
        {
			username.text = PlayerPrefs.GetString("Username");
			roomname.text = PlayerPrefs.GetString("Roomname");
		}
	}

	public void Connect()
    {
		if (username.text.Length < 3 || roomname.text.Length < 3)
			return;

		PlayerIO.Authenticate(
			"battlechips-tmwm0lz8memju96zesetw",
			"public",
			new Dictionary<string, string> {
				{ "userId", username.text },
			},
			null,
			delegate (Client client) {
				Debug.Log("Successfully connected to Player.IO");

				client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

				client.Multiplayer.CreateJoinRoom(
					roomname.text,
					"BattleChip",
					true,
					null,
					null,
					delegate (Connection connection) {
						Debug.Log("Joined Room.");

						PlayerPrefs.SetString("Username", username.text);
						PlayerPrefs.SetString("Roomname", roomname.text);

						server = connection;
						server.OnMessage += OnMessage;
						server.OnDisconnect += OnDisconnect;

						server.Send("Count");
						ShowMenu(Menu.Ready);
					},
					delegate (PlayerIOError error) {
						Debug.Log("Error Joining Room: " + error.ToString());

						ShowMenu(Menu.Connect);
					}
				);
			},
			delegate (PlayerIOError error) {
				Debug.Log("Error connecting: " + error.ToString());

				ShowMenu(Menu.Connect);
			}
		);
	}

	public void Ready(bool state)
    {
		server.Send("Ready", state);
    }

	public static void AddShip(int x, int y, int dir, int length)
    {
		instance.server.Send("Add", x, y, dir, length);
	}

	public static void RemoveShip(int x, int y, int dir, int length)
	{
		instance.server.Send("Add", x, y, dir, length);
	}

	public static void Boarded()
    {
		instance.server.Send("Boarded");
    }

	private void FixedUpdate()
	{
		while (messages.Count > 0)
		{
			Message message = messages.Dequeue();

			switch (message.Type)
			{
				case "Board":
                {
					int id = message.GetInt(0);
					int count = message.GetInt(1);

					ShowMenu(Menu.None);
					Map.nPlayers = count;
					Map.myId = id;
					Map.Init();
					Main.instance.canvasSelection.SetActive(true);
					
					// Lancer la selection de board et répondre "Boarded" quand terminé CHECK

					break;
                }

				case "Count":
                {
					int ready = message.GetInt(0);
					int total = message.GetInt(1);

					playerCount.text = ready + "/" + total + " prêt" + (ready > 1 ? "s" : "");

					break;
                }

				case "Shoot":
                {
					int id = message.GetInt(0);
					int x = message.GetInt(1);
					int y = message.GetInt(2);

					bool touched = message.GetBoolean(3);
					bool destroyed = message.GetBoolean(4);

					if (touched) {
						// Lancer le missile avec la variable destroyed pour indiquer s'il faut afficher le bateau coulé pendant l'animation
                    }
					else {
						// Case devient rouge
                    }
					nCurrentPlayer++;
					nCurrentPlayer %= Map.nPlayers;
					break;
                }

				case "Play":
				{
					Main.instance.canvasSelection.SetActive(false);
					// La partie vient de commencer tous les joueurs ont répondu "Boarded"

					break;
                }

				case "Dead":
                {
					int id = message.GetInt(0);

					// Le joueur id vient de perdre son dernier vaisseau

					break;
                }

				case "End":
                {
					int winner = message.GetInt(0);

					ShowMenu(Menu.End);

					break;
                }
			}
		}
	}

	private void OnMessage(object sender, Message message)
	{
		messages.Enqueue(message);
	}

	private void OnDisconnect(object sender, string reason)
    {
		Disconnect();
    }

	public static void Disconnect()
    {
		if (instance.server != null)
		{
			instance.server.Disconnect();
			instance.server = null;
        }

		instance.ShowMenu(Menu.Connect);
    }

    private void OnDestroy()
    {
		if (server != null)
			Disconnect();
    }

	#region UI

	[SerializeField]
	private TMP_InputField username;

	[SerializeField]
	private TMP_InputField roomname;

	[SerializeField]
	private TextMeshProUGUI playerCount;

	[SerializeField]
	private GameObject connect;

	[SerializeField]
	private GameObject ready;

	private enum Menu
    {
		None,
		Connect,
		Ready,
		End
    }

	private void ShowMenu(Menu menu)
    {
		connect.SetActive(menu == Menu.Connect);
		ready.SetActive(menu == Menu.Ready);
    }

	#endregion
}
