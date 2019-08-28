using Scripts.Models;
using Scripts.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardControl : MonoBehaviour
{
	[SerializeField]
	private GameObject SlateSample;

	[SerializeField]
	private GameObject MyPawnSample;

	[SerializeField]
	private GameObject HisPawnSample;

	[SerializeField]
	private TMPro.TextMeshProUGUI TurnText;

	[SerializeField]
	private GameObject winPanel;

	[SerializeField]
	private TMPro.TextMeshProUGUI winText;

	private bool MyTurn = false;

	private GameObject[] SlateGameObjects;

	// Start is called before the first frame update
	void Start()
    {
		winPanel.SetActive(false);

		if (winPanel == null) {
			throw new Exception("missing win panel!");
		}
		if (winText == null) {
			throw new Exception("missing win text!");
		}

		Vector3[] positions = new Vector3[] {
			new Vector3(-1.0f, 1.0f),
			new Vector3(0f, 1.0f),
			new Vector3(1.0f, 1.0f),

			new Vector3(-1.0f, 0.0f),
			new Vector3(0f, 0.0f),
			new Vector3(1.0f, 0.0f),

			new Vector3(-1.0f, -1.0f),
			new Vector3(0f, -1.0f),
			new Vector3(1.0f, -1.0f),
		};

		SlateGameObjects = new GameObject[9];


		for (int i = 0; i < 9; i++) {
			GameObject slate = Instantiate(SlateSample);

			SlateGameObjects[i] = slate;

			slate.transform.parent = gameObject.transform;
			slate.transform.position = positions[i];
			BoardSlateControl slateCon = slate.GetComponent<BoardSlateControl>();
			if (slateCon != null) {
				slateCon.Index = (ushort)i;
			}
		}

		MatchModel.CurrentMatch.OnBoardChange.AddListener(OnBoardChanged);
		SetPlayerTurn(MatchModel.CurrentMatch.CurrentPlayerClientID == NetworkingManager.Instacne.ClientID);
    }

	private ushort SlateIndex;
	private MatchModel.SlateStatus SlateStatus = MatchModel.SlateStatus.NONE;

	private void OnBoardChanged(ushort slateIndex, MatchModel.SlateStatus slateStatus) {
		Debug.Log("board changed " + slateIndex + " changed to " + slateStatus);

		SlateIndex = slateIndex;
		SlateStatus = slateStatus;
	}

	public void SlateClicked(ushort slateIndex) {
		if (MyTurn && MatchModel.CurrentMatch.IsSlateAvailable(slateIndex)) {
			MatchModel.CurrentMatch.ReportSlateTaken(slateIndex);
		}
	}

	void Update() {

		if (SlateStatus != MatchModel.SlateStatus.NONE) {
			GameObject pawnGO;
			if (SlateStatus == MatchModel.SlateStatus.MINE) {
				// my pawn
				pawnGO = Instantiate(MyPawnSample);
			} else {
				// his pawn
				pawnGO = Instantiate(HisPawnSample);
			}

			GameObject slate = SlateGameObjects[SlateIndex];
			pawnGO.transform.parent = slate.transform;
			pawnGO.transform.localPosition = new Vector3();

			SlateStatus = MatchModel.SlateStatus.NONE;

			Debug.Log("current turn is: " + MatchModel.CurrentMatch.CurrentPlayerClientID + " i'm: " + NetworkingManager.Instacne.ClientID);

			SetPlayerTurn(MatchModel.CurrentMatch.CurrentPlayerClientID == NetworkingManager.Instacne.ClientID);

			if (MatchModel.CurrentMatch.Win) {
				// match ended, we have a winner
				winPanel.SetActive(true);
				winText.text = MatchModel.CurrentMatch.IWin ? "I WON!" : "I'm a loser!";
			} else if (MatchModel.CurrentMatch.Draw){
				winPanel.SetActive(true);
				winText.text = "we both are losers";
			}
		}
	}

	private void SetPlayerTurn(bool myTurn) {
		MyTurn = myTurn;
		TurnText.text = myTurn ? "my turn" : "his turn";
	}

	public void Replay() {
		MatchModel.CurrentMatch = null;
		NetworkingManager.Instacne.Disconnect();
		SceneManager.LoadScene("IntroScene");
	}
}
