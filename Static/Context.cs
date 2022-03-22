using System.Collections.Generic;
using UnityEngine;

namespace Context
{
    // ������� � ����
    public enum Side { �����, ������ }
    public enum Filter { �������, ���������, ����������� }
    public enum PerformerType { �����, ������, �������� }
    public enum Race { �������� = '0', ����������� = 'C', �������� = 'S' }

    public static class Sider
    {
        public static Side Invert(Side side) => (Side)Mathf.Abs((int)side - 1);
        public static Side Count(Side side, Filter filter) => filter == Filter.��������� ? Invert(side) : side;
        public static Side GetByOwner(byte owner) => owner % 2 == 0 ? Side.����� : Side.������;
    }

    // ��������� ���� ����� � ���� �����
    public enum DamageType { �����, �������, ����������, ������� }
    public enum ArmorType { ������������, ˸����, �������, ���������, ����������� }
    public enum Technique { �������, �������, ������, ��������� }

    // ��������� ��� ������ � UI
    public interface IObjectReader
    {
        public string Name { get; }
        public string Description { get; }
        public string Tooltip { get; }
    }

    // �������������� �������
    public interface IPropertyReader
    {
        public float Initial { get; }
        public float Value { get; }
    }

    // ������ ������ � ����������� �� ����
    public class Player
    {
        public static Player lDummy = new Player("", "", Side.�����);
        public static Player rDummy = new Player("", "", Side.������);

        public string Id { get; private set; }
        public string Name { get; private set; }
        public Side Side { get; private set; }
        public Race Race { get; private set; }
        public bool Ready { get; private set; }
        public PlayerColor Color { get; private set; }
        public bool IsBot { get => Id.StartsWith("bot"); }
        public bool IsDummy { get => Id == ""; }

        public int Stock { get; set; }

        public Player(string id = "", string name = "", Side side = Side.�����, int colorId = -1, Race race = Race.��������, bool ready = false)
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