using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Hexagon.Scripts
{
    public class BombTileScript : TileScript
    {
        public TMPro.TMP_Text remainingMoveText;

        private int _remainingMove;

        protected override void Start()
        {
            base.Start();

            RemainingMove = Random.Range(gameDatabaseData.BombTileMinMove, gameDatabaseData.BombTileMaxMove + 1);
        }

        public override void OnEveryMove()
        {
            base.OnEveryMove();
            RemainingMove--;
        }

        public int RemainingMove
        {
            get => _remainingMove;
            set
            {
                _remainingMove = value;
                CreateParticle();

                if (remainingMoveText != null)
                    remainingMoveText.text = _remainingMove.ToString();

                if (_remainingMove <= 0)
                {
                    levelController.GameOver("BOMB EXPLODED");
                }
            }
        }

        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;
                remainingMoveText.color = value;
            }
        }
    }
}
