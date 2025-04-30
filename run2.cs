using System;
using System.Collections.Generic;
using System.Linq;

class run2
{
    static List<List<char>> GetInput()
    {
        var map = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
            map.Add([.. line.ToCharArray()]);
        return map;
    }

    static readonly int[] rowDirections = [-1, 1, 0, 0];
    static readonly int[] colDirections = [0, 0, -1, 1];

    // Структура состояния карты.
    // Структуры работают быстрее классов, так как увеличивают скорость доступа в некоторых типах данных,
    // например, для HashSet.
    readonly struct State((int row, int col)[] robotPositions, int keys) : IEquatable<State>
    {
        public readonly byte Robot1Row = (byte)robotPositions[0].row, Robot1Col = (byte)robotPositions[0].col, Robot2Row = (byte)robotPositions[1].row, Robot2Col = (byte)robotPositions[1].col, Robot3Row = (byte)robotPositions[2].row, Robot3Col = (byte)robotPositions[2].col, Robot4Row = (byte)robotPositions[3].row, Robot4Col = (byte)robotPositions[3].col;
        // Битовая маска для отслеживания собранных ключей,
        // увеличение эффективности по памяти и скорости.
        public readonly int KeysMask = keys;  

        // Получаем координаты робота по индексу.
        public (int row, int col) GetRobot(int robotIndex) => robotIndex switch
        {
            0 => (Robot1Row, Robot1Col),
            1 => (Robot2Row, Robot2Col),
            2 => (Robot3Row, Robot3Col),
            _ => (Robot4Row, Robot4Col),
        };

        // Метод для создания нового состояния с перемещенным роботом.
        public State WithMovedRobot(int robotIndex, int newRow, int newCol)
        {
            return robotIndex switch
            {
                0 => new State(new (int, int)[] { (newRow, newCol), (Robot2Row, Robot2Col), (Robot3Row, Robot3Col), (Robot4Row, Robot4Col) }, KeysMask),
                1 => new State(new (int, int)[] { (Robot1Row, Robot1Col), (newRow, newCol), (Robot3Row, Robot3Col), (Robot4Row, Robot4Col) }, KeysMask),
                2 => new State(new (int, int)[] { (Robot1Row, Robot1Col), (Robot2Row, Robot2Col), (newRow, newCol), (Robot4Row, Robot4Col) }, KeysMask),
                _ => new State(new (int, int)[] { (Robot1Row, Robot1Col), (Robot2Row, Robot2Col), (Robot3Row, Robot3Col), (newRow, newCol) }, KeysMask),
            };
        }

        // Метод для создания нового состояния с обновленным набором собранных ключей.
        public State WithNewKeys(int newKeys) => new(new (int, int)[] { (Robot1Row, Robot1Col), (Robot2Row, Robot2Col), (Robot3Row, Robot3Col), (Robot4Row, Robot4Col) }, newKeys);

        // Переопределение метода Equals для проверки равенства состояний,
        // будет работать лучше, чем побитовое сравнение.
        public bool Equals(State other)
        {
            return Robot1Row == other.Robot1Row && Robot1Col == other.Robot1Col &&
                   Robot2Row == other.Robot2Row && Robot2Col == other.Robot2Col &&
                   Robot3Row == other.Robot3Row && Robot3Col == other.Robot3Col &&
                   Robot4Row == other.Robot4Row && Robot4Col == other.Robot4Col &&
                   KeysMask == other.KeysMask;
        }

        public override bool Equals(object obj) => obj is State other && Equals(other);

        // Переопределение метода GetHashCode для создания хеша состояния для ускорения.
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = KeysMask;
                hash = hash * 31 + (Robot1Row << 8) + Robot1Col;
                hash = hash * 31 + (Robot2Row << 8) + Robot2Col;
                hash = hash * 31 + (Robot3Row << 8) + Robot3Col;
                hash = hash * 31 + (Robot4Row << 8) + Robot4Col;
                return hash;
            }
        }
    }

    static int Solve(List<List<char>> map)
    {
        var rows = map.Count;
        var cols = map[0].Count;
        var allKeys = 0;
        var robots = new List<(int row, int col)>();

        // Поиск позиций роботов и всех ключей на карте.
        for (var row = 0; row < rows; ++row)
            for (var col = 0; col < cols; ++col)
            {
                var cell = map[row][col];
                if (cell == '@') robots.Add((row, col));  // Стартовые позиции роботов
                else if (char.IsLower(cell)) allKeys |= 1 << (cell - 'a');  // Добавляем ключи в битовую маску
            }

        // Начальное состояние с позициями роботов и пустой маской ключей
        var initialState = new State([.. robots], 0);
        var queue = new Queue<(State state, int steps)>();
        var visited = new HashSet<State>();

        queue.Enqueue((initialState, 0));  // Добавляем начальное состояние в очередь
        visited.Add(initialState);  // Отмечаем начальное состояние как посещенное

        // BFS для поиска минимального пути.
        while (queue.Count > 0)
        {
            var (currentState, steps) = queue.Dequeue();

            // Если все ключи собраны, возвращаем количество шагов.
            if (currentState.KeysMask == allKeys)
                return steps;

            // Для каждого робота пытаемся сделать шаг в 4 направления.
            for (var robotIndex = 0; robotIndex < 4; ++robotIndex)
            {
                var (currentRow, currentCol) = currentState.GetRobot(robotIndex);

                for (var direction = 0; direction < 4; ++direction)
                {
                    var newRow = currentRow + rowDirections[direction];
                    var newCol = currentCol + colDirections[direction];

                    // Пропускаем, если вышли за пределы карты.
                    if (newRow < 0 || newCol < 0 || newRow >= rows || newCol >= cols) continue;

                    var cell = map[newRow][newCol];

                    // Пропускаем стены.
                    if (cell == '#') continue;

                    // Если встречаем дверь и у нас нет ключа, пропускаем.
                    if (char.IsUpper(cell) && (currentState.KeysMask & (1 << (cell - 'A'))) == 0) continue;

                    var newKeys = currentState.KeysMask;
                    if (char.IsLower(cell))  // Если нашли ключ, добавляем его в маску
                        newKeys |= (1 << (cell - 'a'));

                    var newState = currentState.WithMovedRobot(robotIndex, newRow, newCol).WithNewKeys(newKeys);
                    if (visited.Add(newState))  // Если новое состояние еще не посещено
                        queue.Enqueue((newState, steps + 1));  // Добавляем его в очередь
                }
            }
        }

        // Если не удалось собрать все ключи, возвращаем -1.
        return -1;
    }

    static void Main()
    {
        var data = GetInput();

        var result = Solve(data);

        Console.WriteLine(result == -1 ? "No solution found" : result);
    }
}
