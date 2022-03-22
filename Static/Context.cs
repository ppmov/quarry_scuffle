using System.Collections.Generic;
using UnityEngine;

namespace Context
{
    // стороны и расы
    public enum Side { Левые, Правые }
    public enum Filter { Союзное, Вражеское, Собственное }
    public enum PerformerType { Нечто, Здание, Создание }
    public enum Race { Случайно = '0', Крестоносцы = 'C', Сектанты = 'S' }

    public static class Sider
    {
        public static Side Invert(Side side) => (Side)Mathf.Abs((int)side - 1);
        public static Side Count(Side side, Filter filter) => filter == Filter.Вражеское ? Invert(side) : side;
        public static Side GetByOwner(byte owner) => owner % 2 == 0 ? Side.Левые : Side.Правые;
    }

    // сочетания типа брони и типа урона
    public enum DamageType { Тупой, Колющий, Магический, Осадный }
    public enum ArmorType { Естественная, Лёгкая, Тяжелая, Антимагия, Укрепленная }
    public enum Technique { Ближний, Дальний, Бросок, Телекинез }

    // интерфейс для чтения в UI
    public interface IObjectReader
    {
        public string Name { get; }
        public string Description { get; }
        public string Tooltip { get; }
    }

    // характеристика объекта
    public interface IPropertyReader
    {
        public float Initial { get; }
        public float Value { get; }
    }

    // объект игрока с информацией по нему
    public class Player
    {
        public static Player lDummy = new Player("", "", Side.Левые);
        public static Player rDummy = new Player("", "", Side.Правые);

        public string Id { get; private set; }
        public string Name { get; private set; }
        public Side Side { get; private set; }
        public Race Race { get; private set; }
        public bool Ready { get; private set; }
        public PlayerColor Color { get; private set; }
        public bool IsBot { get => Id.StartsWith("bot"); }
        public bool IsDummy { get => Id == ""; }

        public int Stock { get; set; }

        public Player(string id = "", string name = "", Side side = Side.Левые, int colorId = -1, Race race = Race.Случайно, bool ready = false)
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