using System.Collections.Generic;
using System.Collections;
using PlayerIOClient;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
		PlayerIO.Authenticate(
			"battlechips-tmwm0lz8memju96zesetw",
			"public",
			new Dictionary<string, string> {
				{ "userId", "Zalphug" },
			},
			null,
			delegate (Client client) {
				Debug.Log("Successfully connected to Player.IO");

				client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

				client.Multiplayer.CreateJoinRoom(
					"room_0",
					"BattleChip",
					true,
					null,
					null,
					delegate (Connection connection) {
						Debug.Log("Joined Room.");
					},
					delegate (PlayerIOError error) {
						Debug.Log("Error Joining Room: " + error.ToString());
					}
				);
			},
			delegate (PlayerIOError error) {
				Debug.Log("Error connecting: " + error.ToString());
			}
		);
	}
}
