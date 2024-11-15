using System;
using System.Collections.Generic;

public class GoGame
{
    // Matriz 9x9 para representar el tablero del juego
    // 0: vacío, 1: piedra negra, 2: piedra blanca
    private int[,] board = new int[9, 9];

    // Variables para el puntaje: cada jugador gana puntos capturando piezas o controlando territorio
    private int blackScore = 0;
    private int whiteScore = 0;
    private bool gameEnded = false; // Indicador de final del juego cuando ambos jugadores deciden pasar

    // Constructor: Inicializa el tablero
    public GoGame()
    {
        InitializeBoard();
    }

    // Inicializa el tablero estableciendo todas las posiciones en 0 (vacío)
    private void InitializeBoard()
    {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
                board[i, j] = 0; // Todos los espacios se marcan como vacíos
    }

    // Muestra el tablero en la consola. Cada posición se representa con:
    // 'B' para piedra negra, 'W' para piedra blanca y '.' para espacios vacíos.
    private void PrintBoard()
    {
        Console.WriteLine("  0 1 2 3 4 5 6 7 8"); // Números de columna
        for (int i = 0; i < 9; i++)
        {
            Console.Write(i + " "); // Número de fila
            for (int j = 0; j < 9; j++)
            {
                char symbol = board[i, j] == 1 ? 'B' : board[i, j] == 2 ? 'W' : '.';
                Console.Write(symbol + " ");
            }
            Console.WriteLine();
        }
    }

    // Coloca una piedra en el tablero si el espacio está vacío y verifica posibles capturas
    public bool PlaceStone(int x, int y, int player)
    {
        if (board[x, y] != 0) // Si la posición está ocupada
        {
            Console.WriteLine("Posición ocupada, elige otra.");
            return false; // No se puede colocar una piedra aquí
        }

        board[x, y] = player; // Coloca la piedra del jugador (1 para negro, 2 para blanco)

        // Verifica capturas en las posiciones adyacentes
        CaptureGroups(x, y, player == 1 ? 2 : 1);

        PrintBoard(); // Imprime el tablero actualizado
        return true; // Piedra colocada correctamente
    }

    // Captura grupos de piedras enemigas adyacentes si no tienen libertades
    private void CaptureGroups(int x, int y, int opponent)
    {
        // Direcciones posibles: derecha, abajo, izquierda, arriba
        var directions = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
        foreach (var (dx, dy) in directions)
        {
            int nx = x + dx, ny = y + dy; // Coordenadas adyacentes
            // Si la posición adyacente contiene una piedra del oponente
            if (IsInBounds(nx, ny) && board[nx, ny] == opponent)
            {
                var group = GetGroup(nx, ny); // Obtiene el grupo de piedras conectadas
                if (!HasLiberty(group)) // Si el grupo no tiene libertades
                {
                    foreach (var (gx, gy) in group)
                    {
                        board[gx, gy] = 0; // Elimina las piedras capturadas del tablero
                        if (opponent == 1) whiteScore++; // Si es una piedra negra, blanco gana puntos
                        else blackScore++; // Si es una piedra blanca, negro gana puntos
                    }
                }
            }
        }
    }

    // Determina si un grupo de piedras tiene al menos una libertad
    private bool HasLiberty(List<(int, int)> group)
    {
        foreach (var (x, y) in group)
        {
            // Direcciones para verificar libertades en los alrededores
            var directions = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            foreach (var (dx, dy) in directions)
            {
                int nx = x + dx, ny = y + dy; // Coordenadas adyacentes
                if (IsInBounds(nx, ny) && board[nx, ny] == 0) // Si hay una libertad
                    return true;
            }
        }
        return false; // Sin libertades
    }

    // Obtiene un grupo de piedras conectadas del mismo color
    private List<(int, int)> GetGroup(int x, int y)
    {
        int color = board[x, y]; // Color de la piedra inicial
        var group = new List<(int, int)>();
        var toVisit = new Queue<(int, int)>();
        var visited = new HashSet<(int, int)>();
        toVisit.Enqueue((x, y));

        while (toVisit.Count > 0)
        {
            var (cx, cy) = toVisit.Dequeue();
            // Si está dentro de los límites, es del color correcto y no ha sido visitado
            if (!IsInBounds(cx, cy) || board[cx, cy] != color || visited.Contains((cx, cy)))
                continue;

            group.Add((cx, cy)); // Añadir al grupo
            visited.Add((cx, cy)); // Marcar como visitado

            // Añadir las posiciones adyacentes a la cola para explorarlas
            toVisit.Enqueue((cx + 1, cy));
            toVisit.Enqueue((cx - 1, cy));
            toVisit.Enqueue((cx, cy + 1));
            toVisit.Enqueue((cx, cy - 1));
        }

        return group; // Retorna el grupo completo
    }

    // Verifica si una posición está dentro de los límites del tablero
    private bool IsInBounds(int x, int y) => x >= 0 && x < 9 && y >= 0 && y < 9;

    // Calcula el puntaje final considerando capturas y áreas controladas
    public void CalculateScore()
    {
        int blackControlled = 0, whiteControlled = 0;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (board[i, j] == 0) // Para cada espacio vacío
                {
                    var emptyGroup = GetGroup(i, j); // Obtiene un grupo de posiciones vacías
                    bool blackAdjacent = false, whiteAdjacent = false;

                    foreach (var (x, y) in emptyGroup)
                    {
                        // Verifica las piedras adyacentes para ver si están controladas por negro o blanco
                        var directions = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
                        foreach (var (dx, dy) in directions)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (IsInBounds(nx, ny))
                            {
                                if (board[nx, ny] == 1) blackAdjacent = true;
                                if (board[nx, ny] == 2) whiteAdjacent = true;
                            }
                        }
                    }

                    if (blackAdjacent && !whiteAdjacent) blackControlled += emptyGroup.Count;
                    if (whiteAdjacent && !blackAdjacent) whiteControlled += emptyGroup.Count;
                }
            }
        }

        // Agrega los puntos de control al puntaje final
        blackScore += blackControlled;
        whiteScore += whiteControlled;

        Console.WriteLine($"Puntuación Final: Negro = {blackScore}, Blanco = {whiteScore}");
        Console.WriteLine(blackScore > whiteScore ? "Gana el Jugador Negro" : "Gana el Jugador Blanco");
    }

    // Maneja el flujo del juego, alternando turnos entre los jugadores
    public void PlayGame()
    {
        int player = 1; // 1  Negro, 2  Blanco

        while (!gameEnded) // Bucle que define si se sigue jugando o no 
        {
            Console.WriteLine($"Turno del Jugador {(player == 1 ? "Negro" : "Blanco")}");
            PrintBoard(); //  tablero

            // Jugada
            Console.Write("Ingrese la posición x: ");
            int x = int.Parse(Console.ReadLine());
            Console.Write("Ingrese la posición y: ");
            int y = int.Parse(Console.ReadLine());

            if (PlaceStone(x, y, player)) // Coloca la piedra y alterna el turno
                player = player == 1 ? 2 : 1;

            // Pregunta si ambos jugadores desean pasar
            Console.WriteLine("¿Desean pasar ambos jugadores? (s/n): ");
            gameEnded = Console.ReadLine()?.ToLower() == "s";
        }

        CalculateScore(); //  puntaje final
    }
}

public class Program
{
    public static void Main()
    {
        GoGame game = new GoGame();
        game.PlayGame();
    }
}
