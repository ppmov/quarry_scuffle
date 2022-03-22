using System;
using Context;

// имя объекта как разбираемый идентификатор
// tRA0.1[9..9], где: t - тип, R - раса, A - порядковый номер, 0 - уровень, 1 - владелец, 9..9 - уникальный ключ
public struct Naming
{
    public enum Variety { Building = 'b', Unit = 'u', Summoned = 's' }
    private enum HexId { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P }
    public static char Int2Hex(int index) => ((HexId)index).ToString()[0];

    // разбор структуры именования
    public Variety Type => (Variety)Text[0];
    public Race Race => (Race)Text[1];
    public char Id { get => Text[2]; }
    public int Grade { get => Text[3] - '0'; }
    public int Owner { get => Text[5] - '0'; }

    public byte Index
    {
        get
        {
            if (Text.Length >= 7)
                return (byte)int.Parse(Text.Substring(6));

            return byte.MaxValue;
        }
    }

    public string Trunc { get => Text.Substring(0, 4); }
    public Side Side { get => Players.GetPlayer((byte)Owner).Side; }
    public bool EndsWith(string end) => Text.EndsWith(end);

    private string Text { get; set; }

    // конструкторы
    public Naming(string input)
    {
        Text = input;
    }

    public Naming(Variety type, Race race, char index, int grade)
    {
        Text = (char)type + ((char)race).ToString() + index + grade;
    }

    public static string operator +(Naming v1, string v2) => v1.Text + v2;
    public static implicit operator string(Naming n) => n.Text;
    public static implicit operator Naming(string s) => new Naming(s);

    // проверка наследуемых по идентификатору
    public bool IsChild(string name)
    {
        Naming other = new Naming(name);
        if (Text == string.Empty) return false;
        if (name == string.Empty) return false;
        if (other.Race != Race) return false;
        if (other.Id != Id) return false;

        switch (Grade)
        {
            case 0:
                return other.Grade > 0 && other.Grade <= 3;
            case 1:
                return other.Grade > 3 && other.Grade <= 5;
            case 2:
                return other.Grade > 5 && other.Grade <= 7;
            case 3:
                return other.Grade == 8;
            case 4:
                return other.Grade == 9;
            default:
                return false;
        }
    }

    // поиск наследуемых по идентификатору
    public int CountChildGrade(int index)
    {
        switch (Grade)
        {
            case 0:
                return 1 + index;
            case 1:
                return 4 + index;
            case 2:
                return 6 + index;
            case 3:
                return 8;
            case 4:
                return 9;
            default:
                return 0;
        }
    }
}
