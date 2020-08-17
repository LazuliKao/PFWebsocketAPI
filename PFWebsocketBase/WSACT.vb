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
            For Each client In wsConnections
                Try
                    client.Send(sendMsg)
                    WriteLine("Send", sendMsg)
                Catch ex As Exception
                    WriteLineERR($"Server发送到Client({client.ConnectionInfo.Id})失败", ex)
                End Try
            Next
        End Sub
        Public Sub Start(ByVal OnMessage As Action(Of String))
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
                                 OnMessage.Invoke(msg)
                                 WriteLine("receive", msg)
                             End Sub
                             connection.OnOpen =
                             Sub()
                                 wsConnections.Add(connection)
                                 WriteLine($"与{connection.ConnectionInfo.Id}({connection.ConnectionInfo.ClientIpAddress}:{connection.ConnectionInfo.ClientPort})建立连接")
                             End Sub
                             connection.OnClose =
                             Sub()
                                 wsConnections.Remove(connection)
                                 WriteLine($"与{connection.ConnectionInfo.Id}({connection.ConnectionInfo.ClientIpAddress}:{connection.ConnectionInfo.ClientPort})断开连接")
                             End Sub
                             'connection.OnError =
                             'Sub([error]) 
                             'End Sub
                         End Sub)
        End Sub
    End Module
End Namespace
