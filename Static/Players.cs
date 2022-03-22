using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Context;
using ExitGames.Client.Photon;

// Список игроков и взаимодействие с ним
public static class Players
{
    public static bool IsGameStarted { get; private set; }
    public static int MaxPlayersCount { get => list.Count; }
    public static List<PlayerColor> PlayerColors { get; private set; } = new List<PlayerColor>();
    
    private static List<Player> list = new List<Player>();

    static Players()
    {
        Material[] buils = Resources.LoadAll<Material>("Materials/Buildings");
        Material[] units = Resources.LoadAll<Material>("Materials/Units");
        Material[] flags = Resources.LoadAll<Material>("Materials/Flags");

        for (int i = 0; i < buils.Length; i++)
            PlayerColors.Add(new PlayerColor(buils[i], units[i], flags[i]));
    }

    // текущий игрок
    public static Player Myself
    {
        get
        {
            foreach (Player p in list)
                if (p.Id == PhotonNetwork.LocalPlayer.UserId)
                    return p;

            return null;
        }
    }

    // получение игрока по порядковому номеру
    public static Player GetPlayer(byte index)
    {
        if (index < 0 || index > MaxPlayersCount - 1)
            return null;

        return list[index];
    }

    // первый свободный слот
    public static byte GetFreeSlot()
    {
        for (byte i = 0; i < list.Count; i++)
            if (list[i].IsDummy)
                return i;

        return byte.MaxValue;
    }

    // обновление полей по конкретному слоту
    public static Player UpdateSlotLocal(byte index, string id, string name, Race race = Race.Случайно, bool ready = false)
    {
        if (index >= MaxPlayersCount)
            return null;

        list[index] = new Player(id, name, index % 2 == 0 ? Side.Левые : Side.Правые, index, race, ready);

        if (!list[index].IsDummy)
            for (byte i = 0; i < list.Count; i++)
                if (i != index && list[i].Id == id)
                    ClearSlotLocal(i);

        return list[index];
    }

    public static void UpdateBot(byte index, Race race = Race.Случайно)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        UpdateSlotLocal(index, "bot-" + index, "ИИ" + index, race, true);
        SetMyProperty("bot-" + index, (int)(char)race);
    }

    public static List<Player> GetBotPlayers()
    {
        List<Player> bots = new List<Player>();

        for (byte i = 0; i < list.Count; i++)
            if (list[i].IsBot)
                bots.Add(list[i]);

        return bots;
    }

    // увеличение ресурсов для всех игроков
    public static void IncomeAllPlayers(int value)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (value == 0) return;

        for (byte i = 0; i < list.Count; i++)
        {
            if (list[i].IsDummy)
                continue;

            int stock = 0;

            // найдем игрока и добавим ему нужное значение в общий контейнер
            foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
                if (player.UserId == list[i].Id)
                {
                    Hashtable props = player.CustomProperties;

                    if (props.TryGetValue("stock", out object obj))
                        stock = (int)obj;

                    stock += value;
                    props["stock"] = stock;
                    player.SetCustomProperties(props);
                    break;
                }

            // логика для ИИ
            if (stock == 0)
                stock = list[i].Stock + value;

            // локальные данные
            list[i].Stock = stock;
        }
    }

    // трата ресурсов
    public static bool SpendMyStock(int value)
    {
        Hashtable props = PhotonNetwork.LocalPlayer.CustomProperties;
        int stock = Myself.Stock;

        if (props.TryGetValue("stock", out object obj))
            stock = (int)obj;

        if (stock - value < 0)
            return false;

        props["stock"] = stock - value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        return true;
    }

    // очистка игрового слота
    public static Player ClearSlotLocal(byte index)
    {
        return UpdateSlotLocal(index, "", "");
    }

    // поиск игрока по идентификатору
    public static byte IndexOfPlayer(string id)
    {
        for (byte i = 0; i < list.Count; i++)
            if (list[i].Id == id)
                return i;

        return byte.MaxValue;
    }

    // все готовы?
    public static bool IsEveryoneReady()
    {
        foreach (Player player in list)
            if (!player.Ready && !player.IsDummy)
                return false;

        return true;
    }

    // исключение
    public static void KickPlayer(int index)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (GetPlayer((byte)index).IsBot)
        {
            Hashtable props = PhotonNetwork.LocalPlayer.CustomProperties;
            props.Remove("bot-" + index);
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            ReloadPreparingCustomProperties();
        }

        foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
            if (player.UserId == GetPlayer((byte)index).Id)
            {
                PhotonNetwork.CloseConnection(player);
                break;
            }
    }

    // обновление подготовительных данных по игрокам
    public static void ReloadPreparingCustomProperties(bool lastTime = false)
    {
        if (PhotonNetwork.CurrentRoom == null) 
            return;

        if (IsGameStarted)
            return;

        if (lastTime)
            IsGameStarted = true;

        for (byte i = 0; i < MaxPlayersCount; i++)
        {
            string id = string.Empty;
            string name = string.Empty;
            Race race = Race.Случайно;
            bool ready = false;

            foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
                if (player.CustomProperties.TryGetValue("pos", out object obj) && (byte)obj == i)
                {
                    id = player.UserId;
                    name = player.NickName;

                    if (player.CustomProperties.TryGetValue("race", out obj))
                        race = (Race)(char)(int)obj;

                    if (player.CustomProperties.TryGetValue("ready", out obj))
                        ready = (bool)obj;
                }

            if (id == string.Empty)
                if (PhotonNetwork.MasterClient.CustomProperties.TryGetValue("bot-" + i, out object value))
                {
                    id = "bot-" + i;
                    name = "ИИ" + i;
                    race = (Race)(char)(int)value;
                    ready = true;
                }

            UpdateSlotLocal(i, id, name, race, ready);
        }
    }

    // обновление игровых данных по игрокам
    public static void ReloadRealtimeCustomProperties()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
            if (player.CustomProperties.TryGetValue("pos", out object obj) && (byte)obj < MaxPlayersCount)
            {
                byte pos = (byte)obj;
                int stock = list[pos].Stock;

                if (player.CustomProperties.TryGetValue("stock", out obj))
                    stock = (int)obj;

                list[pos].Stock = stock;
            }
    }

    // заполнение параметра
    public static void SetMyProperty(string property, object value)
    {
        Hashtable props = PhotonNetwork.LocalPlayer.CustomProperties;
        props[property] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        ReloadPreparingCustomProperties();
    }

    // случайный выбор расы для игроков, не выбравших ее
    public static void RandomizePlayersRaces()
    {
        if (!PhotonNetwork.IsMasterClient) 
            return;

        ReloadPreparingCustomProperties();

        for (byte i = 0; i < list.Count; i++)
        {
            if (list[i].IsBot)
                UpdateBot(i, GetRandomRace());
            else
            {
                foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
                    if (list[i].Id == player.UserId)
                        if (list[i].Race == Race.Случайно)
                        {
                            Hashtable props = player.CustomProperties;
                            props["race"] = (int)(char)GetRandomRace();
                            player.SetCustomProperties(props);
                        }
            }
        }

        ReloadPreparingCustomProperties();
    }

    private static Race GetRandomRace()
    {
        System.Array array = System.Enum.GetValues(typeof(Race));
        return (Race)array.GetValue(Random.Range(1, array.Length));
    }

    // сброс списка игроков
    public static void Revert()
    {
        list = new List<Player>
        {
            Player.lDummy,
            Player.rDummy,
            Player.lDummy,
            Player.rDummy
        };

        SetMyProperty("pos", byte.MaxValue);
        SetMyProperty("race", (int)(char)Race.Случайно);
        SetMyProperty("ready", false);
        SetMyProperty("stock", 0);
        IsGameStarted = false;
    }
}
