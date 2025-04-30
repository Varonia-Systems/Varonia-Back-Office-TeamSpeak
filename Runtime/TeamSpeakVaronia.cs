using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using anyID = System.UInt16;
using uint64 = System.UInt64;
using IntPtr = System.IntPtr;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;


#if VBO_TEAMSPEAK
namespace VaroniaBackOffice
{

    public class TeamSpeakVaronia : MonoBehaviour
    {

        // connection parameter
        [HideInInspector] public string serverAddress = "";
        [HideInInspector] public int serverPort = 9987;
        [HideInInspector] public string serverPassword = "secret";
        [HideInInspector] public string nickName = "";
        private string[] defaultChannel = new string[] { "Channel_1", "" };
        [HideInInspector] public string defaultChannelPassword = null;







        // switch channel parameter
        [HideInInspector]
        public int channelIDX = 1;
        [HideInInspector]
        public string channelPW = "secret";


        private static TeamSpeakClient ts3_client;

        public static bool didNotFindServer = false;

        private static List<int> onTalkStatusChange_status = new List<int>();
        private static List<string> onTalkStatusChange_ClientName = new List<string>();
        private static string onTalkStatusChange_labelText = "";


        public static TeamSpeakVaronia Instance;





        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Instance = this;


        }
        // Use this for initialization
        IEnumerator Start()
        {


            //Getting the client		
            ts3_client = TeamSpeakClient.GetInstance();

            //Attaching functions to the TeamSpeak callbacks.
            TeamSpeakCallbacks.onTalkStatusChangeEvent += onTalkStatusChangeEvent;

            //enabling logging of some pre defined errors.
            TeamSpeakClient.logErrors = true;



            yield return new WaitUntil(() => Config.VaroniaConfig != null);

            serverAddress = Config.VaroniaConfig.TeamSpeak_ServerIP;
            nickName = Config.VaroniaConfig.PlayerName;

            if (string.IsNullOrEmpty(nickName) || nickName.Length <= 2)
                nickName = "Player" + Random.Range(0, 99999);



            StartCoroutine(Connection());

        }


        IEnumerator Connection()
        {

            connect();
            yield return new WaitForSeconds(4);
            if (TeamSpeakClient.started == true)
            {
                var A = ts3_client.GetChannelList();
                if (A != null)
                {
                    for (int i = 0; i < A.Count; i++)
                    {


                        Debug.Log(A[i]);
                    }
                }

                string Temp = "";
                Temp = Config.VaroniaConfig.TeamSpeak_Channel.ToString();
                if (A != null)
                    if (A.Count(l => l == uint64.Parse(Temp)) > 0)
                    {
                        ts3_client.RequestClientMove(ts3_client.GetClientID(), uint64.Parse(Temp), channelPW);
                        Debug.Log("Join Channel " + Temp);

                        var scHandlerID = ts3_client.GetServerConnectionHandlerID();
                        ts3_client.SetPlaybackConfigValue(scHandlerID, TeamSpeakClient.PlaybackConfig.volume_factor_wave, Config.VaroniaConfig.TeamSpeak_Amplification.ToString());
                        ts3_client.SetPlaybackConfigValue(scHandlerID, TeamSpeakClient.PlaybackConfig.volume_modifier, Config.VaroniaConfig.TeamSpeak_Amplification.ToString());
                        ts3_client.SetPreProcessorConfigValue(scHandlerID, TeamSpeakClient.PreProcessorConfig.voiceactivation_level, Config.VaroniaConfig.TeamSpeak_VoiceDetector.ToString());
                    }
                    else
                    {
                        Debug.Log("Create Channel " + Temp);

                        var scHandlerID = ts3_client.GetServerConnectionHandlerID();
                        uint64 _channelID = 0;
                        /* Set data of new channel. Use channelID of 0 for creating channels. */


                        if (Temp != "")
                            ts3_client.SetChannelVariableAsString(scHandlerID, _channelID, ChannelProperties.CHANNEL_NAME, Temp);



                        ts3_client.SetChannelVariableAsInt(scHandlerID, _channelID, ChannelProperties.CHANNEL_FLAG_PERMANENT, 1);
                        ts3_client.SetChannelVariableAsInt(scHandlerID, _channelID, ChannelProperties.CHANNEL_FLAG_SEMI_PERMANENT, 0);

                        /* Flush changes to server */
                        ts3_client.FlushChannelCreation(scHandlerID, 0);

                        yield return new WaitForSeconds(2);

                        ts3_client.SetPlaybackConfigValue(scHandlerID, TeamSpeakClient.PlaybackConfig.volume_factor_wave, Config.VaroniaConfig.TeamSpeak_Amplification.ToString());
                        ts3_client.SetPlaybackConfigValue(scHandlerID, TeamSpeakClient.PlaybackConfig.volume_modifier, Config.VaroniaConfig.TeamSpeak_Amplification.ToString());
                        ts3_client.SetPreProcessorConfigValue(scHandlerID, TeamSpeakClient.PreProcessorConfig.voiceactivation_level, Config.VaroniaConfig.TeamSpeak_VoiceDetector.ToString());

                        ts3_client.RequestClientMove(ts3_client.GetClientID(), uint64.Parse(Temp), channelPW);
                        Debug.Log("Join Channel " + Temp);


                    }



            }

        }

        //Starting the Client
        private void connect()
        {
            ts3_client.StartClient(serverAddress, (uint)serverPort, serverPassword, nickName, ref defaultChannel, defaultChannelPassword);
            // UI Elements




        }

        // Disconnect the Client
        private void disconnect()
        {
            var _leaveMessage = "Bye bye";
            ts3_client.StopConnection(_leaveMessage);
            // UI Elements
            if (TeamSpeakClient.started == true)
            {
            }
        }

        // an example how to switch between Push to Talk / Voice activation


        // an example how to move between channels
        private void switchChannel()
        {
            ts3_client.RequestClientMove(ts3_client.GetClientID(), (ulong)channelIDX, channelPW);
        }


        // an example on how to use different TeamSpeakClient functions.
        private void debugTest()
        {
            //retrieving information.
            Debug.Log("Client nickname: " + ts3_client.GetClientSelfVariableAsString(ClientProperties.CLIENT_NICKNAME));
            Debug.Log("Client version: " + ts3_client.GetClientLibVersion());
            Debug.Log("Server name: " + ts3_client.GetServerVariableAsString(VirtualServerProperties.VIRTUALSERVER_NAME));
            Debug.Log("Max number of clients" + ts3_client.GetServerVariableAsInt(VirtualServerProperties.VIRTUALSERVER_MAXCLIENTS));
            ts3_client.SetClientSelfVariableAsString(ClientProperties.CLIENT_NICKNAME, "newNickName");
            ts3_client.FlushClientSelfUpdates();
            Debug.Log("New client nickname: " + ts3_client.GetClientSelfVariableAsString(ClientProperties.CLIENT_NICKNAME));
            anyID clientID = ts3_client.GetClientID();
            uint64 channelID = ts3_client.GetChannelOfClient(clientID);

            List<uint64> serverConnectionHandlerList = ts3_client.GetServerConnectionHandlerList();
            Debug.Log("Channel name: " + ts3_client.GetChannelVariableAsString(serverConnectionHandlerList[0], channelID, ChannelProperties.CHANNEL_NAME));
            List<anyID> channelClientIDs = ts3_client.GetChannelClientList(serverConnectionHandlerList[0], channelID);
            if (channelClientIDs != null)
            {
                foreach (anyID id in channelClientIDs)
                {
                    Debug.Log("Client id in channel: " + id);
                }
            }

            ts3_client.SetLogVerbosity(TeamSpeakInterface.LogLevel.LogLevel_INFO);

            List<string> captureModes = ts3_client.GetCaptureModeList();
            foreach (string s in captureModes)
            {
                Debug.Log("available capture mode: " + s);
            }

            string defaultCaptureMode = ts3_client.GetDefaultCaptureMode();
            Debug.Log("default capture mode: " + defaultCaptureMode);
            List<TeamSpeakClient.TeamSpeakSoundDevice> captureDevices = ts3_client.GetCaptureDeviceList(defaultCaptureMode);
            foreach (TeamSpeakClient.TeamSpeakSoundDevice soundDevice in captureDevices)
            {
                Debug.Log("Capture Device: " + soundDevice.deviceID + "->" + soundDevice.deviceName);
            }
            TeamSpeakClient.TeamSpeakSoundDevice defaultCaptureDevice = ts3_client.GetDefaultCaptureDevice(defaultCaptureMode);
            Debug.Log("Default capture device: " + defaultCaptureDevice.deviceID + "->" + defaultCaptureDevice.deviceName);

            string bitrate = ts3_client.GetEncodeConfigValue(ts3_client.GetServerConnectionHandlerID(), TeamSpeakClient.EncodeConfig.bitrate);
            Debug.Log("Encode bitrate: " + bitrate);

            ts3_client.LogMessage("Test log message", TeamSpeakInterface.LogLevel.LogLevel_INFO, "client", ts3_client.GetServerConnectionHandlerID());

            //Playing a wave file works on android but the StreamingAssets folder is compressed on Android, therefor the example doesn't support this.
            if (Application.platform != RuntimePlatform.Android)
            {
                string path = Application.streamingAssetsPath;
                path = System.IO.Path.Combine(path, "wavExample.wav");
                uint64 waveHandle;
                ts3_client.PlayWaveFileHandle(ts3_client.GetServerConnectionHandlerID(), path, true, out waveHandle);
                StartCoroutine(PauseWaveHandleIn5Seconds(waveHandle));
            }
        }



        void OnDisable()
        {
            ts3_client.StopClient();
            //On windows the program sometimes freezes on closing the application. You can use the following hack to solve this.
            //#if UNITY_STANDALONE_WIN
            //        if (!Application.isEditor)
            //        {
            //            System.Diagnostics.Process.GetCurrentProcess().Kill();
            //        }
            //#endif
        }

        private void onTalkStatusChangeEvent(uint64 serverConnectionHandlerID, int status, int isReceivedWhisper, anyID clientID)
        {
            string name = ts3_client.GetClientVariableAsString(clientID, ClientProperties.CLIENT_NICKNAME);
            onTalkStatusChange_status.Add(status);
            onTalkStatusChange_ClientName.Add(name);
            if (onTalkStatusChange_status.Count > 1)
            {
                onTalkStatusChange_status.RemoveRange(0, onTalkStatusChange_status.Count - 1);
            }
            onTalkStatusChange_labelText = "";
            for (int i = 0; i < onTalkStatusChange_status.Count; i++)
            {
                onTalkStatusChange_labelText += onTalkStatusChange_ClientName[i];
                if (onTalkStatusChange_status[i] == 1)
                {
                    // Debug.Log(name + " started talking");
                    onTalkStatusChange_labelText += " started talking.\n";
                }
                else
                {
                    // Debug.Log(name + " stopped talking");
                    onTalkStatusChange_labelText += " stopped talking.\n";
                }
            }
        }

        IEnumerator PauseWaveHandleIn5Seconds(uint64 waveHandle)
        {
            yield return new WaitForSeconds(2f);
            ts3_client.Set3DWaveAttributes(ts3_client.GetServerConnectionHandlerID(), waveHandle, new Vector3(10, 10, 10));
            yield return new WaitForSeconds(3f);
            ts3_client.PauseWaveFileHandle(ts3_client.GetServerConnectionHandlerID(), waveHandle, true);
        }



        public void MutePlayer()
        {
            ts3_client.RequestMuteClients(new anyID[] { ts3_client.GetClientID() });
        }

        public void UnMutePlayer()
        {
            ts3_client.RequestUnmuteClients(new anyID[] { ts3_client.GetClientID() });
        }

    }
}
#endif