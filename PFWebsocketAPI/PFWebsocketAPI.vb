Imports CSR
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Ookii.Dialogs.Wpf
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
Imports Timer = System.Timers.Timer

Namespace PFWebsocketAPI
    Friend Class Program
#Region "Console"
        Private Shared defaultForegroundColor As ConsoleColor = ConsoleColor.White
        Private Shared defaultBackgroundColor As ConsoleColor = ConsoleColor.Black

        Private Shared Sub ResetConsoleColor()
            Console.ForegroundColor = defaultForegroundColor
            Console.BackgroundColor = defaultBackgroundColor
        End Sub

        Public Shared Sub WriteLine(ByVal content As Object)
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

        Public Shared Sub WriteLineERR(ByVal type As Object, ByVal content As Object)
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
        Public Shared cmdQueue As Queue(Of WSAPImodel.ExecuteCmdModel) = New Queue(Of WSAPImodel.ExecuteCmdModel)()
        Private Shared cmdTimer As Timer = New Timer() With {
            .AutoReset = True,
            .Enabled = False
        }
        Private Shared CmdCallBack As MCCSAPI.EventCab = Function(e)
                                                             Try

                                                                 If CmdOutput IsNot Nothing Then
                                                                     If Equals(CmdOutput.Result, Nothing) Then
                                                                         CmdOutput.Result = TryCast(BaseEvent.getFrom(e), ServerCmdOutputEvent).output

                                                                         If cmdQueue.Count = 0 Then
                                                                             ListeningOutPut = False
                                                                         End If
                                                                     End If

                                                                     CmdOutput = Nothing
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

        Private Shared CmdOutput As WSAPImodel.ExecuteCmdModel = Nothing
        Private Shared _listeningOutPut As Boolean = False

        Public Shared Property ListeningOutPut As Boolean
            Get
                Return _listeningOutPut
            End Get
            Set(ByVal value As Boolean)

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

        Private Shared Sub CmdTimer_Elapsed(ByVal sender As Object, ByVal ev As ElapsedEventArgs)
            Try

                If CmdOutput Is Nothing Then
                    CmdOutput = cmdQueue.Dequeue()
                    ListeningOutPut = True
                    api.runcmd(CmdOutput.cmd)
                Else
                    If Not ListeningOutPut Then Return

                    If Equals(CmdOutput.Result, Nothing) Then     '指令超时检测
                        CmdOutput.waitTimes += 1

                        If CmdOutput.waitTimes * WSBASE.Config.CMDInterval > WSBASE.Config.CMDTimeout Then
                            CmdOutput.Result = "null"
                            CmdOutput = Nothing

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

        Private Shared api As MCCSAPI = Nothing

        Public Shared Sub Init(ByVal mcapi As MCCSAPI)
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
                    Dim authorsInfo As String() = New String() {"████████████████████████████", "正在裝載PFWebsocketAPI", "作者        gxh2004", "版本信息    v" & Assembly.GetExecutingAssembly().GetName().Version.ToString(), "适用于bds1.16(CSRV0.1.16.20.3v4编译)", "如版本不同可能存在问题", "当前CSRunnerAPI版本:" & api.VERSION, "配置文件位于""[BDS目录]\plugins\PFWebsocket\config.json""", "请修改配置文件后使用，尤其是Password和endpoint", "以免被他人入侵", "████████████████████████████"}
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
#Region "EULA "
                Dim eulaPath = Path.GetDirectoryName(WSBASE.ConfigPath) & "\EULA"
                Dim version As String = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                Dim eulaINFO As JObject = New JObject From {
                    New JProperty("author", "gxh"),
                    New JProperty("version", version),
                    New JProperty("device", WSTools.SFingerPrint())
                }

                Try

                    If File.Exists(eulaPath) Then
                        If Encoding.UTF32.GetString(File.ReadAllBytes(eulaPath)) IsNot WSTools.GetMD5(WSTools.StringToUnicode(eulaINFO.ToString())) Then
                            WriteLineERR("EULA", "使用条款需要更新!")
                            File.Delete(eulaPath)
                            Throw New Exception()
                        End If
                    Else
                        Throw New Exception()
                    End If

                Catch __unusedException1__ As Exception

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
#End Region
#End If
#End Region
                WSACT.Start(Sub(message)
                                Dim receiveData As WSAPImodel.ExecuteCmdModel

                                Try
                                    receiveData = New WSAPImodel.ExecuteCmdModel(JObject.Parse(message))
                                Catch err As Exception
                                    WriteLineERR("收信文本转换失败", err.Message)
                                    Return
                                End Try

                                Try

                                    If receiveData.cmd.StartsWith("op ") OrElse receiveData.cmd.StartsWith("execute") AndAlso receiveData.cmd.IndexOf("op ") <> -1 Then
                                        WriteLine("出于安全考虑，禁止远程执行op命令")
                                        WSACT.SendToAll(receiveData.GetFeedback("出于安全考虑，禁止远程执行op命令"))
                                    ElseIf receiveData.Auth Then '添加到序列
                                        cmdQueue.Enqueue(receiveData)
                                        CmdTimer_Elapsed(cmdTimer, Nothing)
                                        If Not cmdTimer.Enabled Then cmdTimer.Start() '直接返回错误
                                    Else
                                        WriteLine("命令未执行")
                                        WSACT.SendToAll(receiveData.GetFeedback())
                                    End If

                                Catch __unusedException1__ As Exception
                                End Try
                            End Sub)
#Region "注册各类监听"
                If WSBASE.Config.PlayerJoinCallback Then
                    api.addBeforeActListener(EventKey.onLoadName, Function(eventraw)
                                                                      Try
                                                                          Return True
                                                                      Finally
                                                                          Dim e = TryCast(BaseEvent.getFrom(eventraw), LoadNameEvent)
                                                                          Task.Run(Sub()
                                                                                       Try
                                                                                           Dim sendData = New WSAPImodel.SendModel(WSAPImodel.SendType.onjoin, e.playername, e.xuid)
                                                                                           WSACT.SendToAll(sendData.ToString())
                                                                                       Catch err As Exception
                                                                                           WriteLineERR("PlayerJoinCallback", err)
                                                                                       End Try
                                                                                   End Sub)
                                                                      End Try
                                                                  End Function)
                    WriteLine("已开启PlayerJoinCallback监听")
                End If

                If WSBASE.Config.PlayerLeftCallback Then
                    api.addBeforeActListener(EventKey.onPlayerLeft, Function(eventraw)
                                                                        Try
                                                                            Return True
                                                                        Finally
                                                                            Dim e = TryCast(BaseEvent.getFrom(eventraw), PlayerLeftEvent)
                                                                            Task.Run(Sub()
                                                                                         Try
                                                                                             Dim sendData = New WSAPImodel.SendModel(WSAPImodel.SendType.onleft, e.playername, e.xuid)
                                                                                             WSACT.SendToAll(sendData.ToString())
                                                                                         Catch err As Exception
                                                                                             WriteLineERR("PlayerLeftCallback", err)
                                                                                         End Try
                                                                                     End Sub)
                                                                        End Try
                                                                    End Function)
                    WriteLine("已开启PlayerLeftCallback监听")
                End If

                If WSBASE.Config.PlayerCmdCallback Then
                    api.addBeforeActListener(EventKey.onInputCommand, Function(eventraw)
                                                                          Try
                                                                              Return True
                                                                          Finally
                                                                              Dim e = TryCast(BaseEvent.getFrom(eventraw), InputCommandEvent)
                                                                              Task.Run(Sub()
                                                                                           Try
                                                                                               Dim sendData = New WSAPImodel.SendModel(WSAPImodel.SendType.oncmd, e.playername, e.cmd.Substring(1))
                                                                                               WSACT.SendToAll(sendData.ToString())
                                                                                           Catch err As Exception
                                                                                               WriteLineERR("PlayerCmdCallback", err)
                                                                                           End Try
                                                                                       End Sub)
                                                                          End Try
                                                                      End Function)
                    WriteLine("已开启PlayerCmdCallback监听")
                End If

                If WSBASE.Config.PlayerMessageCallback Then
                    api.addBeforeActListener(EventKey.onInputText, Function(eventraw)
                                                                       Try
                                                                           Return True
                                                                       Finally
                                                                           Dim e = TryCast(BaseEvent.getFrom(eventraw), InputTextEvent)
                                                                           Task.Run(Sub()
                                                                                        Try
                                                                                            Dim sendData = New WSAPImodel.SendModel(WSAPImodel.SendType.onmsg, e.playername, e.msg)
                                                                                            WSACT.SendToAll(sendData.ToString())
                                                                                        Catch err As Exception
                                                                                            WriteLineERR("PlayerMessageCallback", err)
                                                                                        End Try
                                                                                    End Sub)
                                                                       End Try
                                                                   End Function)
                    WriteLine("已开启PlayerMessageCallback监听")
#End Region
                End If
            Catch err As Exception
                WriteLineERR("插件遇到严重错误，无法继续运行", err.Message)
            End Try
        End Sub
    End Class

    Friend Class WSAPImodel
        Public Enum SendType
            onmsg
            onjoin
            onleft
            oncmd
        End Enum

        Friend Class SendModel
            Public Sub New(ByVal _type As SendType, ByVal _target As String, ByVal _text As String)
                operate = _type
                target = _target
                text = _text
            End Sub

            Public operate As SendType
            Public target, text As String

            Public Overrides Function ToString() As String
                Dim feedback = JObject.FromObject(Me)
                feedback("operate") = operate.ToString()
                Return feedback.ToString(Formatting.None)
            End Function
        End Class

        Public Enum ReceiveType
            runcmd
        End Enum

        Friend Class ExecuteCmdModel
            Public Sub New(ByVal receive As JObject)
                operate = CType([Enum].Parse(GetType(ReceiveType), receive.Value(Of String)("operate")), ReceiveType)
                cmd = receive.Value(Of String)("cmd").TrimStart()
                msgid = receive.Value(Of String)("msgid")
                Dim token = receive.Value(Of String)("passwd")
                receive("passwd") = ""
                Auth = token Is WSTools.GetMD5(WSBASE.Config.Password & Date.Now.ToString("yyyyMMddHHmm") & "@" & receive.ToString(Formatting.None))
                If Not Auth Then Auth = token Is WSTools.GetMD5(WSBASE.Config.Password & (Date.Now - (New TimeSpan(0, 0, 1, 0))).ToString("yyyyMMddHHmm") & "@" & receive.ToString(Formatting.None))
                If Not Auth Then WSACT.WriteLineERR("密匙不匹配:", "收到密匙:" & token & Microsoft.VisualBasic.Constants.vbTab & "本地密匙:" & WSTools.GetMD5(WSBASE.Config.Password & Date.Now.ToString("yyyyMMddHHmm") & "@" & receive.ToString(Formatting.None)))
            End Sub

            Public Auth As Boolean = False
            Public operate As ReceiveType
            Public cmd, msgid As String
            Private resultField As String = Nothing

            Public Property Result As String
                Get
                    Return resultField
                End Get
                Set(ByVal value As String)
                    resultField = value
                    WSACT.SendToAll(GetFeedback())
                End Set
            End Property

            Public waitTimes As Integer = 0

            Public Function GetFeedback(ByVal text As String) As String
                Result = text
                Return GetFeedback()
            End Function

            Public Function GetFeedback() As String
                Dim feedback As JObject = New JObject From {
                    New JProperty("operate", "runcmd"),
                    New JProperty("Auth", If(Auth, "PasswdMatch", "Failed")),
                    New JProperty("text", If(Auth, Result.TrimEnd(Microsoft.VisualBasic.Strings.ChrW(13), Microsoft.VisualBasic.Strings.ChrW(10), " "c), "Password Not Match")),
                    New JProperty("msgid", msgid)
                }
                Return feedback.ToString(Formatting.None)
            End Function
        End Class
    End Class
End Namespace
