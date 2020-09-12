Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading
Imports Fleck
Imports Microsoft.VisualBasic
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports MaterialDesignExtensions.Controls
Imports PFWebsocketWindows

Namespace PFWebsocketBase
    Public Module WSBASE
        Public wsConnections As List(Of WebsocketConnection) = New List(Of WebsocketConnection)()
        Public Class WebsocketConnection
            Public connection As IWebSocketConnection
            Public avaliable As Boolean = True
            Public Sub New(con As IWebSocketConnection)
                connection = con
            End Sub
        End Class
        Private ServerData As WebSocketServer = Nothing
        Public ReadOnly Property Server As WebSocketServer
            Get
                If ServerData Is Nothing Then ServerData = New WebSocketServer("ws://0.0.0.0:" & Config.Port & If(String.IsNullOrEmpty(Config.EndPoint), "", "/" & Config.EndPoint))
                Return ServerData
            End Get
        End Property
        Public ReadOnly Property ConfigPath As String
            Get
                Return Environment.CurrentDirectory & "\plugins\PFWebsocket\config.json"
            End Get
        End Property
        Private configData As ConfigModel = Nothing
        Public Property Config As ConfigModel
            Get
                If configData Is Nothing Then
                    If File.Exists(ConfigPath) Then
                        configData = JObject.Parse(File.ReadAllText(ConfigPath)).ToObject(Of ConfigModel)()
                        WriteLine("读取配置文件 << ""plugins\PFWebsocket\config.json""")
                        WriteLine("配置文件内容 >> " & configData.ToString())
                    Else
                        Try
                            Dim waitForComplete = New AutoResetEvent(False)
                            Dim uithread As Thread = New Thread(Sub()
                                                                    Try
                                                                        Dim dialog = New PFWebsocketWindows.Setup
                                                                        WriteLine("████████████████████")
                                                                        WriteLine("█请在弹出窗口中输入Websocket配置信息 █")
                                                                        WriteLine("████████████████████")
                                                                        dialog.ShowDialog()
                                                                        waitForComplete.Set()
                                                                    Catch ex As Exception
                                                                        WriteLineERR("uithread", ex)
                                                                    End Try
                                                                End Sub)
                            uithread.SetApartmentState(ApartmentState.STA)
                            uithread.Start()
                            waitForComplete.WaitOne()
                            If Setup.configSuccess Then
                                WSBASE.Config = Setup.ConfigData
                                WriteLine("输出生成配置 >> " & configData.ToString())
                            Else
                                WSBASE.Config = New ConfigModel()
                                WriteLine("输出默认配置 >> " & configData.ToString())
                            End If

                            'Dim windowShowing = False
                            'Dim dialog As PFWebsocketWindows.Setup = New PFWebsocketWindows.Setup
                            'dialog.waitForComplete = New AutoResetEvent(False)
                            'Dim uithread As Thread = New Thread(Sub()
                            '                                        Try
                            '                                            WriteLine("████████████████████")
                            '                                            WriteLine("█请在弹出窗口中输入Websocket配置信息 █")
                            '                                            WriteLine("████████████████████")
                            '                                            dialog.ShowDialog()
                            '                                            dialog.waitForComplete.Set()
                            '                                        Catch ex As Exception
                            '                                            WriteLineERR("uithread", ex)
                            '                                        End Try
                            '                                    End Sub)
                            'uithread.SetApartmentState(ApartmentState.STA)
                            'uithread.Start()
                            'dialog.waitForComplete.WaitOne()
                            'If dialog.configSuccess Then
                            '    WSBASE.Config = dialog.ConfigData
                            '    WriteLine("输出生成配置 >> " & configData.ToString())
                            'Else
                            '    WSBASE.Config = New ConfigModel()
                            '    WriteLine("输出默认配置 >> " & configData.ToString())
                            'End If
                        Catch ex As Exception
                            WriteLineERR("uithread-create", ex)
                        End Try
                        'uithread.Start(Sub()
                        '                   Dim dialog As PFWebsocketWindows.Setup = New PFWebsocketWindows.Setup
                        '                   dialog.ShowDialog()
                        '               End Sub)
                    End If
                End If
                Return configData
            End Get
            Set(ByVal value As ConfigModel)
                configData = value
                If Not Directory.Exists(Path.GetDirectoryName(ConfigPath)) Then
                    Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath))
                    WriteLine("创建插件目录 >> ""plugins\PFWebsocket\")
                End If
                File.WriteAllText(ConfigPath, configData.ToString(Formatting.Indented))
                WriteLine("写入配置文件 >> ""plugins\PFWebsocket\config.json""")
            End Set
        End Property
    End Module
End Namespace
