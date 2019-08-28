using Scripts.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NameForm : MonoBehaviour
{
	[SerializeField]
	private TMPro.TMP_InputField InputField;
	private bool IsSubmiting = false;
	private bool LoadingScene = false;

    // Start is called before the first frame update
    void Start()
    {
        if (InputField == null) {
			throw new System.Exception("missing input field");
		}
    }

	//SceneManager.LoadScene("MatchScene");

	private void Update() {
		if (LoadingScene == false && NetworkingManager.Instacne.GotMatch) {
			LoadingScene = true;
			SceneManager.LoadScene("MatchScene");
		}
	}

	public void OnSubmitClick() {

		if (IsSubmiting) {
			return;
		}

		string name = InputField.text.Trim();

		if (name == string.Empty) {
			return;
		}

		if (NetworkingManager.Instacne.ConnectionState == DarkRift.ConnectionState.Connected || NetworkingManager.Instacne.Connect()) {
			IsSubmiting = true;
			NetworkingManager.Instacne.MessageNameToServer(name);
		}

	}
}
