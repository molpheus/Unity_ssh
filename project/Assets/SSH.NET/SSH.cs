using System.Collections;
using UnityEngine;
using Renci.SshNet;
using System;
using UnityEngine.Events;
using System.IO;

namespace SSH
{
	public class SuccessConnect : UnityEvent<string> { }
	public class FailureConnect : UnityEvent<string> { }

	public class SSH
	{
		SshClient client = null;

		#region CONNECT
		public SuccessConnect successEvent;
		public FailureConnect failureEvent;

        public bool isConnect { get { return client == null ? false : client.IsConnected; } }

        public SSH()
        {
            client = null;
            successEvent = new SuccessConnect();
            failureEvent = new FailureConnect();

        }

        ~SSH()
        {
            if (client != null) {
                DisConnected();
            }
        }

		/// <summary>
		/// パスワードを使用して接続を行う
		/// </summary>
		/// <param name="host">ホスト名</param>
		/// <param name="port">ポート番号</param>
		/// <param name="userName">ユーザ名</param>
		/// <param name="passWord">パスワード</param>
		public void ConnectedToPassword(string host, int port, string userName, string passWord)
		{
			if (client != null) {
				Debug.LogWarning("[Warn] Connected Server!! \n -- Auto DisConnection");
				DisConnected();
			}
			try {
                // 接続ポイントの作成
                ConnectionInfo connection = new PasswordConnectionInfo(host, port, userName, System.Text.Encoding.UTF8.GetBytes(passWord));
                ConnectedClient(connection);
			} catch (Exception e) {
                failureEvent.Invoke ("[NG] " + e.Message);
			}
		}

        private void Connection_AuthenticationBanner(object sender, Renci.SshNet.Common.AuthenticationBannerEventArgs e)
        {
            Debug.Log(sender);
        }

        /// <summary>
        /// Keyファイルを使用して接続を行う
        /// </summary>
        /// <param name="host">ホスト名</param>
        /// <param name="port">ポート番号</param>
        /// <param name="userName">ユーザ名</param>
        /// <param name="keyPath">接続に使用するKeyファイルのパス</param>
        public void ConnectedToKey(string host, int port, string userName, string keyPath)
		{
			try {
				// 接続ポイントの作成
				ConnectionInfo connection = new ConnectionInfo(host, port, userName, new AuthenticationMethod[] {
						//new PasswordAuthenticationMethod(userName, passWord)
						new PrivateKeyAuthenticationMethod(userName, new PrivateKeyFile[]{ new PrivateKeyFile(keyPath) })
                        /* PrivateKeyAuthenticationMethod("キーの場所")を指定することでssh-key認証にも対応しています */
                    }
				);
				ConnectedClient(connection);
			} catch (Exception e) {
				Debug.Log(e.Message);
			}
		}

		/// <summary>
		/// SSH接続を行う
		/// </summary>
		/// <param name="connection"></param>
		private void ConnectedClient(ConnectionInfo connection)
		{
			// コネクションの作成
			client = new SshClient(connection);

            // コネクションの確立
            client.Connect();

			// 接続に成功したかどうか
			if (client.IsConnected) {
				successEvent.Invoke("[OK] SSH Connection succeeded!!");
			}
			else {
				failureEvent.Invoke("[NG] SSH Connection failed!!");
				client = null;
			}

		}

		/// <summary>
		/// 接続を切断する
		/// </summary>
		public void DisConnected()
		{
			// nullの場合、接続していない
			if (client == null) { return; }

			client.Disconnect();
			client.Dispose();

			client = null;
		}
		#endregion

		#region COMMAND

		/// <summary>
		/// コマンドを実行する
		/// </summary>
		/// <param name="command"></param>
		public void Command(string command)
		{
			if (client == null) {
				Debug.LogError("NOT Connected Server");
				return;
			}

			try {
				// コマンドの作成
				SshCommand cmd = client.CreateCommand(command);

				// コマンドの実行
				cmd.Execute();

				if (cmd.Result != string.Empty) {
					successEvent.Invoke(cmd.Result);
				}
				if (cmd.ExitStatus != 0 && cmd.Error != string.Empty) {
					failureEvent.Invoke(cmd.Error);
				}
			} catch (Exception e) {
				Debug.Log(e.Message);
			}
		}

        public IEnumerator IECommand(string command)
        {
            if (client == null) {
                Debug.LogError("NOT Connected Server");
                yield break;
            }

            Debug.Log("Start IEnumerator");

            SshCommand cmd = client.CreateCommand(command);

            var asyncCmd = cmd.BeginExecute();
            var stdoutReader = new StreamReader(cmd.OutputStream);
            var stderrReader = new StreamReader(cmd.ExtendedOutputStream);

            while (!asyncCmd.IsCompleted) {
                string line = stdoutReader.ReadToEnd();
                if (!string.IsNullOrEmpty(line)) {
                    successEvent.Invoke(line);
                }
                line = stderrReader.ReadToEnd();
                if (!string.IsNullOrEmpty(line)) {
                    failureEvent.Invoke(line);
                }

                yield return null;
            }
            cmd.EndExecute(asyncCmd);

            stdoutReader.Dispose();
            stderrReader.Dispose();

            Debug.Log("End IEnumerator");
        }
		#endregion
	}

    public class ScriptOutputLine
    {
        public string Line { get; private set; }
        public bool IsErrorLine { get; private set; }

        public ScriptOutputLine(string line, bool isErrorLine)
        {
            Line = line;
            IsErrorLine = isErrorLine;
        }
    }
}
