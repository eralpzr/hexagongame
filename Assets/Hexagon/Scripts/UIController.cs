using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Hexagon.Scripts
{
    public class UIController : MonoBehaviour
    {
        public TMPro.TMP_Text scoreText;
        public TMPro.TMP_Text gameOverText;

        private LevelController _levelController;

        protected virtual void Awake()
        {
            _levelController = GameObject.FindGameObjectWithTag("LevelController")?.GetComponent<LevelController>();
        }

        public void SetScoreText(string score)
        {
            if (scoreText == null)
                return;

            scoreText.text = $"SCORE\n" + score;
        }

        public void ToggleGameOverText(bool toggle, string reason = null)
        {
            gameOverText.gameObject.SetActive(toggle);
            if (reason != null)
            {
                gameOverText.text = reason + "\nGAME OVER!";
            }
        }

        public void OnClickPlay()
        {
            SceneManager.LoadScene("Game");
        }

        public void OnClickMenu()
        {
            SceneManager.LoadScene("Home");
        }

        public void OnClickUndo()
        {

        }

        public void OnClickExit()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
#endif
            Application.Quit();
        }
    }
}
