using System.Linq;
using Assets.Hexagon.Scripts.Datas;
using UnityEngine;

namespace Assets.Hexagon.Scripts
{
    public class TileScript : MonoBehaviour
    {
        protected GameDatabaseData gameDatabaseData;
        protected SpriteRenderer spriteRenderer;
        protected TileGenerator tileGenerator;
        protected LevelController levelController;
        protected GameObject selectedObject;

        private bool _isSelected = false;

        protected virtual void Awake()
        {
            gameDatabaseData = GameController.Instance.gameDatabaseData;
            selectedObject = transform.GetChild(0).gameObject;
            spriteRenderer = GetComponent<SpriteRenderer>();
            tileGenerator = GameObject.FindGameObjectWithTag("LevelController").GetComponent<TileGenerator>();
            levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        }

        protected virtual void Start()
        {
            selectedObject.SetActive(_isSelected);
        }

        protected virtual void Update()
        {
        }

        public virtual void OnEveryMove()
        {

        }

        public virtual void OnTileDestroy()
        {
            levelController.Score += gameDatabaseData.ScoreEveryExplode;
            CreateParticle();
        }

        public void CreateParticle()
        {
            var particle = Instantiate(tileGenerator.particlePrefab, transform.position, Quaternion.identity);
            var particleScript = particle.GetComponent<ParticleSystem>();
            var settings = particleScript.main;
            settings.startColor = Color;
        }

        public virtual Color Color
        {
            get => spriteRenderer.color;
            set
            {
                if (spriteRenderer.color == value)
                    return;

                spriteRenderer.color = value;
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (selectedObject == null)
                    return;

                selectedObject.SetActive(_isSelected = value);
            }
        }
    }
}
