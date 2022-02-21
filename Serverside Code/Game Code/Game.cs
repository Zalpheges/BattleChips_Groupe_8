using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System;

public class Ship
{
	int x;
	int y;
	int dir;

	bool[] state;

	public Ship(int x, int y, int dir, int length)
    {
		this.x = x;
		this.y = y;
		this.dir = dir;

		state = new bool[length];
		for (int i = 0; i < length; ++i)
			state[i] = true;
	}

	public bool Intersect(Ship other)
	{
		int lx = Math.Abs(other.x - x);
		int ly = Math.Abs(other.y - y);

		return (((IsHorizontallyDirectedTo(other.x) && state.Length <= lx) ||
			(other.IsHorizontallyDirectedTo(x) && other.state.Length <= lx)) &&
			((IsVerticallyDirectedTo(other.y) && state.Length <= ly) ||
			(other.IsVerticallyDirectedTo(y) && other.state.Length <= ly)));
	}

	public bool IsHorizontallyDirectedTo(int x)
    {
		return this.x == x ||
			(dir == 0 && this.x < x) ||
			(dir == 2 && this.x > x);
	}

	public bool IsVerticallyDirectedTo(int y)
	{
		return this.y == y ||
			(dir == 1 && this.y < y) ||
			(dir == 3 && this.y > y);
	}
}

public class Player : BasePlayer
{
	List<Ship> ships = new List<Ship>();

	public bool AddShip(int x, int y, int dir, int length)
	{
		return false;
    }
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

		Console.WriteLine(new Ship(0, 0, 0, 4).Intersect(new Ship(2, 2, 3, 3)));
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