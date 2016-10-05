using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SetActiveUIButton : MonoBehaviour {

	public GameObject eventSys;
	public GameObject activeButton;

	void OnEnable() {
		eventSys.GetComponent<EventSystem>().SetSelectedGameObject(activeButton);
	}
}
