using Scripts.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Scripts.Models {

	public class BoardChangeEvent : UnityEvent<ushort, MatchModel.SlateStatus> { };

	public class MatchModel {
		
		public enum SlateStatus {
			NONE,
			MINE,
			HIS
		}

		public BoardChangeEvent OnBoardChange;
		public SlateStatus[] slates;

		public readonly ushort Id;
		public static MatchModel CurrentMatch;
		public ushort CurrentPlayerClientID;

		private bool win = false;
		private bool draw = false;
		private bool iWin = false;

		public MatchModel(ushort id, ushort currentPlayerClientID) {
			Id = id;
			CurrentPlayerClientID = currentPlayerClientID;
			slates = new SlateStatus[9];
			OnBoardChange = new BoardChangeEvent();
		}

		public bool IsSlateAvailable(ushort slateIndex) {
			return slates[slateIndex] == SlateStatus.NONE;
		}

		public void ReportSlateTaken(ushort slateIndex) {
			NetworkingManager.Instacne.MessageSlateTaken(slateIndex, Id);
		}

		public void ServerReportSlateTaken(ushort slateIndex, bool myMove, byte gameState, ushort winnerClientID) {
			if (slateIndex >= slates.Length) {
				return;
			}

			if (myMove) {
				CurrentPlayerClientID = (ushort)(NetworkingManager.Instacne.ClientID == 0 ? 1 : 0);
			} else {
				CurrentPlayerClientID = NetworkingManager.Instacne.ClientID;
			}

			win = gameState == 1;
			draw = gameState == 2;
			iWin = winnerClientID == NetworkingManager.Instacne.ClientID;

			if (slates[slateIndex] == SlateStatus.NONE) {
				slates[slateIndex] = myMove ? SlateStatus.MINE : SlateStatus.HIS;
				OnBoardChange.Invoke(slateIndex, slates[slateIndex]);
			} else {
				throw new Exception("trying to override a taken slate");
			}

		}

		public bool Win {
			get { return win; }
		}

		public bool Draw {
			get { return draw; }
		}

		public bool IWin {
			get { return iWin; }
		}

	}
}
