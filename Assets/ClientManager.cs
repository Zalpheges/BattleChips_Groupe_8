using System.Collections.Generic;
using PlayerIOClient;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
	private static ClientManager instance;

	private Queue<Message> messages;

	private Connection server;
	public static bool Wait = false;

	public static int nCurrentPlayer = -1;

	public static bool MyTurn => nCurrentPlayer == Map.myId;

	[SerializeField] private GameObject _prefabMissile;
	[SerializeField] private Transform _exampleFiringPoint;
	[SerializeField] private Transform _examplePlayer;
	private Vector3 _offsetMissileSpawn;


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

		_offsetMissileSpawn = _exampleFiringPoint.position - _examplePlayer.position;
	}

	public static void Connect(string username, string roomname)
	{
		PlayerIO.Authenticate(
			"battlechips-tmwm0lz8memju96zesetw",
			"public",
			new Dictionary<string, string> {
				{ "userId", username },
			},
			null,
			delegate (Client client) {
				Debug.Log("Successfully connected to Player.IO");

				client.Multiplayer.DevelopmentServer = new ServerEndpoint("25.59.158.42", 8184);
				//client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

				client.Multiplayer.CreateJoinRoom(
					roomname,
					"BattleChip",
					true,
					null,
					null,
					delegate (Connection connection) {
						Debug.Log("Joined " + roomname);

						instance.server = connection;
						instance.server.OnMessage += instance.OnMessage;
						instance.server.OnDisconnect += instance.OnDisconnect;

						instance.server.Send("Count");
					},
					delegate (PlayerIOError error) {
						Debug.Log("Error Joining Room: " + error.ToString());

						UIManager.ShowMenu(UIManager.Menu.Connect);
					}
				);
			},
			delegate (PlayerIOError error) {
				Debug.Log("Error connecting: " + error.ToString());

				UIManager.ShowMenu(UIManager.Menu.Connect);
			}
		);
	}

	public void Ready(bool state)
	{
		server.Send("Ready", state);
	}

	public static void AddShip(int id, int x, int y, int dir, int length)
	{
		Debug.Log("Add");
		instance.server.Send("Add", id, x, y, dir, length);
	}

	public static void RemoveShip(int x, int y)
	{
		Debug.Log("Remove");
		instance.server.Send("Remove", x, y);
	}

	public static void Boarded()
	{
		Debug.Log("Boarded");
		InputManager.boarded = true;
		instance.server.Send("Boarded");
	}
	public static void Shoot(int id, int x, int y)
	{
		Debug.Log("Shoot");
		instance.server.Send("Shoot", id, x, y);
	}

	private void FixedUpdate()
	{
		while (messages.Count > 0 && !Wait)
		{
			Message message = messages.Dequeue();

			switch (message.Type)
			{
				case "Board":
				{
					int id = message.GetInt(0);
					int count = message.GetInt(1);

					string[] playersName = ExtractMessage<string>(message, 2);

					Map.nPlayers = count;
					Map.myId = id;
					Map.Init(playersName);
					InputManager.connected = true;
					InputManager.instance.CanvasSelection.SetActive(true);

					UIManager.ShowMenu(UIManager.Menu.None);

					break;
				}

				case "Count":
				{
					int ready = message.GetInt(0);
					int total = message.GetInt(1);

					UIManager.SetCount(ready, total);

					break;
				}

				case "Shoot":
				{
					int id = message.GetInt(0);
					int x = message.GetInt(1);
					int y = message.GetInt(2);

					bool touched = message.GetBoolean(3);
					bool destroyed = message.GetBoolean(4);

					int idShip = 0, xShip = 0, yShip = 0, dirShip = 0;

					if (destroyed)
					{
						idShip = message.GetInt(5);
						xShip = message.GetInt(6);
						yShip = message.GetInt(7);
						dirShip = message.GetInt(8);
					}

					Transform currentPlayerT = Map.GetPlayerById(nCurrentPlayer).transform;
					Player targetedPlayer = Map.GetPlayerById(id);

					Wait = true;
					Debug.Log("Shoot");
					Missile missile = Instantiate(_prefabMissile).GetComponent<Missile>();
					Vector3 relativeOffset = currentPlayerT.forward * _offsetMissileSpawn.z + currentPlayerT.right * _offsetMissileSpawn.x;

					UIManager.ShowShoot(Map.GetPlayerById(nCurrentPlayer).nickName, Map.GetPlayerById(id).nickName);
					missile.Init(currentPlayerT.position + relativeOffset, targetedPlayer.GetWorldPosition(x, y), touched, id);
					missile.SetText(Map.GetPlayerById(nCurrentPlayer).nickName + " tire sur " + Map.GetPlayerById(id).nickName);

					if (touched)
					{
						// Lancer le missile avec la variable destroyed pour indiquer s'il faut afficher le bateau coul� pendant l'animation
						targetedPlayer.ShipCellHit(x, y);
						if (destroyed)
						{
							Transform shipT;
							if (!targetedPlayer.you)
							{
								Vector3 yRotation = new Vector3(0, -dirShip * 90, 0);
								shipT = Instantiate(InputManager.chipsButtons[idShip].transform.GetChild(0), targetedPlayer.GetWorldPosition(xShip, yShip),
									targetedPlayer.transform.rotation * Quaternion.Euler(yRotation), targetedPlayer.transform);
							}
							else
								shipT = targetedPlayer.GetShip(x, y).transform;
						missile.SetDestroyedShip(shipT);
						}
					}
					else
					{
						// Case devient rouge
						targetedPlayer.EmptyCellHit(x, y);
					}
					do
					{
						++nCurrentPlayer;
						nCurrentPlayer %= Map.nPlayers;

					} while (Map.GetPlayerById(nCurrentPlayer).dead);
					Player currentPlayer = Map.GetPlayerById(nCurrentPlayer);
					_currentPlayerText.text = $"Tour de {currentPlayer.nickName}";
					InputManager.currentState = currentPlayer.id == nCurrentPlayer ? InputManager.PlayerState.Aiming : InputManager.PlayerState.Waiting;
					break;
				}

				case "Play":
				{
					nCurrentPlayer = 0;
					Player currentPlayer = Map.GetPlayerById(nCurrentPlayer);
					InputManager.currentState = currentPlayer.id == nCurrentPlayer ? InputManager.PlayerState.Aiming : InputManager.PlayerState.Waiting;

					InputManager.instance.CanvasSelection.SetActive(false);
					_gamePanel.SetActive(true);

					UIManager.SetTurn(currentPlayer.nickName);

					break;
				}

				case "Dead":
				{
					int id = message.GetInt(0);

					Map.GetPlayerById(id).dead = true;
					// Le joueur id vient de perdre son dernier vaisseau

					break;
				}

				case "End":
				{
					int winner = message.GetInt(0);

					//UIManager.ShowEnd()

					break;
				}
			}
		}
	}

	private void OnMessage(object sender, Message message)
	{
		messages.Enqueue(message);
	}

	private T[] ExtractMessage<T>(Message message, uint startIndex = 0)
	{
		T[] items = new T[message.Count - startIndex];

		for (uint i = startIndex; i < message.Count; ++i)
			items[i - startIndex] = (T)message[i];

		return items;
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

		UIManager.ShowMenu(UIManager.Menu.Connect);
	}

	private void OnDestroy()
	{
		if (server != null)
			Disconnect();
	}
}
