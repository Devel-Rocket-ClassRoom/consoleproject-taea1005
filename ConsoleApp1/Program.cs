using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1
{
    public class Map
    {
        public static void PrintMap(char[,] map)
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j]);
                }
                Console.WriteLine();
            }
        }

        public char[,] CreateMap(int row, int col)
        {
            Random random = new Random();
            int randomCol = random.Next(1, col - 1);
            int randomRow = random.Next(1, row - 1);
            int randomCount = random.Next(1, 5);
            int randomwWallCol = random.Next(1, col - 3);
            int randomwWallRow = random.Next(1, row - 2);

            char[,] map = new char[col, row];
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    if (i == 0 || j == 0 || i == col - 1 || j == row - 1)
                        map[i, j] = '#';
                    else
                        map[i, j] = ' ';
                }
            }

            if (randomCount % 2 == 0)
            {
                map[randomwWallCol, randomwWallRow] = '#';
                map[randomwWallCol + 1, randomwWallRow] = '#';
                map[randomwWallCol + 2, randomwWallRow] = '#';
            }
            else
            {
                map[randomwWallCol, randomwWallRow] = '#';
                map[randomwWallCol, randomwWallRow + 1] = '#';
                map[randomwWallCol, randomwWallRow + 2] = '#';
            }

            while (true)
            {
                if (map[randomCol, randomRow] != '#')
                {
                    map[randomCol, randomRow] = 'P';
                    break;
                }
                else
                {
                    randomCol = random.Next(1, col - 1);
                    randomRow = random.Next(1, row - 1);
                }
            }
            return map;
        }
    }

    public class Player
    {
        public int PlayerHealth = 20;
        public int ControlMap(char[,] map, ref int Count, int stage, int totalStages, MonsterManager mm)
        {
            int totalTurn = 0;
            int NowPlayerCol = 0;
            int NowPlayerRow = 0;

            while (true)
            {
                if (Count <= 0 && map[map.GetLength(0) - 2, map.GetLength(1) - 1] == '#')
                {
                    map[map.GetLength(0) - 2, map.GetLength(1) - 1] = 'D';
                }

                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        if (map[i, j] == 'P')
                        {
                            NowPlayerCol = i;
                            NowPlayerRow = j;
                        }
                    }
                }

                int NextPlayerCol = NowPlayerCol;
                int NextPlayerRow = NowPlayerRow;

                Map.PrintMap(map);
                Console.WriteLine($"남은 적: {Count} | 스테이지: {stage}/{totalStages} | 체력: {PlayerHealth}");
                Console.WriteLine("이동: W,A,S,D (종료:E)");

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                char move = char.ToLower(keyInfo.KeyChar);
                if (move == 'e') return 0;

                if (move == 'w') NextPlayerCol--;
                else if (move == 'a') NextPlayerRow--;
                else if (move == 's') NextPlayerCol++;
                else if (move == 'd') NextPlayerRow++;

                char target = map[NextPlayerCol, NextPlayerRow];

                if (target == 'D') return 1;
                if (target == 'L') return -1;

                if (target == 'M')
                {
                    if (mm.CheckMonsterHit(NextPlayerCol, NextPlayerRow))
                    {
                        map[NextPlayerCol, NextPlayerRow] = 'P';
                        map[NowPlayerCol, NowPlayerRow] = ' ';
                        Count--;
                        totalTurn++;
                    }
                }
                else if (target == ' ')
                {
                    map[NextPlayerCol, NextPlayerRow] = 'P';
                    map[NowPlayerCol, NowPlayerRow] = ' ';
                    totalTurn++;
                }
                else if (target == '#')
                {
                    Console.WriteLine("이동이 불가합니다.");
                    Thread.Sleep(300);
                    continue;
                }

                if (totalTurn > 0 && totalTurn % 2 == 0)
                {
                    mm.MonsterMove(map, this, NextPlayerCol, NextPlayerRow);
                    if (this.PlayerHealth <= 0) return 0;
                }
                Console.Clear();
            }
        }
        public void TakeDamege(int damage)
        {
            PlayerHealth -= damage;
            Console.WriteLine(PlayerHealth);
        }

    }

    public class Monster
    {
        public int X;
        public int Y;
        public bool IsAlive;

        public Monster(int x, int y)
        {
            X = x;
            Y = y;
            IsAlive = true;
        }
    }

    public class MonsterManager
    {
        private List<Monster> monsters = new List<Monster>();

        public int Spawn(int row, int col, char[,] map)
        {
            monsters.Clear();
            Random random = new Random();
            int count = random.Next(1, 5);

            for (int i = 0; i < count; i++)
            {
                while (true)
                {
                    int rCol = random.Next(1, col - 1);
                    int rRow = random.Next(1, row - 1);
                    if (map[rCol, rRow] != 'P' && map[rCol, rRow] != '#' && map[rCol, rRow] != 'M')
                    {
                        map[rCol, rRow] = 'M';
                        monsters.Add(new Monster(rCol, rRow));
                        break;
                    }
                }
            }
            return count;
        }

        public bool CheckMonsterHit(int col, int row)
        {
            foreach (var m in monsters)
            {
                if (m.IsAlive && m.X == col && m.Y == row)
                {
                    m.IsAlive = false;
                    return true;
                }
            }
            return false;
        }

        public void MonsterMove(char[,] map, Player player, int playerCol, int playerRow)
        {
            for (int i = 0; i < monsters.Count; i++)
            {
                var m = monsters[i];
                if (!m.IsAlive) continue;

                map[m.X, m.Y] = ' ';

                int nextCol = m.X;
                int nextRow = m.Y;
                int diffCol = Math.Abs(playerCol - m.X);
                int diffRow = Math.Abs(playerRow - m.Y);

                if (diffCol > diffRow)
                {
                    if (nextCol < playerCol) nextCol++;
                    else if (nextCol > playerCol) nextCol--;
                }
                else
                {
                    if (nextRow < playerRow) nextRow++;
                    else if (nextRow > playerRow) nextRow--;
                }

                if (map[nextCol, nextRow] == ' ')
                {
                    m.X = nextCol;
                    m.Y = nextRow;
                }
                else if (nextCol == playerCol && nextRow == playerRow)
                {
                    Console.WriteLine($"{i}번 몬스터의 근접 공격!");
                    player.TakeDamege(2);
                    Thread.Sleep(500);
                }

                map[m.X, m.Y] = 'M';
            }
        }
    }

    public class Dungeon
    {
        private List<char[,]> stageMaps = new List<char[,]>();
        private List<MonsterManager> stageManagers = new List<MonsterManager>();
        private List<int> stageMonsterCounts = new List<int>();
        private Player player = new Player();
        private Map map1 = new Map();

        public void GamePlaying(int row, int col, int totalStages)
        {
            int currentIdx = 0; 

            while (currentIdx >= 0 && currentIdx < totalStages)
            {
                if (stageMaps.Count <= currentIdx)
                {
                    char[,] Map1 = map1.CreateMap(row, col);
                    MonsterManager mm = new MonsterManager();
                    int mCount = mm.Spawn(row, col, Map1);

                    if (currentIdx > 0)
                    {
                        Map1[Map1.GetLength(0) - 2, 0] = 'L';
                    }

                    stageMaps.Add(Map1);
                    stageManagers.Add(mm);
                    stageMonsterCounts.Add(mCount);
                }

                int monC = stageMonsterCounts[currentIdx];

                int result = player.ControlMap(stageMaps[currentIdx], ref monC, currentIdx + 1, totalStages, stageManagers[currentIdx]);

                stageMonsterCounts[currentIdx] = monC; 

                if (result == 1) currentIdx++;  
                else if (result == -1) currentIdx--; 
                else break;                          

                Console.Clear();
            }
            Console.WriteLine("게임이 종료되었습니다.");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            
            Console.Write("맵의 가로 길이: ");
            int row = int.Parse(Console.ReadLine());
            Console.Write("맵의 세로 길이: ");
            int col = int.Parse(Console.ReadLine());
            Console.Write("스테이지 갯수: ");
            int stage = int.Parse(Console.ReadLine());
            Console.Clear();
            Console.CursorVisible = true;
            
            Dungeon dungeon = new Dungeon();
            dungeon.GamePlaying(row, col, stage);
        }
    }
}