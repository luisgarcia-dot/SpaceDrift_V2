using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    //Funció per carregar una escena de intro. La escena de joc ha de comensar si hi ha un partida guardada.
    public static void CarregarEscena(string nomEscena)
    {
        SceneManager.LoadScene(nomEscena);
    }
    public static void EscenaAjustes()
    {
        SceneManager.LoadScene("EscenaAjustes");
    }
    public static void VolverInicio()
    {
        SceneManager.LoadScene("PantallaInicio");
    }
    public static void SortirAplicacio()
    {
        Application.Quit(); //Només funciona en una build. No per debug.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif 
    }
}
