using CSR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;
using PFWebsocketBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static Timer cmdTimer = new Timer() { AutoReset = true, Enabled = false };
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
#if false
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
                    ListeningOutPut = true;
                    api.runcmd(CmdOutput.cmd);
                }
                else
                {
                    if (!ListeningOutPut) return;
                    if (CmdOutput.Result == null)
                    {     //指令超时检测
                        CmdOutput.waitTimes++;
                        if (CmdOutput.waitTimes * WSBASE.Config.CMDInterval > WSBASE.Config.CMDTimeout)
                        {
                            CmdOutput.Result = "null";
                            CmdOutput = null;
                            if (cmdQueue.Count == 0) { ListeningOutPut = false; }
                        }
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
                #region INFO
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                try
                {
                    string[] authorsInfo = new string[] {
                        "████████████████████████████" ,
                        "正在裝載PFWebsocketAPI",
                        "作者        gxh2004",
                        "版本信息    v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() ,
                        "适用于bds1.16(CSRV0.1.16.20.3v4编译)"  ,
                        "如版本不同可能存在问题" ,
                        "当前CSRunnerAPI版本:" + api.VERSION  ,
                        "配置文件位于\"[BDS目录]\\plugins\\PFWebsocket\\config.json\"",
                        "请修改配置文件后使用，尤其是Password和endpoint",
                        "以免被他人入侵",
                        "████████████████████████████"
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
                cmdTimer.Interval = WSBASE.Config.CMDInterval;
                cmdTimer.Elapsed += CmdTimer_Elapsed;
#if !DEBUG
                #region EULA 
                string eulaPath = Path.GetDirectoryName(WSBASE.ConfigPath) + "\\EULA";
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                JObject eulaINFO = new JObject { new JProperty("author", "gxh"), new JProperty("version", version), new JProperty("device", WSTools.SFingerPrint()) };
                try
                {
                    if (File.Exists(eulaPath))
                    {
                        if (Encoding.UTF32.GetString(File.ReadAllBytes(eulaPath)) != WSTools.GetMD5(WSTools.StringToUnicode(eulaINFO.ToString())))
                        {
                            WriteLineERR("EULA", "使用条款需要更新!");
                            File.Delete(eulaPath);
                            throw new Exception();
                        }
                    }
                    else throw new Exception();
                }
                catch (Exception)
                {
                    using (TaskDialog dialog = new TaskDialog())
                    {
                        dialog.WindowTitle = "接受食用条款";
                        dialog.MainInstruction = "假装下面是本插件的食用条款";
                        dialog.Content =
                            "1.请在遵守CSRunner前置使用协议的前提下使用本插件\n" +
                            "2.不保证本插件不会影响服务器正常运行，如使用本插件造成服务端奔溃等问题，均与作者无瓜\n" +
                            "3.严厉打击插件倒卖等行为，共同维护良好的开源环境";
                        dialog.ExpandedInformation = "点开淦嘛,没东西[doge]";
                        dialog.Footer = "本插件 <a href=\"https://github.com/littlegao233/PFWebsocketAPI\">GitHub开源地址</a>.";
                        dialog.HyperlinkClicked += new EventHandler<HyperlinkClickedEventArgs>((sender, e) => { Process.Start("https://github.com/littlegao233/PFWebsocketAPI"); });
                        dialog.FooterIcon = TaskDialogIcon.Information;
                        dialog.EnableHyperlinks = true;
                        TaskDialogButton acceptButton = new TaskDialogButton("Accept");
                        dialog.Buttons.Add(acceptButton);
                        TaskDialogButton refuseButton = new TaskDialogButton("拒绝并关闭本插件");
                        dialog.Buttons.Add(refuseButton);
                        if (dialog.ShowDialog() == refuseButton)
                            throw new Exception("---尚未接受食用条款，本插件加载失败---");
                    }
                    File.WriteAllBytes(eulaPath, Encoding.UTF32.GetBytes(WSTools.GetMD5(WSTools.StringToUnicode(eulaINFO.ToString()))));
                }
                #endregion
#endif
                #endregion
                WSACT.Start(message =>
                {
                    WSAPImodel.ExecuteCmdModel receiveData;
                    try { receiveData = new WSAPImodel.ExecuteCmdModel(JObject.Parse(message)); }
                    catch (Exception err) { WriteLineERR("收信文本转换失败", err.Message); return; }
                    try
                    {
                        if (receiveData.cmd.StartsWith("op ") ||
                           (receiveData.cmd.StartsWith("execute") && receiveData.cmd.IndexOf("op ") != -1))
                        {
                            WriteLine("出于安全考虑，禁止远程执行op命令");
                            WSACT.SendToAll(receiveData.GetFeedback("出于安全考虑，禁止远程执行op命令"));
                        }
                        else if (receiveData.Auth)
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
                    api.addBeforeActListener(EventKey.onLoadName, eventraw =>
                    {
                        try { return true; }
                        finally
                        {
                            var e = BaseEvent.getFrom(eventraw) as LoadNameEvent;
                            _ = Task.Run(() =>
                                                        {
                                                            try
                                                            {
                                                                var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.onjoin, e.playername, e.xuid);
                                                                WSACT.SendToAll(sendData.ToString());
                                                            }
                                                            catch (Exception err)
                                                            { WriteLineERR("PlayerJoinCallback", err); }
                                                        });
                        }
                    });
                    WriteLine("已开启PlayerJoinCallback监听");
                }
                if (WSBASE.Config.PlayerLeftCallback)
                {
                    api.addBeforeActListener(EventKey.onPlayerLeft, eventraw =>
                    {
                        try { return true; }
                        finally
                        {
                            var e = BaseEvent.getFrom(eventraw) as PlayerLeftEvent;
                            _ = Task.Run(() =>
                                                        {
                                                            try
                                                            {
                                                                var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.onleft, e.playername, e.xuid);
                                                                WSACT.SendToAll(sendData.ToString());
                                                            }
                                                            catch (Exception err)
                                                            { WriteLineERR("PlayerLeftCallback", err); }
                                                        });
                        }
                    });
                    WriteLine("已开启PlayerLeftCallback监听");
                }
                if (WSBASE.Config.PlayerCmdCallback)
                {
                    api.addBeforeActListener(EventKey.onInputCommand, eventraw =>
                    {
                        try { return true; }
                        finally
                        {
                            var e = BaseEvent.getFrom(eventraw) as InputCommandEvent;
                            _ = Task.Run(() =>
                                                        {
                                                            try
                                                            {
                                                                var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.oncmd, e.playername, e.cmd.Substring(1));
                                                                WSACT.SendToAll(sendData.ToString());
                                                            }
                                                            catch (Exception err)
                                                            { WriteLineERR("PlayerCmdCallback", err); }
                                                        });
                        }
                    }); WriteLine("已开启PlayerCmdCallback监听");
                }
                if (WSBASE.Config.PlayerMessageCallback)
                {
                    api.addBeforeActListener(EventKey.onInputText, eventraw =>
                    {
                        try { return true; }
                        finally
                        {
                            var e = BaseEvent.getFrom(eventraw) as InputTextEvent;
                            _ = Task.Run(() =>
                                                        {
                                                            try
                                                            {
                                                                var sendData = new WSAPImodel.SendModel(WSAPImodel.SendType.onmsg, e.playername, e.msg);
                                                                WSACT.SendToAll(sendData.ToString());
                                                            }
                                                            catch (Exception err)
                                                            { WriteLineERR("PlayerMessageCallback", err); }
                                                        });
                        }
                    }); WriteLine("已开启PlayerMessageCallback监听");
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
            oncmd
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
                cmd = receive.Value<string>("cmd").TrimStart();
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
            public string GetFeedback(string text)
            {
                Result = text;
                return GetFeedback();
            }
            public string GetFeedback()
            {
                JObject feedback = new JObject {
                    new JProperty("operate", "runcmd"),
                    new JProperty("Auth", Auth?"PasswdMatch":"Failed"),
                    new JProperty("text",Auth?Result.TrimEnd('\r', '\n',' '): "Password Not Match"),
                    new JProperty("msgid",msgid)
                };
                return feedback.ToString(Formatting.None);
            }
        }
    }
}
