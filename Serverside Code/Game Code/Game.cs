using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System;

public class Ship
{
    public readonly int x;
    public readonly int y;
    public readonly int dir;

    private bool[] state;
    public int length => state.Length;

    public bool destroyed => Array.TrueForAll(state, s => s);

    public Ship(int x, int y, int dir, int length)
    {
        this.x = x;
        this.y = y;
        this.dir = dir;

        state = new bool[length];
    }

    public bool Shoot(int x, int y)
    {
        if (Intersect(x, y))
        {
            int index = Math.Abs(x - this.x) + Math.Abs(y - this.y);

            state[index] = true;

            return destroyed;
        }

        return false;
    }

    public bool Intersect(int x, int y)
    {
        return (this.y == y && IsHorizontallyDirectedTo(x) && state.Length >= Math.Abs(x - this.x)) ||
            (this.x == x && IsVerticallyDirectedTo(y) && state.Length >= Math.Abs(y - this.y));
    }

    public bool Intersect(Ship other)
    {
        int x = other.x, y = other.y;
        for (int i = 0; i < other.length; ++i)
        {
            if (Intersect(x, y))
                return true;

            if (other.dir == 0 || other.dir == 2)
                x += other.dir == 0 ? 1 : -1;
            else
                y += other.dir == 1 ? 1 : -1;
        }

        return false;
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
	public bool IsReady = false;
    public int Index;

	private List<Ship> ships = new List<Ship>();

    public bool IsDead()
    {
        return ships.TrueForAll(s => s.destroyed);
    }

    public bool Shoot(int x, int y, out bool destroyed)
    {
        Ship ship = ships.Find(s => s.Intersect(x, y));

        if (ship != null)
        {
            destroyed = ship.Shoot(x, y);

            return true;
        }

        return destroyed = false;
    }

	public bool AddShip(int x, int y, int dir, int length)
	{
        Ship ship = new Ship(x, y, dir, length);

        bool intersect = ships.Find(s => s.Intersect(ship)) != null;

        if (intersect)
            return false;

        ships.Add(ship);

        return true;
    }

    public bool RemoveShip(int x, int y)
    {
        Ship ship = ships.Find(s => s.Intersect(x, y));

        if (ships != null)
            return ships.Remove(ship);

        return false;
    }
}

[RoomType("BattleChip")]
public class GameCode : Game<Player>
{
	private List<Player> players;

	private bool isRunning = false;

	int current;

	public override void GameStarted()
	{
		players = new List<Player>();
	}

    public override bool AllowUserJoin(Player player)
    {
		return !isRunning && players.Count <= 5;
    }

    public override void GameClosed()
	{

	}

	public override void UserJoined(Player player)
	{
		players.Add(player);
	}

	public override void UserLeft(Player player)
	{
		if (isRunning)
			players.ForEach(p => p.Disconnect());
	}

	public override void GotMessage(Player sender, Message message)
	{
		switch (message.Type)
		{
			case "Ready":
			{
				sender.IsReady = message.GetBoolean(0);

				int count = players.FindAll(player => player.IsReady).Count;

				Broadcast("Count", count, players.Count);

				if (players.Count > 1 && count == players.Count)
                {
					isRunning = true;

                    for (int i = 0; i < players.Count; ++i)
                    {
                        players[i].Index = i;
                        players[i].Send("Board", i);
                        players[i].IsReady = false;
                    }
                }

				break;
			}

            case "Add":
            {
                int x = message.GetInt(0);
                int y = message.GetInt(1);
                int dir = message.GetInt(2);
                int length = message.GetInt(3);

                sender.AddShip(x, y, dir, length);

                break;
            }

            case "Remove":
            {
                int x = message.GetInt(0);
                int y = message.GetInt(1);

                sender.RemoveShip(x, y);

                break;
            }

            case "Boarded":
            {
                sender.IsReady = true;

                int count = players.FindAll(player => player.IsReady).Count;

                Broadcast("Count", count, players.Count);

                if (players.Count > 1 && count == players.Count)
                {
                    Broadcast("Play");

                    current = 0;

                    foreach (Player player in players)
                        player.IsReady = false;
                }

                break;
            }

            case "Shoot":
            {
                if (current == sender.Index)
                {
                    int id = message.GetInt(0);

                    if (id >= 0 && id != sender.Index && id < players.Count)
                    {
                        int x = message.GetInt(1);
                        int y = message.GetInt(2);

                        bool touched = players[id].Shoot(x, y, out bool destroyed);

                        Broadcast("Shoot", id, x, y, touched, destroyed);

                        if (players[id].IsDead())
                            Broadcast("Dead", id);

                        ++current;

                        int count = 0, winner = -1;
                        foreach (Player player in players)
                        {
                            if (!player.IsDead())
                            {
                                ++count;
                                winner = player.Index;
                            }
                        }

                        if (count == 1)
                            Broadcast("End", winner);
                    }
                }

                break;
            }

            case "Count":
            {
                int count = players.FindAll(player => player.IsReady).Count;

                sender.Send("Count", count, players.Count);

                break;
            }
        }
	}
}