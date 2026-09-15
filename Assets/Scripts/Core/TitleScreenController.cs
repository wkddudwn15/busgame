using UnityEngine;
using UnityEngine.SceneManagement;

namespace BusMystery.Core
{
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private string busSceneName = "Bus";

        public void StartGame()
        {
            SceneManager.LoadScene(busSceneName);
        }
    }
}
