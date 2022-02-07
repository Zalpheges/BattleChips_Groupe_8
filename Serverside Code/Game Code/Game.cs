using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System;

public class Player : BasePlayer
{

}

[RoomType("BattleChip")]
public class GameCode : Game<Player>
{
	public override void GameStarted()
	{
		//RoomId
		//AddTimer
		//Players
		//PlayerCount
		//Broadcast
	}

	public override void GameClosed()
	{

	}

	public override void UserJoined(Player player)
	{

	}

	public override void UserLeft(Player player)
	{

	}

	public override void GotMessage(Player player, Message message)
	{
		switch (message.Type)
		{
			case "Move":
				break;
		}
	}
}