Imports CSR
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Ookii.Dialogs.Wpf
Imports PFWebsocketAPI.PFWebsocketAPI.Model
Imports PFWebsocketBase
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Timers
Imports System.Windows.Threading
Imports Timer = System.Timers.Timer
Namespace PFWebsocketAPI
    Friend Module Program
#Region "Console"
        Private defaultForegroundColor As ConsoleColor = ConsoleColor.White
        Private defaultBackgroundColor As ConsoleColor = ConsoleColor.Black

        Private Sub ResetConsoleColor()
            Console.ForegroundColor = defaultForegroundColor
            Console.BackgroundColor = defaultBackgroundColor
        End Sub

        Public Sub WriteLine(ByVal content As Object)
            For i = 0 To 250
                If WSACT.WritingAvaliable Then Exit For
                Thread.Sleep(4)
            Next

            WSACT.WritingAvaliable = False
            Console.Write($"[{Date.Now:yyyy-MM-dd HH:mm:ss} ")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.Write("PFWS")
            Console.ForegroundColor = defaultForegroundColor
            Console.Write("]")
            Console.ForegroundColor = ConsoleColor.Cyan
            Console.Write("[Main] ")
            ResetConsoleColor()
            Console.WriteLine(content)
            WSACT.WritingAvaliable = True
        End Sub

        Public Sub WriteLineERR(ByVal type As Object, ByVal content As Object)
            For i = 0 To 250
                If WSACT.WritingAvaliable Then Exit For
                Thread.Sleep(4)
            Next
            WSACT.WritingAvaliable = False
            Console.Write($"[{Date.Now:yyyy-MM-dd HH:mm:ss} ")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.Write("PFWS")
            Console.ForegroundColor = defaultForegroundColor
            Console.Write("]")
            Console.ForegroundColor = ConsoleColor.Red
            Console.Write("[ERROR] ")
            Console.BackgroundColor = ConsoleColor.DarkRed
            Console.ForegroundColor = ConsoleColor.White
            Console.Write($">{type}<")
            ResetConsoleColor()
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine(content)
            ResetConsoleColor()
            WSACT.WritingAvaliable = True
        End Sub
#End Region
#Region "Window"
        '        private static Thread windowthread = null;
        '        private static ManualResetEvent manualResetEvent = null;
        '        private static bool windowOpened = false;
        '        private static void ShowSettingWindow()
        '        {
        '            try
        '            {
        '                if (windowthread == null)
        '                {
        '                    windowthread = new Thread(new ThreadStart(() =>
        '                    {
        '                        try
        '                        {
        '                            WriteLine("正在加载WPF库");
        '                            while (true)
        '                            {
        '                                try
        '                                {
        '                                    windowOpened = true;
        '                                    new MainWindow().ShowDialog();
        '                                    GC.Collect();
        '                                    windowOpened = false;
        '                                    manualResetEvent = new ManualResetEvent(false);
        '#if DEBUG
        '                                    WriteLine("窗体线程manualResetEvent返回:" +
        '#endif
        '                                            manualResetEvent.WaitOne()
        '#if DEBUG
        '                                            )
        '#endif
        '                                            ;
        '                                    manualResetEvent.Reset();
        '                                }
        '                                catch (Exception err) { WriteLine("窗体执行过程中发生错误\n信息" + err.ToString()); }
        '                            }
        '                        }
        '                        catch (Exception err) { WriteLine("窗体线程发生严重错误\n信息" + err.ToString()); windowthread = null; }
        '                    }));
        '                    windowthread.SetApartmentState(ApartmentState.STA);
        '                    windowthread.Start();
        '                }
        '                else
        '                { if (windowOpened) WriteLine("窗体已经打开"); else manualResetEvent.Set(); }
        '            }
        '            catch (Exception
        '#if DEBUG
        '                    err
        '#endif
        '                    )
        '            {
        '#if DEBUG
        '                WriteLine(err.ToString());
        '#endif
        '            }
        '        }
#End Region
        Public cmdQueue As New Queue(Of CauseRuncmdFeedback)
        Friend cmdTimer As Timer = New Timer() With {
            .AutoReset = True,
            .Enabled = False
        }
        Private CmdCallBack As MCCSAPI.EventCab = Function(e)
                                                      Try
                                                          If InvokingCmd IsNot Nothing Then
                                                              If Equals(InvokingCmd.params.result, Nothing) Then
                                                                  InvokingCmd.params.result = TryCast(BaseEvent.getFrom(e), ServerCmdOutputEvent).output
                                                                  If cmdQueue.Count = 0 Then
                                                                      ListeningOutPut = False
                                                                  End If
                                                              End If
                                                              InvokingCmd = Nothing
#If False Then
                    WriteLine("ExecuteCmdOutPuted");
                    return true;
#Else
                                                              Return False
#End If
                                                          End If
                                                      Catch err As Exception
                                                          WriteLineERR("读取命令输出出错", err)
                                                      End Try
                                                      Return True
                                                  End Function
        Private _InvokingCmd As CauseRuncmdFeedback = Nothing
        Private Property InvokingCmd As CauseRuncmdFeedback
            Get
                Return _InvokingCmd
            End Get
            Set(value As CauseRuncmdFeedback)
                If _InvokingCmd IsNot Nothing Then
                    Dim sendData = _InvokingCmd.ToString
                    If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                    SendToCon(_InvokingCmd.params.con, sendData)
                End If
                _InvokingCmd = value
            End Set
        End Property
        Private _listeningOutPut As Boolean = False
        Public Property ListeningOutPut As Boolean
            Get
                Return _listeningOutPut
            End Get
            Set(value As Boolean)
                If _listeningOutPut <> value Then
                    If value Then
                        api.addBeforeActListener(EventKey.onServerCmdOutput, CmdCallBack)
                    Else
                        api.removeBeforeActListener(EventKey.onServerCmdOutput, CmdCallBack)
                        If cmdTimer.Enabled Then cmdTimer.Stop()
                    End If
                End If
                _listeningOutPut = value
            End Set
        End Property
        Friend Sub CmdTimer_Elapsed(sender As Object, ev As ElapsedEventArgs)
            Try
                If InvokingCmd Is Nothing AndAlso cmdQueue.Count > 0 Then
                    InvokingCmd = cmdQueue.Dequeue()
                    ListeningOutPut = True
                    api.runcmd(InvokingCmd.params.cmd)
                Else
                    If Not ListeningOutPut Then Return
                    If IsNothing(InvokingCmd.params.result) Then     '指令超时检测
                        InvokingCmd.params.waiting += 1
                        If InvokingCmd.params.waiting * Config.CMDInterval > Config.CMDTimeout Then
                            InvokingCmd.params.result = "null"
                            InvokingCmd = Nothing
                            If cmdQueue.Count = 0 Then
                                ListeningOutPut = False
                            End If
                        End If
                    End If
                End If
            Catch err As Exception
                WriteLineERR("命令序列操作异常", err)
            End Try
        End Sub
        Friend api As MCCSAPI = Nothing
        Public Sub Init(ByVal mcapi As MCCSAPI)
            api = mcapi
            Console.OutputEncoding = Encoding.UTF8
            defaultForegroundColor = Console.ForegroundColor
            defaultBackgroundColor = Console.BackgroundColor
            WSACT.defaultForegroundColor = Console.ForegroundColor
            WSACT.defaultBackgroundColor = Console.BackgroundColor
            Try
#Region "加载"
#Region "INFO"
                Console.ForegroundColor = ConsoleColor.Cyan
                Console.BackgroundColor = ConsoleColor.DarkBlue
                Try
                    Dim authorsInfo As String() = New String() {
                        "████████████████████████████",
                        "正在裝載PFWebsocketAPI", "作者        gxh2004",
                        "版本信息    v" & Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                        "适用于bds1.16(CSRV0.1.16.201v7编译)", "如版本不同可能存在问题", "当前CSRunnerAPI版本:" & api.VERSION,
                        "配置文件位于""[BDS目录]\plugins\PFWebsocket\config.json""",
                        "请修改配置文件后使用，尤其是Password和endpoint",
                        "以免被他人入侵",
                        "████████████████████████████"}
                    Dim GetLength As Func(Of String, Integer) = Function(input) Encoding.GetEncoding("GBK").GetByteCount(input)
                    Dim infoLength = 0

                    For Each line In authorsInfo
                        infoLength = Math.Max(infoLength, GetLength(line))
                    Next
                    For i = 0 To authorsInfo.Length - 1
                        While GetLength(authorsInfo(i)) < infoLength
                            authorsInfo(i) += " "
                        End While
                        Console.WriteLine("█" & authorsInfo(i) & "█")
                    Next
                Catch __unusedException1__ As Exception
                End Try
                ResetConsoleColor()
#End Region
                cmdTimer.Interval = WSBASE.Config.CMDInterval
                AddHandler cmdTimer.Elapsed, AddressOf CmdTimer_Elapsed
#If Not DEBUG Then
#Region "EULA"
                Try
                    Dim height As Integer = Nothing
                    Dim width As Integer = Nothing
                    Dim title As String = Nothing
                    Try
                        height = System.Console.WindowHeight : width = System.Console.WindowWidth : title = System.Console.Title   'set
                    Catch : End Try
                    Dispatcher.CurrentDispatcher.Invoke(Sub()
                                                            Try
                                                                Dim eulaPath = Path.GetDirectoryName(WSBASE.ConfigPath) & "\EULA"
                                                                Dim version As String = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                                                                Dim eulaINFO As JObject = New JObject From {New JProperty("author", "gxh"), New JProperty("version", version), New JProperty("device", WSTools.SFingerPrint())}
                                                                Try
                                                                    If File.Exists(eulaPath) Then
                                                                        If Encoding.UTF32.GetString(File.ReadAllBytes(eulaPath)) <> WSTools.GetMD5(WSTools.StringToUnicode(eulaINFO.ToString())) Then
                                                                            WriteLineERR("EULA", "使用条款需要更新!")
                                                                            File.Delete(eulaPath)
                                                                            Throw New Exception()
                                                                        End If
                                                                    Else
                                                                        Throw New Exception()
                                                                    End If
                                                                Catch __unusedException1__ As Exception
                                                                    Try
                                                                        System.Console.Beep()
                                                                        System.Console.SetWindowSize(System.Console.WindowWidth, 3) : System.Console.Title = "当前控制台会无法操作，请同意使用条款即可恢复"
                                                                    Catch : End Try
                                                                    WriteLine("请同意使用条款")
                                                                    Using dialog As TaskDialog = New TaskDialog()
                                                                        dialog.WindowTitle = "接受食用条款"
                                                                        dialog.MainInstruction = "假装下面是本插件的食用条款"
                                                                        dialog.Content = "1.请在遵守CSRunner前置使用协议的前提下使用本插件" & Microsoft.VisualBasic.Constants.vbLf & "2.不保证本插件不会影响服务器正常运行，如使用本插件造成服务端奔溃等问题，均与作者无瓜" & Microsoft.VisualBasic.Constants.vbLf & "3.严厉打击插件倒卖等行为，共同维护良好的开源环境"
                                                                        dialog.ExpandedInformation = "点开淦嘛,没东西[doge]"
                                                                        dialog.Footer = "本插件 <a href=""https://github.com/littlegao233/PFWebsocketAPI"">GitHub开源地址</a>."
                                                                        AddHandler dialog.HyperlinkClicked, New EventHandler(Of HyperlinkClickedEventArgs)(Sub(sender, e) Process.Start("https://github.com/littlegao233/PFWebsocketAPI"))
                                                                        dialog.FooterIcon = TaskDialogIcon.Information
                                                                        dialog.EnableHyperlinks = True
                                                                        Dim acceptButton As TaskDialogButton = New TaskDialogButton("Accept")
                                                                        dialog.Buttons.Add(acceptButton)
                                                                        Dim refuseButton As TaskDialogButton = New TaskDialogButton("拒绝并关闭本插件")
                                                                        dialog.Buttons.Add(refuseButton)
                                                                        If dialog.ShowDialog() Is refuseButton Then Throw New Exception("---尚未接受食用条款，本插件加载失败---")
                                                                    End Using
                                                                    File.WriteAllBytes(eulaPath, Encoding.UTF32.GetBytes(WSTools.GetMD5(WSTools.StringToUnicode(eulaINFO.ToString()))))
                                                                End Try
                                                            Catch err As Exception
                                                                WriteLineERR("条款获取出错", err)
                                                            End Try
                                                        End Sub)
                    Try
                        If title IsNot Nothing Then
                            System.Console.Title = title : System.Console.SetWindowSize(width, height)  'recover
                        End If
                    Catch : End Try
                Catch ex As Exception
                    Throw
                End Try
#End Region
#End If
#End Region
                WSACT.Start(AddressOf PackOperation.ReadPackInitial)
#Region "注册各类监听"
                If WSBASE.Config.PlayerJoinCallback Then
                    api.addAfterActListener(EventKey.onLoadName, Function(eventraw)
                                                                     Try
                                                                         Dim e = TryCast(BaseEvent.getFrom(eventraw), LoadNameEvent)
                                                                         Task.Run(Sub()
                                                                                      Try
                                                                                          Dim sendData = New CauseJoin(e.playername, e.xuid, e.uuid, GetPlayerIP(e.playerPtr, True)).ToString
                                                                                          If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                                                                                          SendToAll(sendData)
                                                                                      Catch err As Exception
                                                                                          WriteLineERR("PlayerJoinCallback", err)
                                                                                      End Try
                                                                                  End Sub)
                                                                         Return True
                                                                     Finally
                                                                     End Try
                                                                 End Function)
                    WriteLine("已开启PlayerJoinCallback监听")
                End If
                If WSBASE.Config.PlayerLeftCallback Then
                    api.addAfterActListener(EventKey.onPlayerLeft, Function(eventraw)
                                                                       Try
                                                                           Dim e = TryCast(BaseEvent.getFrom(eventraw), PlayerLeftEvent)
                                                                           Task.Run(Sub()
                                                                                        Try
                                                                                            Dim sendData = New CauseLeft(e.playername, e.xuid, e.uuid, GetPlayerIP(e.playerPtr)).ToString
                                                                                            If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                                                                                            SendToAll(sendData)
                                                                                        Catch err As Exception
                                                                                            WriteLineERR("PlayerLeftCallback", err)
                                                                                        End Try
                                                                                    End Sub)
                                                                           Return True
                                                                       Finally
                                                                       End Try
                                                                   End Function)
                    WriteLine("已开启PlayerLeftCallback监听")
                End If
                If WSBASE.Config.PlayerCmdCallback Then
                    api.addAfterActListener(EventKey.onInputCommand, Function(eventraw)
                                                                         Try
                                                                             Dim e = TryCast(BaseEvent.getFrom(eventraw), InputCommandEvent)
                                                                             Task.Run(Sub()
                                                                                          Try
                                                                                              Dim sendData = New CauseCmd(e.playername, e.cmd).ToString
                                                                                              If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                                                                                              SendToAll(sendData)
                                                                                          Catch err As Exception
                                                                                              WriteLineERR("PlayerCmdCallback", err)
                                                                                          End Try
                                                                                      End Sub)
                                                                             Return True
                                                                         Finally
                                                                         End Try
                                                                     End Function)
                    WriteLine("已开启PlayerCmdCallback监听")
                End If
                If WSBASE.Config.PlayerMessageCallback Then
                    api.addAfterActListener(EventKey.onInputText, Function(eventraw)
                                                                      Try
                                                                          Dim e = TryCast(BaseEvent.getFrom(eventraw), InputTextEvent)
                                                                          Task.Run(Sub()
                                                                                       Try
                                                                                           Dim sendData = New CauseChat(e.playername, e.msg).ToString
                                                                                           If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                                                                                           SendToAll(sendData)
                                                                                       Catch err As Exception
                                                                                           WriteLineERR("PlayerMessageCallback", err)
                                                                                       End Try
                                                                                   End Sub)
                                                                          Return True
                                                                      Finally
                                                                      End Try
                                                                  End Function)
                    WriteLine("已开启PlayerMessageCallback监听")
#End Region
                End If
            Catch err As Exception
                WriteLineERR("插件遇到严重错误，无法继续运行", err.Message)
            End Try
        End Sub
    End Module

End Namespace
