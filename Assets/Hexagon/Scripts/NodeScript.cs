using System.Linq;
using Assets.Hexagon.Scripts.Datas;
using Assets.Hexagon.Scripts.Models;
using UnityEngine;

namespace Assets.Hexagon.Scripts
{
    public sealed class NodeScript : MonoBehaviour
    {
        private GameDatabaseData _gameDatabaseData;
        private TileGenerator _tileGenerator;
        private bool _isSelected = false;

        private void Awake()
        {
            _tileGenerator = GameObject.FindGameObjectWithTag("LevelController").GetComponent<TileGenerator>();
            _gameDatabaseData = GameController.Instance.gameDatabaseData;
            GetConnectedSlots();
        }

        private void Start()
        {
            gameObject.SetActive(_isSelected);
        }

        private void Update()
        {
        
        }

        public void GetConnectedSlots()
        {
            if (_tileGenerator.Slots.Count == 0)
                return;

            ConnectedSlots = _tileGenerator.Slots.Where(x => (transform.position - x.position).sqrMagnitude < 1f)
                                           .ToArray();

            foreach (var connectedSlot in ConnectedSlots)
                connectedSlot.ConnectedNodes.Add(this);
        }

        public void TurnClockwise()
        {
            if (ConnectedSlots.Length == 0)
                return;

            var list = ConnectedSlots.ToList();
            GameObject temp = list.Last().tileObject;
            foreach (var tile in list)
            {
                var nextTemp = tile.tileObject;
                tile.tileObject = temp;

                temp = nextTemp;
            }
        }

        public void TurnCounterClockwise()
        {
            if (ConnectedSlots.Length == 0)
                return;

            var list = ConnectedSlots.Reverse().ToList();
            GameObject temp = list.Last().tileObject;
            foreach (var tile in list)
            {
                var nextTemp = tile.tileObject;
                tile.tileObject = temp;

                temp = nextTemp;
            }
        }

        public bool CheckSlotsSameColors()
        {
            if (ConnectedSlots == null || ConnectedSlots.Length == 0 || ConnectedSlots.Any(x => x.tileObject == null))
                return false;
            
            return ConnectedSlots.Select(x => x.tileObject.GetComponent<TileScript>().Color).Distinct().Count() == 1;
        }

        public void DestroyTiles()
        {
            foreach (var slot in ConnectedSlots)
            {
                if (slot.tileObject != null)
                    slot.tileObject.GetComponent<TileScript>().OnTileDestroy();

                slot.DestroyTile();
            }
        }

        public Slot[] ConnectedSlots { get; private set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                if (value)
                {
                    var previousNode = _tileGenerator.Nodes.FirstOrDefault(x => x.IsSelected);
                    if (previousNode != null)
                    {
                        foreach (var slot in previousNode.ConnectedSlots.Except(ConnectedSlots))
                        {
                            slot.tileObject.GetComponent<TileScript>().IsSelected = false;
                        }
                        previousNode.gameObject.SetActive(false);
                        previousNode._isSelected = false;
                    }
                }

                foreach (var slot in ConnectedSlots)
                {
                    slot.tileObject.GetComponent<TileScript>().IsSelected = value;
                }
                gameObject.SetActive(value);

                _isSelected = value;
            }
        }
    }
}
