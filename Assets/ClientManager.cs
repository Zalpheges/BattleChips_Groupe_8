using System.Collections.Generic;
using PlayerIOClient;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
	private static ClientManager _instance;

	private Queue<Message> _messages;

	private Connection _server;
	public static bool Wait = false;

	private int _currentTurn = -1;

	public static int MyID { get; private set; }
	public static bool MyTurn => _instance._currentTurn == MyID;

	[SerializeField]
	private Missile _missilePrefab;
	[SerializeField] private Transform _exampleFiringPoint;
	[SerializeField] private Transform _examplePlayer;
	private Vector3 _localSpawnPosition;

	private Player[] _players;

	private Player Me => _players[MyID];
	private Player CurrentPlayer => _players[_currentTurn];

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		_messages = new Queue<Message>();

		_localSpawnPosition = _exampleFiringPoint.position - _examplePlayer.position;
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

						_instance._server = connection;
						_instance._server.OnMessage += _instance.OnMessage;
						_instance._server.OnDisconnect += _instance.OnDisconnect;

						_instance._server.Send("Count");
					},
					delegate (PlayerIOError error) {
						Debug.Log($"Error Joining Room: {error}");

						UIManager.ShowMenu(UIManager.Menu.Connect);
					}
				);
			},
			delegate (PlayerIOError error) {
				Debug.Log($"Error connecting: {error}");

				UIManager.ShowMenu(UIManager.Menu.Connect);
			}
		);
	}

	public void Ready(bool state)
	{
		_server.Send("Ready", state);
	}

	public static void AddShip(int id, int x, int y, int dir, int length)
	{
		_instance._server.Send("Add", id, x, y, dir, length);
	}

	public static void RemoveShip(int x, int y)
	{
		_instance._server.Send("Remove", x, y);
	}

	public static void Boarded()
	{
		GameManager.Boarded = true;
		_instance._server.Send("Boarded");
	}
	public static void Shoot(int id, int x, int y)
	{
		_instance._server.Send("Shoot", id, x, y);
	}

	private void FixedUpdate()
	{
		while (_messages.Count > 0 && !Wait)
		{
			Message message = _messages.Dequeue();

			switch (message.Type)
			{
				case "Board":
				{
					int id = message.GetInt(0);
					int count = message.GetInt(1);

					string[] playersName = ExtractMessage<string>(message, 2);

					_players = Map.CreatePlayers(id, count);

					InputManager.connected = true;
					InputManager.instance.CanvasSelection.SetActive(true);

					UIManager.ShowMenu(UIManager.Menu.Board);

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

					Player target = _players[id];
					Transform player = CurrentPlayer.transform;

					Vector3 from = player.position + _localSpawnPosition;
					Vector3 to = target.GetWorldPosition(x, y);

					Wait = true;

					Vector3 worldSpawnPosition = player.forward * _localSpawnPosition.z + player.right * _localSpawnPosition.x;

					Missile missile = Instantiate(_missilePrefab);
					missile.SetCallbacks(
						delegate () {
							if (destroyed)
								target.GetShip(x, y).transform.localRotation *= Quaternion.Euler(Vector3.right * 1000f);
						},
						delegate () {
							UIManager.SetTurn(CurrentPlayer.nickName);
						}
					);

					missile.Shoot(from, to, touched);

					UIManager.ShowShoot(CurrentPlayer.nickName, target.nickName);

					if (touched)
					{
						// Lancer le missile avec la variable destroyed pour indiquer s'il faut afficher le bateau coul� pendant l'animation
						target.ShipCellHit(x, y);
						if (destroyed)
							{

								int idShip = message.GetInt(5);
								int xShip = message.GetInt(6);
								int yShip = message.GetInt(7);
								int dirShip = message.GetInt(8);

								Transform shipT;
							if (!target.you)
							{
								Vector3 yRotation = new Vector3(0, -dirShip * 90, 0);
								shipT = Instantiate(InputManager.chipsButtons[idShip].transform.GetChild(0), target.GetWorldPosition(xShip, yShip),
									target.transform.rotation * Quaternion.Euler(yRotation), target.transform);
							}
							else
								shipT = target.GetShip(x, y).transform;
						missile.SetDestroyedShip(shipT);
						}
					}
					else
					{
						// Case devient rouge
						target.EmptyCellHit(x, y);
					}
					do
					{
						_currentTurn = (_currentTurn + 1) % _players.Length;
					} while (CurrentPlayer.dead);

					GameManager.CurrentState = GameManager.PlayerState.Playing;
						
					break;
				}

				case "Play":
				{
					_currentTurn = 0;

					InputManager.currentState = currentPlayer.id == current ? InputManager.PlayerState.Aiming : InputManager.PlayerState.Waiting;

					UIManager.SetTurn(CurrentPlayer.nickName);

					break;
				}

				case "Dead":
				{
					int id = message.GetInt(0);

					_players[id].dead = true;

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
		_messages.Enqueue(message);
	}

	private void OnDisconnect(object sender, string reason)
	{
		Disconnect();
	}

	public static void Disconnect()
	{
		if (_instance._server != null)
		{
			_instance._server.Disconnect();
			_instance._server = null;
		}

		UIManager.ShowMenu(UIManager.Menu.Connect);
	}

	private void OnDestroy()
	{
		if (_server != null)
			Disconnect();
	}

	#region Tools

	private Message CreateMessage<T>(string type, List<T> list, params object[] parameters)
	{
		return CreateMessage(type, list.ToArray(), parameters);
	}

	private Message CreateMessage<T>(string type, T[] list, params object[] parameters)
	{
		Message message = Message.Create(type);

		foreach (object parameter in parameters)
			message.Add(parameter);

		foreach (T item in list)
			message.Add(item);

		return message;
	}

	private T[] ExtractMessage<T>(Message message, uint startIndex = 0)
	{
		T[] items = new T[message.Count - startIndex];

		for (uint i = startIndex; i < message.Count; ++i)
			items[i - startIndex] = (T)message[i];

		return items;
	}

	#endregion
}
