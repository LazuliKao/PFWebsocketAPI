Imports System
Imports System.Threading

Namespace PFWebsocketBase
    Public Module WSACT
#Region "Console"
        Public defaultForegroundColor As ConsoleColor = ConsoleColor.White
        Public defaultBackgroundColor As ConsoleColor = ConsoleColor.Black

        Friend Sub ResetConsoleColor()
            Console.ForegroundColor = defaultForegroundColor
            Console.BackgroundColor = defaultBackgroundColor
        End Sub

        Public WritingAvaliable As Boolean = False

        Friend Sub WriteLine(ByVal type As Object, ByVal content As Object)
            For i = 0 To 250
                If WritingAvaliable Then Exit For
                Thread.Sleep(4)
            Next
            WritingAvaliable = False
            Console.Write($"[{Date.Now:yyyy-MM-dd HH:mm:ss} ")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.Write("PFWS")
            Console.ForegroundColor = defaultForegroundColor
            Console.Write("]")
            Console.ForegroundColor = ConsoleColor.Cyan
            Console.Write("[WSBASE]")
            Console.ForegroundColor = ConsoleColor.Green
            Console.Write("[" & type.ToString().ToUpper() & "] ")
            ResetConsoleColor()
            Console.WriteLine(content)
            WritingAvaliable = True
        End Sub
        Friend Sub WriteLine(ByVal content As Object)
            For i = 0 To 250
                If WritingAvaliable Then Exit For
                Thread.Sleep(4)
            Next
            WritingAvaliable = False
            Console.Write($"[{Date.Now:yyyy-MM-dd HH:mm:ss} ")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.Write("PFWS")
            Console.ForegroundColor = defaultForegroundColor
            Console.Write("]")
            Console.ForegroundColor = ConsoleColor.Cyan
            Console.Write("[WSBASE] ")
            ResetConsoleColor()
            Console.WriteLine(content)
            WritingAvaliable = True
        End Sub
        Public Sub WriteLineERR(ByVal type As Object, ByVal content As Object)
            For i = 0 To 250
                If WritingAvaliable Then Exit For
                Thread.Sleep(4)
            Next
            WritingAvaliable = False
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
            WritingAvaliable = True
        End Sub
#End Region
        Public Sub SendToAll(ByVal sendMsg As String)
            For i = 0 To wsConnections.Count - 1
                Try
                    For index = 1 To 200
                        If wsConnections(i).avaliable Then
                            wsConnections(i).avaliable = False
                            Exit For
                        End If
                        Thread.Sleep(50)
                    Next
                    wsConnections(i).connection.Send(sendMsg)
                    wsConnections(i).avaliable = True
                    If Not Config.QuietConsole Then WriteLine("Send", sendMsg)
                Catch ex As Exception
                    WriteLineERR($"Server发送到Client({wsConnections(i).connection.ConnectionInfo.Id})失败", ex)
                End Try
            Next
        End Sub
        Public Sub SendToCon(con As Object, sendMsg As String)
            Dim TargetConnection = wsConnections.Find(Function(l) l.connection Is con)
            Try
                For index = 1 To 200
                    If TargetConnection.avaliable Then
                        TargetConnection.avaliable = False
                        Exit For
                    End If
                    Thread.Sleep(50)
                Next
                TargetConnection.connection.Send(sendMsg)
                TargetConnection.avaliable = True
                If Not Config.QuietConsole Then WriteLine("Send", sendMsg)
            Catch ex As Exception
                WriteLineERR($"Server发送到Client({CType(con, Fleck.IWebSocketConnection).ConnectionInfo.Id})失败", ex)
            End Try
        End Sub
        Public Sub Start(ByVal OnMessage As Action(Of String, Object))
            If Config.EnableDebugOutput Then
                Fleck.FleckLog.LogAction = Sub(level, msg, ex)
                                               If level = Fleck.LogLevel.Error Then
                                                   WriteLineERR(msg, ex.ToString())
                                               Else
                                                   WriteLine(level, msg)
                                               End If
                                           End Sub
            Else
                Fleck.FleckLog.LogAction = Sub(level, msg, ex)
                                               If level = Fleck.LogLevel.Debug Then Return
                                               If level = Fleck.LogLevel.Error Then
                                                   WriteLineERR(msg, ex.ToString())
                                               Else
                                                   WriteLine(level, msg)
                                               End If
                                           End Sub
            End If
            Server.RestartAfterListenError = True
            Server.Start(Sub(connection)
                             connection.OnMessage =
                             Sub(msg)
                                 OnMessage.Invoke(msg, connection)
                                 If Not Config.QuietConsole Then WriteLine("receive", msg)
                             End Sub
                             connection.OnOpen =
                             Sub()
                                 wsConnections.Add(New WebsocketConnection(connection))
                                 WriteLine($"与{connection.ConnectionInfo.Id}({connection.ConnectionInfo.ClientIpAddress}:{connection.ConnectionInfo.ClientPort})建立连接")
                             End Sub
                             connection.OnClose =
                             Sub()
                                 wsConnections.Remove(New WebsocketConnection(connection))
                                 WriteLine($"与{connection.ConnectionInfo.Id}({connection.ConnectionInfo.ClientIpAddress}:{connection.ConnectionInfo.ClientPort})断开连接")
                             End Sub
                             'connection.OnError =
                             'Sub([error]) 
                             'End Sub
                         End Sub)
        End Sub
    End Module
End Namespace
