using CSR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PFWebsocketBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PFWebsocketAPI
{
    class Program
    {
        #region Console
        private static ConsoleColor defaultForegroundColor = ConsoleColor.White;
        private static ConsoleColor defaultBackgroundColor = ConsoleColor.Black;
        private static void ResetConsoleColor()
        {
            Console.ForegroundColor = defaultForegroundColor;
            Console.BackgroundColor = defaultBackgroundColor;
        }
        public static void WriteLine(object content)
        {
            for (int i = 0; i < 251; i++)
            {
                if (WSACT.WritingAvaliable) break;
                Thread.Sleep(4);
            }
            WSACT.WritingAvaliable = false;
            Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("PFWS");
            Console.ForegroundColor = defaultForegroundColor;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[Main] ");
            ResetConsoleColor();
            Console.WriteLine(content);
            WSACT.WritingAvaliable = true;
        }
        public static void WriteLineERR(object type, object content)
        {
            for (int i = 0; i < 251; i++)
            {
                if (WSACT.WritingAvaliable) break;
                Thread.Sleep(4);
            }
            WSACT.WritingAvaliable = false;
            Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("PFWS");
            Console.ForegroundColor = defaultForegroundColor;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[ERROR] ");
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($">{type}<");
            ResetConsoleColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(content);
            ResetConsoleColor();
            WSACT.WritingAvaliable = true;
        }
        #endregion
        #region Window
        //        private static Thread windowthread = null;
        //        private static ManualResetEvent manualResetEvent = null;
        //        private static bool windowOpened = false;
        //        private static void ShowSettingWindow()
        //        {
        //            try
        //            {
        //                if (windowthread == null)
        //                {
        //                    windowthread = new Thread(new ThreadStart(() =>
        //                    {
        //                        try
        //                        {
        //                            WriteLine("正在加载WPF库");
        //                            while (true)
        //                            {
        //                                try
        //                                {
        //                                    windowOpened = true;
        //                                    new MainWindow().ShowDialog();
        //                                    GC.Collect();
        //                                    windowOpened = false;
        //                                    manualResetEvent = new ManualResetEvent(false);
        //#if DEBUG
        //                                    WriteLine("窗体线程manualResetEvent返回:" +
        //#endif
        //                                            manualResetEvent.WaitOne()
        //#if DEBUG
        //                                            )
        //#endif
        //                                            ;
        //                                    manualResetEvent.Reset();
        //                                }
        //                                catch (Exception err) { WriteLine("窗体执行过程中发生错误\n信息" + err.ToString()); }
        //                            }
        //                        }
        //                        catch (Exception err) { WriteLine("窗体线程发生严重错误\n信息" + err.ToString()); windowthread = null; }
        //                    }));
        //                    windowthread.SetApartmentState(ApartmentState.STA);
        //                    windowthread.Start();
        //                }
        //                else
        //                { if (windowOpened) WriteLine("窗体已经打开"); else manualResetEvent.Set(); }
        //            }
        //            catch (Exception
        //#if DEBUG
        //                    err
        //#endif
        //                    )
        //            {
        //#if DEBUG
        //                WriteLine(err.ToString());
        //#endif
        //            }
        //        }
        #endregion
        public static Queue<WSAPImodel.ExecuteCmdModel> cmdQueue = new Queue<WSAPImodel.ExecuteCmdModel>();
        private static Timer cmdTimer = new Timer(100) { AutoReset = true, Enabled = false };
        private static MCCSAPI.EventCab CmdCallBack = e =>
        {
            try
            {
                if (CmdOutput != null)
                {
                    if (CmdOutput.Result == null)
                    {
                        CmdOutput.Result = (BaseEvent.getFrom(e) as ServerCmdOutputEvent).output;
                        if (cmdQueue.Count == 0) { ListeningOutPut = false; }
                    }
                    CmdOutput = null;
#if DEBUG
                    WriteLine("ExecuteCmdOutPuted");
                    return true;
#else
                    return false;
#endif
                }
            }
            catch (Exception err) { WriteLineERR("读取命令输出出错", err); }
            return true;
        };
        private static WSAPImodel.ExecuteCmdModel CmdOutput = null;
        private static bool _listeningOutPut = false;
        public static bool ListeningOutPut
        {
            get => _listeningOutPut;
            set
            {
                if (_listeningOutPut != value)
                {
                    if (value)
                    {
                        api.addBeforeActListener(EventKey.onServerCmdOutput, CmdCallBack);
                    }
                    else
                    {
                        api.removeBeforeActListener(EventKey.onServerCmdOutput, CmdCallBack);
                        if (cmdTimer.Enabled) cmdTimer.Stop();
                    }
                }
                _listeningOutPut = value;
            }
        }
        private static void CmdTimer_Elapsed(object sender, ElapsedEventArgs ev)
        {
            try
            {
                if (CmdOutput == null)
                {
                    CmdOutput = cmdQueue.Dequeue();
                    WriteLine(CmdOutput.cmd);
                    ListeningOutPut = true;
                    api.runcmd(CmdOutput.cmd);
                }
                else
                {
                    if (CmdOutput.Result == null)
                    {
                        //指令超时检测
                        if (CmdOutput.waitTimes >= WSBASE.Config.CMDTimeout * 10)
                        {
                            CmdOutput.Result = "";
                            CmdOutput = null;
                            if (cmdQueue.Count == 0) { ListeningOutPut = false; }
                        }
                        CmdOutput.waitTimes++;
                    }
                }
            }
            catch (Exception err) { WriteLineERR("命令序列操作异常", err); }
        }
        private static MCCSAPI api = null;
        public static void Init(MCCSAPI mcapi)
        {
            api = mcapi;
            Console.OutputEncoding = Encoding.UTF8;
            defaultForegroundColor = Console.ForegroundColor;
            defaultBackgroundColor = Console.BackgroundColor;
            WSACT.defaultForegroundColor = Console.ForegroundColor;
            WSACT.defaultBackgroundColor = Console.BackgroundColor;
            try
            {
                #region 加载
                cmdTimer.Elapsed += CmdTimer_Elapsed;
                #region INFO
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                try
                {
                    string[] authorsInfo = new string[] {
                        "███████████████████████████" ,
                        "正在裝載PFWebsocketAPI",
                        "作者        gxh2004",
                        "版本信息    v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() ,
                        "适用于bds1.16(CSRV0.1.16.20.3v4编译)"  ,
                        "如版本不同可能存在问题" ,
                        "当前CSRunnerAPI版本:" + api.VERSION  ,
                        "███████████████████████████"
                    };
                    Func<string, int> GetLength = (input) => { return Encoding.GetEncoding("GBK").GetByteCount(input); };
                    int infoLength = 0;
                    foreach (var line in authorsInfo) infoLength = Math.Max(infoLength, GetLength(line));
                    for (int i = 0; i < authorsInfo.Length; i++)
                    {
                        while (GetLength(authorsInfo[i]) < infoLength) authorsInfo[i] += " ";
                        Console.WriteLine("█" + authorsInfo[i] + "█");
                    }

                }
                catch (Exception) { }
                ResetConsoleColor();
                #endregion
                #endregion
                WSACT.Start(message =>
                {
                    WSAPImodel.ExecuteCmdModel receiveData;
                    try { receiveData = new WSAPImodel.ExecuteCmdModel(JObject.Parse(message)); }
                    catch (Exception err) { WriteLineERR("收信文本转换失败", err.Message); return; }
                    try
                    {
                        if (receiveData.Auth)
                        {//添加到序列
                            cmdQueue.Enqueue(receiveData);
                            CmdTimer_Elapsed(cmdTimer, null);
                            if (!cmdTimer.Enabled) cmdTimer.Start();
                        }
                        else
                        {//直接返回错误
                            WriteLine("命令未执行");
                            WSACT.SendToAll(receiveData.GetFeedback());
                        }
                    }
                    catch (Exception) { }
                });
                #region 注册各类监听
                if (WSBASE.Config.PlayerJoinCallback)
                {
                    api.addAfterActListener(EventKey.onLoadName, eventraw =>
                    {
                        try
                        { return true; }
                        finally
                        {
                            try
                            {
                                var e = BaseEvent.getFrom(eventraw) as LoadNameEvent;
                                var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.onjoin, e.playername, e.xuid);
                                WSACT.SendToAll(sendData.ToString());
                            }
                            catch (Exception err)
                            { WriteLineERR("PlayerJoinCallback", err); }
                        }
                    });
                }
                if (WSBASE.Config.PlayerLeftCallback)
                {
                    api.addAfterActListener(EventKey.onPlayerLeft, eventraw =>
                    {
                        try
                        { return true; }
                        finally
                        {
                            try
                            {
                                var e = BaseEvent.getFrom(eventraw) as PlayerLeftEvent;
                                var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.onleft, e.playername, e.xuid);
                                WSACT.SendToAll(sendData.ToString());
                            }
                            catch (Exception err)
                            { WriteLineERR("PlayerLeftCallback", err); }
                        }
                    });
                }
                if (WSBASE.Config.PlayerCmdCallback)
                {
                    api.addAfterActListener(EventKey.onInputCommand, eventraw =>
                    {
                        try
                        { return true; }
                        finally
                        {
                            try
                            {
                                var e = BaseEvent.getFrom(eventraw) as InputCommandEvent;
                                var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.onCMD, e.playername, e.cmd.Substring(1));
                                WSACT.SendToAll(sendData.ToString());
                            }
                            catch (Exception err)
                            { WriteLineERR("PlayerCmdCallback", err); }
                        }
                    });
                }
                if (WSBASE.Config.PlayerMessageCallback)
                {
                    api.addAfterActListener(EventKey.onChat, eventraw =>
                    {
                        try
                        { return true; }
                        finally
                        {
                            try
                            {
                                var e = BaseEvent.getFrom(eventraw) as ChatEvent;
                                Console.WriteLine(e.chatstyle);
                                if (e.chatstyle == "chat")
                                {
                                    var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.onmsg, e.playername, e.msg);
                                    WSACT.SendToAll(sendData.ToString());
                                }
                            }
                            catch (Exception err)
                            { WriteLineERR("PlayerMessageCallback", err); }
                        }
                    });
                }
                #endregion
            }
            catch (Exception err) { WriteLineERR("插件遇到严重错误，无法继续运行", err.Message); }
        }
    }
    class WSAPImodel
    {
        public enum SendType
        {
            onmsg,
            onjoin,
            onleft,
            onCMD
        }
        internal class SendModel
        {
            public SendModel(SendType _type, string _target, string _text)
            {
                operate = _type;
                target = _target;
                text = _text;
            }
            public SendType operate;
            public string target, text;
            public override string ToString()
            {
                JObject feedback = JObject.FromObject(this);
                feedback["operate"] = operate.ToString();
                return feedback.ToString(Formatting.None);
            }
        }
        public enum ReceiveType
        {
            runcmd
        }
        internal class ExecuteCmdModel
        {
            public ExecuteCmdModel(JObject receive)
            {
                operate = (ReceiveType)Enum.Parse(typeof(ReceiveType), receive.Value<string>("operate"));
                cmd = receive.Value<string>("cmd");
                msgid = receive.Value<string>("msgid");
                string token = receive.Value<string>("passwd");
                receive["passwd"] = "";
                Auth = token == WSTools.GetMD5(WSBASE.Config.Password + DateTime.Now.ToString("yyyyMMddHHmm") + "@" + receive.ToString(Formatting.None));
                if (!Auth)
                    Auth = token == WSTools.GetMD5(WSBASE.Config.Password + (DateTime.Now - (new TimeSpan(0, 0, 1, 0))).ToString("yyyyMMddHHmm") + "@" + receive.ToString(Formatting.None));
                if (!Auth)
                    WSACT.WriteLineERR("密匙不匹配:", "收到密匙:" + token + "\t本地密匙:" + WSTools.GetMD5(WSBASE.Config.Password + DateTime.Now.ToString("yyyyMMddHHmm") + "@" + receive.ToString(Formatting.None)));
            }
            public bool Auth = false;
            public ReceiveType operate;
            public string cmd, msgid;
            private string result = null;
            public string Result
            {
                get => result;
                set
                {
                    result = value;
                    WSACT.SendToAll(GetFeedback());
                }
            }
            public int waitTimes = 0;
            public string GetFeedback()
            {
                JObject feedback = new JObject {
                    new JProperty("operate", "runcmd"),
                    new JProperty("Auth", Auth?"PasswdMatch":"Failed"),
                    new JProperty("text",Auth?Result: "Password Not Match"),
                    new JProperty("msgid",msgid)
                };
                return feedback.ToString(Formatting.None);
            }
        }
    }
}
