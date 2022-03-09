using System.Collections.Generic;
using PlayerIOClient;
using UnityEngine;
using System;

public class ClientManager : MonoBehaviour
{
	private static ClientManager _instance;

	private Queue<Message> _messages;

	private Connection _server;

	public static bool Wait { get; set; }

	private string[] _players;

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

					_players = ExtractMessage<string>(message, 2);

					GameManager.Board(id, count);

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
						
					int shipId = message.GetInt(5);
					int shipX = message.GetInt(6);
					int shipY = message.GetInt(7);
					int shipDir = message.GetInt(8);

					if (destroyed)
						GameManager.Shoot(id, x, y, shipId, shipX, shipY, shipDir);
					else
						GameManager.Shoot(id, x, y, touched);
						
					break;
				}

				case "Play":
				{
					GameManager.Play();

					break;
				}

				case "Dead":
				{
					int id = message.GetInt(0);

					GameManager.KillPlayer(id);

					break;
				}

				case "End":
				{
					int winner = message.GetInt(0);

					UIManager.ShowEnd(winner, _players);

					break;
				}
			}
		}
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
		_instance._server.Send("Boarded");
	}

	public static void Shoot(int id, int x, int y)
	{
		_instance._server.Send("Shoot", id, x, y);
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
