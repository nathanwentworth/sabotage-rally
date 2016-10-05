using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionsChange : MonoBehaviour {

	public DataManager data;
	public Slider turnTimeSlider;
	public Slider playerNumberSlider;
	public Toggle randomPlayerToggle;
	public Text playerNumberText;
	public Text turnTimeText;

	void OnEnable() {

		turnTimeSlider.value = data.TurnTime;
		playerNumberSlider.value = data.TotalPlayers;
		randomPlayerToggle.isOn = data.RandomPlayerOrder;


		turnTimeSlider.onValueChanged.AddListener(TurnTimeSet);
		playerNumberSlider.onValueChanged.AddListener(PlayerNumberSet);
		randomPlayerToggle.onValueChanged.AddListener(SetRandomPlayerOrder);
		TurnTimeSet(turnTimeSlider.value);
		PlayerNumberSet(playerNumberSlider.value);
		SetRandomPlayerOrder(randomPlayerToggle.isOn);
	}

	public void SaveButton() {
		data.Save();
	}

	public void SetRandomPlayerOrder(bool toggle) {
		if (toggle) {
			data.RandomPlayerOrder = true;
		} else {
			data.RandomPlayerOrder = false;
		}
	}

	public void PlayerNumberSet(float val) {
		float playerSliderVal = val;
		int roundedVal = (int)Mathf.Round(playerSliderVal);
		data.TotalPlayers = roundedVal;
		playerNumberText.text = roundedVal + "";
	}

	public void TurnTimeSet(float val) {
		val = Mathf.Round(val * 10f) / 10f;
		data.TurnTime = val;
		turnTimeText.text = val + "";
	}

	void OnDisable() {
		turnTimeSlider.onValueChanged.RemoveAllListeners();
		playerNumberSlider.onValueChanged.RemoveAllListeners();
	}

}
