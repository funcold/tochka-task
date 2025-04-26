using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

partial class HotelCapacity
{
    static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        var events = new List<Event>();

        //Парсинг данных работает за O(n).
        foreach (var guest in guests)
        {
            var checkInDate = DateTime.Parse(guest.CheckIn);
            var checkOutDate = DateTime.Parse(guest.CheckOut);

            //Добавляем данные в список событий.
            //1 - въезд, -1 - выезд.
            events.Add(new Event(checkInDate, 1));
            events.Add(new Event(checkOutDate, -1));
        }

        //Сортируем данные, чтобы последовательно по ним пройти.
        //Сначала идут события с самой ранней датой.
        //Если дата одинаковая - сортируем по типу события, выезд раньше въезда,
        //так как по условию задачи въезд считается включительно, а выезд нет, и нужно обработать выезды раньше.
        //Сортировка работает за O(n*log(n)).
        events.Sort((a, b) =>
        {
            var dateCompare = a.Date.CompareTo(b.Date);
            if (dateCompare != 0)
                return dateCompare;
            return a.Type.CompareTo(b.Type);
        });

        var currentGuests = 0;
        var maxGuests = 0;

        //Последовательно проходим по всем событиям, всего 2n событий.
        //Если на какой-то итериции currentGuests станет больше maxCapacity, то возможности разместить всех гостей нет.
        //Сложность O(n).
        foreach (var e in events)
        {
            currentGuests += e.Type;
            if (currentGuests > maxGuests)
            {
                maxGuests = currentGuests;
                if (maxGuests > maxCapacity)
                    return false;
            }
        }

        //Итоговая сложность O(n) + O(n*log(n)) + O(n) = O(n*log(n)), как требуется в условии.
        return true;
    }

    class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }

    //Для сокращения кода можно переписать через Primary Constructors
    //class Event(DateTime date, int type)
    //Но для Mono 6.6 это может не работать
    class Event
    {
        public DateTime Date { get; }
        public int Type { get; }

        public Event(DateTime date, int type)
        {
            Date = date;
            Type = type;
        }
    }

    static void Main()
    {
        var maxCapacity = int.Parse(Console.ReadLine());

        var rawN = Console.ReadLine();

        //В формате ввода указано, что n идёт сразу после max_capacity,
        //Но в примерах между ними есть пустая строка.
        //Чтобы работало и так, и так, добавил проверку.
        if (rawN == "")
            rawN = Console.ReadLine();

        var n = int.Parse(rawN);

        var guests = new List<Guest>();

        for (var i = 0; i < n; ++i)
        {
            var line = Console.ReadLine();
            var guest = ParseGuest(line);
            guests.Add(guest);
        }

        var result = CheckCapacity(maxCapacity, guests);

        Console.WriteLine(result ? "True" : "False");
    }


    static Guest ParseGuest(string json)
    {
        var guest = new Guest();

        var nameMatch = NameRegex.Match(json);
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;

        var checkInMatch = CheckInRegex.Match(json);
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;

        var checkOutMatch = CheckOutRegex.Match(json);
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;

        return guest;
    }

    //Можно было использовать [GeneratedRegex], но это может не работать в Mono 6.6
    private static readonly Regex NameRegex = new("\"name\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
    private static readonly Regex CheckInRegex = new("\"check-in\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
    private static readonly Regex CheckOutRegex = new("\"check-out\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
}