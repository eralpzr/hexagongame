using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Hexagon.Scripts.Datas;
using Assets.Hexagon.Scripts.Models;
using UnityEngine;

namespace Assets.Hexagon.Scripts
{
    public class TileGenerator : MonoBehaviour
    {
        public GameObject tilePrefab;
        public GameObject bombTilePrefab;
        public GameObject nodePrefab;
        public GameObject particlePrefab;
        public Vector3 tileSpace;
        public Vector3 cameraOffset;

        private LevelController _levelController;
        private GameDatabaseData _gameDatabaseData;
        private Camera _mainCamera;

        protected virtual void Awake()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            _gameDatabaseData = GameController.Instance.gameDatabaseData;
            _levelController = GetComponent<LevelController>();
        }

        protected virtual void Start()
        {
            StartCoroutine(GenerateTiles());
        }

        protected virtual void Update()
        {
            if (Slots.Count == 0)
                return;

            foreach (var slot in Slots)
            {
                if (slot.tileObject == null)
                    continue;

                if (slot.tileObject.transform.position == slot.position)
                    continue;

                var fromPosition = slot.tileObject.transform.position;
                slot.tileObject.transform.position = Vector3.Lerp(fromPosition, slot.position, Time.deltaTime * _gameDatabaseData.FallSpeed);
            }
        }

        public IEnumerator GenerateTiles()
        {
            _levelController.CanTouch = false;

            CreateSlots();
            CreateNodes();
            CenterCamera();

            float speed = 0.001f * Slots.Count; 
            foreach (var slot in Slots)
            {
                if (slot.tileObject != null)
                    continue;

                var tileObject = slot.CreateTile(tilePrefab, transform);
                var tileScript = tileObject.GetComponent<TileScript>();
                do
                {
                    tileScript.Color = _gameDatabaseData.HexagonColors[Random.Range(0, _gameDatabaseData.HexagonColors.Count)];
                }
                while (slot.ConnectedNodes.Any(x => x.CheckSlotsSameColors()));

                yield return new WaitForSeconds(speed -= 0.001f);
            }

            _levelController.CanTouch = true;
        }

        public IEnumerator FillEmptySlots()
        {
            foreach (var group in Enumerable.Range(0, Slots.Count).GroupBy(x => x % _gameDatabaseData.Width))
            {
                var slots = group.Select(x => Slots[x]).ToList();
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].tileObject != null)
                        continue;

                    for (int j = i + 1; j < slots.Count; j++)
                    {
                        if (slots[j].tileObject == null)
                            continue;

                        slots[i].tileObject = slots[j].tileObject;
                        slots[j].tileObject = null;
                        break;
                    }
                }
            }

            foreach (var emptySlot in Slots.Where(x => x.tileObject == null))
            {
                var tileObject = emptySlot.CreateTile(tilePrefab, transform);
                var tileScript = tileObject.GetComponent<TileScript>();
                tileScript.Color = _gameDatabaseData.HexagonColors[Random.Range(0, _gameDatabaseData.HexagonColors.Count)];

                yield return new WaitForSeconds(0.001f);
            }
        }

        public void CreateSlots()
        {
            Slots.ForEach(x => x.DestroyTile());
            Slots.Clear();

            for (int i = 0; i < _gameDatabaseData.Height; i++)
            {
                for (int j = 0; j < _gameDatabaseData.Width; j++)
                {
                    var position = new Vector3(j * (.75f + tileSpace.x), i * (1.0f + tileSpace.y));
                    position += j % 2 == 1 ? new Vector3(0f, -.5f) : Vector3.zero;

                    var slot = new Slot(position);
                    Slots.Add(slot);
                }
            }
        }

        public void CreateNodes()
        {
            Nodes.ForEach(Destroy);
            Nodes.Clear();

            for (int i = 0; i < (_gameDatabaseData.Height - 1) * 2; i++)
            {
                for (int j = 0; j < _gameDatabaseData.Width - 1; j++)
                {
                    var position = new Vector3(j  * (.75f + tileSpace.x), i * (1.0f + tileSpace.y) / 2);
                    if (i % 2 == 0)
                    {
                        position += j % 2 == 0 ? new Vector3(.5f, 0f) : new Vector3(.25f, 0f);
                    }
                    else
                    {
                        position += j % 2 == 0 ? new Vector3(.25f, 0f) : new Vector3(.5f, 0f);
                    }

                    var nodeObject = Instantiate(nodePrefab, position, Quaternion.identity, transform);
                    var nodeScript = nodeObject.GetComponent<NodeScript>();

                    Nodes.Add(nodeScript);
                }
            }
        }

        public void CenterCamera()
        {
            if (Slots.Count == 0)
                return;

            var cornerSlot1 = Slots[0];
            var cornerSlot2 = Slots[_gameDatabaseData.Width * _gameDatabaseData.Height - 1];
            _mainCamera.transform.position = Vector3.Lerp(cornerSlot1.position, cornerSlot2.position + cameraOffset, .5f) + Vector3.back * 10;

            var unitsPerPixel = _gameDatabaseData.Width / (float) Screen.width;
            var desiredHalfHeight = unitsPerPixel * Screen.height * .5f;
            _mainCamera.orthographicSize = desiredHalfHeight;
        }

        public List<Slot> Slots { get; } = new List<Slot>();
        public List<NodeScript> Nodes { get; } = new List<NodeScript>();
    }
}
