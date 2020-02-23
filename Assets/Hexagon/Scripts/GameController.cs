using Assets.Hexagon.Scripts.Datas;
using UnityEngine;

namespace Assets.Hexagon.Scripts
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; protected set; }

        public GameDatabaseData gameDatabaseData;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        void Start()
        {
        }

        void Update()
        {
        
        }
    }
}
