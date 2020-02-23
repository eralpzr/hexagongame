using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Hexagon.Scripts.Datas;
using Assets.Hexagon.Scripts.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Hexagon.Scripts
{
    public class LevelController : MonoBehaviour
    {
        public UIController uiController;

        private GameDatabaseData _gameDatabaseData;
        private TileGenerator _tileGenerator;
        private Camera _mainCamera;

        private Vector3 _startTouchPosition;
        private Vector3 _endTouchPosition;

        private int _score;

        protected virtual void Awake()
        {
            _gameDatabaseData = GameController.Instance.gameDatabaseData;
            _tileGenerator = GetComponent<TileGenerator>();
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        protected virtual void Start()
        {
            Score = 0;
        }

        protected virtual void Update()
        {
            if (!CanTouch || IsGameOver)
                return;

            if (Application.isEditor && Input.GetMouseButtonDown(0) || Application.isMobilePlatform && Input.touchCount > 0)
            {
                if (Application.isMobilePlatform)
                {
                    var touch = Input.GetTouch(0);

                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            _startTouchPosition = touch.position;
                            break;

                        case TouchPhase.Moved:
                        case TouchPhase.Ended:
                            _endTouchPosition = touch.position;
                            CheckTouchPositions();
                            break;
                    }
                }
                else
                {
                    _startTouchPosition = Input.mousePosition;
                }
            }
            else if (Application.isEditor && Input.GetMouseButtonUp(0))
            {
                _endTouchPosition = Input.mousePosition;
                CheckTouchPositions();
            }
        }

        private void CheckTouchPositions()
        {
            float x = _endTouchPosition.x - _startTouchPosition.x;
            float y = _endTouchPosition.y - _startTouchPosition.y;

            var selectedNode = _tileGenerator.Nodes.FirstOrDefault(_ => _.IsSelected);
            if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0)
            {
                OnTapped();
            }
            else if (Mathf.Abs(x) > Mathf.Abs(y) && x > 0 || Mathf.Abs(x) < Mathf.Abs(y) && y < 0)
            {
                if (selectedNode == null)
                    return;

                StartCoroutine(TurnAndCheck(TurnType.Clockwise, selectedNode));
            }
            else
            {
                if (selectedNode == null)
                    return;

                StartCoroutine(TurnAndCheck(TurnType.CounterClockwise, selectedNode));
            }
        }

        private void OnTapped()
        {
            Vector3 touchPosition = Vector3.zero;
            if (Application.isEditor)
            {
                touchPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            }
            else if (Application.isMobilePlatform)
            {
                var touch = Input.GetTouch(0);
                touchPosition = _mainCamera.ScreenToWorldPoint(touch.position);
            }

            var foundNode = _tileGenerator
                            .Nodes.OrderBy(x => (touchPosition - x.transform.position).sqrMagnitude)
                            .FirstOrDefault();

            if (foundNode == null)
                return;

            foundNode.IsSelected = true;
        }

        private IEnumerator TurnAndCheck(TurnType turnType, NodeScript nodeScript)
        {
            if (IsGameOver)
                yield break;

            CanTouch = false;

            var found = false;
            var i = 0;
            do
            {
                switch (turnType)
                {
                    case TurnType.Clockwise:
                        nodeScript.TurnClockwise();
                        break;
                    case TurnType.CounterClockwise:
                        nodeScript.TurnCounterClockwise();
                        break;
                }

                i++;

                yield return new WaitForSeconds(0.5f);
                if (found = _tileGenerator.Nodes.Any(x => x.CheckSlotsSameColors()))
                    break;
            }
            while (i < nodeScript.ConnectedSlots.Length);

            if (found)
            {
                nodeScript.IsSelected = false;
                yield return CheckAndDestroySameColors();
            }

            CanTouch = true;
        }

        public IEnumerator CheckAndDestroySameColors()
        {
            if (IsGameOver)
                yield break;

            var oldScore = Score;
            var nodes = _tileGenerator.Nodes.Where(x => x.CheckSlotsSameColors()).ToList();
            var tiles = _tileGenerator.Slots.Select(x => x.tileObject);
            while(nodes.Count > 0)
            {
                foreach (var node in nodes)
                {
                    node.DestroyTiles();
                }

                yield return _tileGenerator.FillEmptySlots();
                yield return new WaitForSeconds(0.5f);

                nodes = _tileGenerator.Nodes.Where(x => x.CheckSlotsSameColors()).ToList();
            }

            foreach (var tile in tiles)
            {
                if (tile == null)
                    continue;

                tile.GetComponent<TileScript>().OnEveryMove();

                if (IsGameOver)
                    yield break;
            }

            if (!CheckValidAnyMove())
            {
                GameOver("NO MOVES LEFT");
                yield break;
            }

            if (oldScore / _gameDatabaseData.BombTileSpawnScore != Score / _gameDatabaseData.BombTileSpawnScore)
            {
                SpawnBombTile();
            }
        }

        public bool CheckValidAnyMove()
        {
            var possibleNodes = _tileGenerator.Nodes.Where(
                x =>
                {
                    return x.ConnectedSlots
                            .Select(y => y.tileObject.GetComponent<TileScript>()
                                          .Color)
                            .Distinct()
                            .Count() == 2;
                });

            foreach (var node in possibleNodes)
            {
                var differentSlot = node.ConnectedSlots.FirstOrDefault(
                    x =>
                    {
                        return node.ConnectedSlots.Count(
                                   y => y.tileObject.GetComponent<TileScript>()
                                         .Color == x
                                                   .tileObject.GetComponent<TileScript>()
                                                   .Color) == 1;
                    });

                if (differentSlot == null)
                    continue;

                var needNodeColor = node.ConnectedSlots.First(x => x != differentSlot).tileObject
                                        .GetComponent<TileScript>().Color;

                var isNeedColorExists = differentSlot
                                        .ConnectedNodes.SelectMany(x => x.ConnectedSlots).Distinct()
                                        .Except(node.ConnectedSlots)
                                        .Any(x => x.tileObject.GetComponent<TileScript>().Color == needNodeColor);
                
                if (isNeedColorExists)
                    return true;
            }

            return false;
        }

        public void SpawnBombTile()
        {
            if (IsGameOver)
                return;

            var slots = _tileGenerator.Slots.Where(x => x.tileObject != null && !(x.tileObject.GetComponent<TileScript>() is BombTileScript)).ToList();
            if (slots.Count == 0)
                return;

            var randomSlot = slots[Random.Range(0, slots.Count)];
            randomSlot.ReplaceTile(_tileGenerator.bombTilePrefab, transform);
        }

        public void GameOver(string reason = null)
        {
            Debug.Log("GAME OVER!");
            IsGameOver = true;

            foreach (var slot in _tileGenerator.Slots)
            {
                slot.tileObject.GetComponent<TileScript>().CreateParticle();
                slot.DestroyTile();
            }

            uiController.ToggleGameOverText(true, reason);
        }

        public bool CanTouch { get; set; }
        public bool IsGameOver { get; set; }

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                uiController.SetScoreText(_score.ToString());
            }
        }
    }
}
