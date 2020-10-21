using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour {

    public void LoadLevelWithName (string name) {
        SceneManager.LoadScene(name);
    }
}
