using System.Collections.Generic;
using UnityEngine;

namespace Context
{
    // sides and fractions
    public enum Side { Left, Right }
    public enum Filter { Ally, Enemy, Own }
    public enum PerformerType { Anything, Building, Unit }
    public enum Race { Random = '0', Crusaders = 'C', Sectarians = 'S' }

    public static class Sider
    {
        public static Side Invert(Side side) => (Side)Mathf.Abs((int)side - 1);
        public static Side Count(Side side, Filter filter) => filter == Filter.Enemy ? Invert(side) : side;
        public static Side GetByOwner(byte owner) => owner % 2 == 0 ? Side.Left : Side.Right;
    }

    // armor and damage types combinations
    public enum DamageType { Normal, Pierce, Magic, Siege }
    public enum ArmorType { Natural, Light, Heavy, Antimagic, Fortified }
    public enum Technique { Melee, Ranged, Throwing, Telekinesis }

    // for UI reading
    public interface IObjectReader
    {
        public string Name { get; }
        public string Description { get; }
        public string Tooltip { get; }
    }

    // unit property
    public interface IPropertyReader
    {
        public float Initial { get; }
        public float Value { get; }
    }

    // player and his info
    public class Player
    {
        public static Player lDummy = new Player("", "", Side.Left);
        public static Player rDummy = new Player("", "", Side.Right);

        public string Id { get; private set; }
        public string Name { get; private set; }
        public Side Side { get; private set; }
        public Race Race { get; private set; }
        public bool Ready { get; private set; }
        public PlayerColor Color { get; private set; }
        public bool IsBot { get => Id.StartsWith("bot"); }
        public bool IsDummy { get => Id == ""; }

        public int Stock { get; set; }

        public Player(string id = "", string name = "", Side side = Side.Left, int colorId = -1, Race race = Race.Random, bool ready = false)
        {
            Id = id;
            Name = name;
            Side = side;
            Race = race;
            Ready = ready;

            if (colorId > -1)
                Color = Players.PlayerColors[colorId];
        }
    }

    public struct PlayerColor
    {
        public Material building;
        public Material units;
        public Material flags;

        public PlayerColor(Material buil, Material unit, Material flag)
        {
            building = buil;
            units = unit;
            flags = flag;
        }
    }
}