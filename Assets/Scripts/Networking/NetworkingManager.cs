using DarkRift.Client;
using System.Net;
using System;
using DarkRift;
using Scripts.Models;

namespace Scripts.Networking {

	public class NetworkingManager
	{

		private static NetworkingManager instance;
		public bool GotMatch = false;

		private DarkRiftClient client;

		public static NetworkingManager Instacne {
			get {
				if (instance == null) {
					instance = new NetworkingManager();
				}
				return instance;
			}
		}



		private NetworkingManager() {
			client = new DarkRiftClient();
		}

		public ConnectionState ConnectionState {
			get {
				return client.ConnectionState;
			}
		}

		public bool IsConnected {
			get {
				return client.ConnectionState == ConnectionState.Connected;
			}
		}

		public bool Connect() {

			if (client.ConnectionState == DarkRift.ConnectionState.Connecting) {
				return false;
			}

			if (client.ConnectionState == DarkRift.ConnectionState.Connected) {
				return true;
			}

			try {
				client.Connect(IPAddress.Parse("127.0.0.1"), 4296, DarkRift.IPVersion.IPv4);
				client.MessageReceived += OnMessageReceived;
				return true;
			} catch(Exception) {

			}
			return false;

		}

		public void Disconnect() {
			GotMatch = false;
			client.MessageReceived -= OnMessageReceived;
			client.Disconnect();
		}

		public void MessageNameToServer(string name) {
			if (IsConnected) {

				using (DarkRiftWriter writer = DarkRiftWriter.Create()) {

					writer.Write(name);

					using (Message message = Message.Create((ushort)Tags.Tag.SET_NAME, writer)) {
						client.SendMessage(message, SendMode.Reliable);
					}

				}

			}
		}

		public void MessageSlateTaken(ushort slateIndex, ushort matchId) {
			using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
				writer.Write(matchId);
				writer.Write(slateIndex);
				using (Message message = Message.Create((ushort)Tags.Tag.SLATE_TAKEN, writer)) {
					client.SendMessage(message, SendMode.Reliable);
				}
			}
		}

		// ------------------- GETTING MESSAGES FROM SERVER --------------------
		private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
			switch((Tags.Tag)e.Tag) {
				case Tags.Tag.GOT_MATCH:

					// TODO: start a new match - move the match scene
					using (Message msg = e.GetMessage()) {
						using (DarkRiftReader reader = msg.GetReader()) {

							ushort matchID = reader.ReadUInt16();
							ushort currentPlayerClientID = reader.ReadUInt16();
							MatchModel.CurrentMatch = new MatchModel(matchID, currentPlayerClientID);

						}
					}

					GotMatch = true;
					break;

				case Tags.Tag.SERVER_CONFIRM_SLATE_TAKEN:

					using (Message msg = e.GetMessage()) {
						using (DarkRiftReader reader = msg.GetReader()) {
							ushort slateIndex = reader.ReadUInt16();
							ushort clientID = reader.ReadUInt16();
							byte gameState = reader.ReadByte();
							ushort winnerClientID = gameState == 1 ? reader.ReadUInt16() : (ushort)0;
							
							MatchModel.CurrentMatch.ServerReportSlateTaken(slateIndex, clientID == client.ID, gameState, winnerClientID);
						}
					}

					break;
			}
		}

		public ushort ClientID {
			get { return client.ID; }
		}

	}

}
