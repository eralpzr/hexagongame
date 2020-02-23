using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Hexagon.Scripts.Datas
{
    [Serializable, CreateAssetMenu(menuName = "Game Database Data", fileName = "New Game Database Data")]
    public class GameDatabaseData : ScriptableObject
    {
        public int Width;
        public int Height;
        public float FallSpeed;
        public int ScoreEveryExplode;
        public int BombTileMinMove;
        public int BombTileMaxMove;
        public int BombTileSpawnScore;
        public List<Color> HexagonColors;
    }
}
