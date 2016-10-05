using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class uiManager : MonoBehaviour
{

    private int selectedCar;
    private int gameMode;
    private int selectedTrack;

    // void Update() {
    // 	print(GetComponent<EventSystem>().currentSelectedGameObject);
    // }

    void Start()
    {
        //doing this to get rid of dumb warning, can delete later
        int hey = selectedCar + gameMode + selectedTrack;
        hey += 1;
    }

    public void LoadScene(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void CanvasDisplay(GameObject canvas)
    {
        if (canvas.GetComponent<CanvasGroup>().alpha == 1)
        {
            canvas.GetComponent<CanvasGroup>().alpha = 0;
            canvas.GetComponent<CanvasGroup>().interactable = false;
            canvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
            canvas.SetActive(false);
        }
        else {
            canvas.GetComponent<CanvasGroup>().alpha = 1;
            canvas.GetComponent<CanvasGroup>().interactable = true;
            canvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
            canvas.SetActive(true);
        }
    }

    public void SetGameMode(int mode)
    {
        gameMode = mode;
        if (mode == 0)
        {
            print("selected normal mode!");
        }
        else if (mode == 1)
        {
            print("selected potato mode!");
        }
        else {
            print("<color=red>error!</color> a nonexistent game mode was selected");
        }
    }

    public void SelectTrack(int track)
    {
        selectedTrack = track;
        switch (track)
        {
            case 1:
                print("Selected Easy");
                break;
            case 2:
                print("Selected Med");
                break;
            case 3:
                print("Selected Hard");
                break;
            default:
                print("Dunno what happened there, but something went wrong.");
                break;
        }
    }

    void RotateCarModel()
    {

    }

    public void SetCar(int car)
    {
        selectedCar = car;
        switch (car)
        {
            case 1:
                print("Selected Gremlin");
                break;
            case 2:
                print("Selected Banana");
                break;
            case 3:
                print("Selected Big Wheel");
                break;
            case 4:
                print("Selected Verminator");
                break;
            case 5:
                print("Selected AE86");
                break;
            default:
                print("Dunno what happened there, but something went wrong.");
                break;
        }
    }

    public void RandCar()
    {
        int randVal = Random.Range(1, 6);
        SetCar(randVal);
    }

}
