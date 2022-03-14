using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System;

public class Ship
{
    public readonly int id;

    public readonly int x;
    public readonly int y;
    public readonly int dir;

    private bool[] state;
    public int length => state.Length;

    public bool destroyed => Array.TrueForAll(state, s => s);

    public Ship(int id, int x, int y, int dir, int length)
    {
        this.id = id;

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
        return (this.y == y && IsHorizontallyDirectedTo(x) && state.Length > Math.Abs(x - this.x)) ||
            (this.x == x && IsVerticallyDirectedTo(y) && state.Length > Math.Abs(y - this.y));
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
                y += other.dir == 1 ? -1 : 1;
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
            (dir == 1 && this.y > y) ||
            (dir == 3 && this.y < y);
    }
}

public class Player : BasePlayer
{
    public bool IsReady = false;
    public int Index;

    private List<Ship> ships = new List<Ship>();

    public void debug()
    {
        foreach (Ship ship in ships)
            Console.WriteLine(ship.x + " " + ship.y);
    }

    public bool IsDead()
    {
        return ships.TrueForAll(s => s.destroyed);
    }

    public Ship Shoot(int x, int y)
    {
        Ship ship = ships.Find(s => s.Intersect(x, y));

        if (ship != null)
        {
            ship.Shoot(x, y);

            return ship;
        }

        return null;
    }

    public bool AddShip(int id, int x, int y, int dir, int length)
    {
        Ship ship = new Ship(id, x, y, dir, length);

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

        int count = players.FindAll(p => p.IsReady).Count;

        Broadcast("Count", count, players.Count);
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
                            players[i].Send(CreateMessage("Board", players.ConvertAll(p => p.ConnectUserId), i, players.Count));

                            players[i].IsReady = false;
                        }
                    }

                    break;
                }

            case "Add":
                {
                    int id = message.GetInt(0);
                    int x = message.GetInt(1);
                    int y = message.GetInt(2);
                    int dir = message.GetInt(3);
                    int length = message.GetInt(4);

                    Console.WriteLine("{0} {1} {2} {3} {4} {5}", players.IndexOf(sender), sender.AddShip(id, x, y, dir, length), x, y, dir, length);

                    break;
                }

            case "Remove":
                {
                    int x = message.GetInt(0);
                    int y = message.GetInt(1);

                    Console.WriteLine("{0} {1} {2} {3}", players.IndexOf(sender), sender.RemoveShip(x, y), x, y);

                    break;
                }

            case "Boarded":
                {
                    sender.IsReady = true;

                    int count = players.FindAll(player => player.IsReady).Count;

                    Broadcast("Count", count, players.Count);

                    if (players.Count > 1 && count == players.Count)
                    {
                        Broadcast(CreateMessage("Play", players.ConvertAll(p => p.ConnectUserId)));

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

                            Ship ship = players[id].Shoot(x, y);
                            bool touched = ship != null;

                            if (touched && ship.destroyed)
                                Broadcast("Shoot", id, x, y, true, true, ship.id, ship.x, ship.y, ship.dir);
                            else
                                Broadcast("Shoot", id, x, y, touched, false);

                            if (players[id].IsDead())
                                Broadcast("Dead", id);

                            do
                            {
                                current = (current + 1) % players.Count;
                            }
                            while (players[current].IsDead());

                            int count = 0, winner = -1;
                            foreach (Player player in players)
                            {
                                if (!player.IsDead())
                                {
                                    ++count;
                                    winner = player.Index;
                                }
                                else
                                {
                                    Console.WriteLine(player.ConnectUserId);
                                    player.debug();
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
}