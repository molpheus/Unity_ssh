using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SSHTest : MonoBehaviour
{
    [SerializeField]
    InputField userName;
    [SerializeField]
    InputField hostName;
    [SerializeField]
    InputField port;
    [SerializeField]
    InputField passwd;
    [SerializeField]
    Button connectBtn;
    [SerializeField]
    Text consoleArea;

    SSH.SSH ssh;

    // 一時保持するコンソールのテキスト
    [SerializeField]
    string consoleText = "";

    [SerializeField]
    string inputStr = "";

    // 表示可能最大テキスト量
    int maxConsoleText = 40000;

    bool isInvoke = false;

    // Start is called before the first frame update
    void Start()
    {
        ssh = new SSH.SSH();

        ssh.successEvent.AddListener((str) => {
            consoleText += "\n" + str + "\n";
            isInvoke = false;
        });
        ssh.failureEvent.AddListener((str) => {
            consoleText += "\n<color=red>" + str + "</color>\n";
        });

        connectBtn.onClick.AddListener (Connect);
    }

    private void Update ()
    {
        if ( Input.anyKeyDown && ssh.isConnect) {
            foreach ( KeyCode code in Enum.GetValues (typeof (KeyCode)) ) {
                if ( Input.GetKeyDown (code) ) {
                    //処理を書く
                    //Debug.Log (code);
                    switch ( code ) {
                        case KeyCode.KeypadEnter:
                        case KeyCode.Return:
                        // Enter key
                        Command (inputStr);
                        isInvoke = true;
                        break;

                        case KeyCode.Backspace:
                        // backspace
                        if (inputStr.Length != 0) {
                            inputStr = inputStr.Substring (0, inputStr.Length - 1);
                        }
                        break;

                        case KeyCode.Ampersand: inputStr += "&"; break;
                        case KeyCode.Asterisk: inputStr += "*"; break;
                        case KeyCode.At: inputStr += "@"; break;
                        case KeyCode.BackQuote: inputStr += "`"; break;
                        case KeyCode.Backslash: inputStr += "\\"; break;
                        case KeyCode.Caret: inputStr += "^"; break;
                        case KeyCode.Colon: inputStr += ":"; break;
                        case KeyCode.Comma: inputStr += ";"; break;
                        case KeyCode.Dollar: inputStr += "$"; break;
                        case KeyCode.DoubleQuote: inputStr += "\""; break;
                        case KeyCode.Equals: inputStr += "="; break;
                        case KeyCode.Exclaim: inputStr += "!"; break;
                        case KeyCode.Greater: inputStr += ">"; break;
                        case KeyCode.Hash: inputStr += "#"; break;
                        case KeyCode.LeftBracket: inputStr += "["; break;
                        case KeyCode.LeftCurlyBracket: inputStr += "{"; break;
                        case KeyCode.LeftParen: inputStr += "("; break;
                        case KeyCode.Less: inputStr += "<"; break;
                        case KeyCode.Minus: inputStr += "-"; break;
                        case KeyCode.Percent: inputStr += "%"; break;
                        case KeyCode.Period: inputStr += "."; break;
                        case KeyCode.Pipe: inputStr += "|"; break;
                        case KeyCode.Plus: inputStr += "+"; break;
                        case KeyCode.Question: inputStr += "?"; break;
                        case KeyCode.Quote: inputStr += "'"; break;
                        case KeyCode.RightBracket: inputStr += "]"; break;
                        case KeyCode.RightCurlyBracket: inputStr += "}"; break;
                        case KeyCode.RightParen: inputStr += ")"; break;
                        case KeyCode.Space: inputStr += " "; break;
                        case KeyCode.Semicolon: inputStr += ";"; break;
                        case KeyCode.Slash: inputStr += "/"; break;
                        case KeyCode.Tilde: inputStr += "~"; break;
                        case KeyCode.Underscore: inputStr += "_"; break;

                        case KeyCode.A:
                        case KeyCode.B:
                        case KeyCode.C:
                        case KeyCode.D:
                        case KeyCode.E:
                        case KeyCode.F:
                        case KeyCode.G:
                        case KeyCode.H:
                        case KeyCode.I:
                        case KeyCode.J:
                        case KeyCode.K:
                        case KeyCode.L:
                        case KeyCode.M:
                        case KeyCode.N:
                        case KeyCode.O:
                        case KeyCode.P:
                        case KeyCode.Q:
                        case KeyCode.R:
                        case KeyCode.S:
                        case KeyCode.T:
                        case KeyCode.U:
                        case KeyCode.V:
                        case KeyCode.W:
                        case KeyCode.X:
                        case KeyCode.Y:
                        case KeyCode.Z:
                        inputStr += Input.GetKey(KeyCode.LeftShift) ? code.ToString () : code.ToString ().ToLower ();
                        break;
                    }
                    break;
                }
            }
        }
        consoleArea.text = consoleText + "> " + inputStr + " _";
    }

    void Command(string command)
    {
        consoleText += "> " + command;

        switch (command) {
            case "exit":
            ssh.Command (command);
            consoleText = "";
            ssh.DisConnected ();
            break;

            default:
            ssh.Command (command);
            break;
        }

        inputStr = "";
    }

    public void Connect()
    {
       if (ssh.isConnect) {
            ssh.DisConnected ();
        }

        ssh.ConnectedToPassword (hostName.text, int.Parse(port.text), userName.text, passwd.text);
    }
}
