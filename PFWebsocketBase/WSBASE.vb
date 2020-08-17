Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Fleck
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace PFWebsocketBase
    Public Module WSBASE
        Public wsConnections As List(Of IWebSocketConnection) = New List(Of IWebSocketConnection)()
        Private ServerData As WebSocketServer = Nothing
        Public ReadOnly Property Server As WebSocketServer
            Get
                If ServerData Is Nothing Then ServerData = New WebSocketServer("ws://0.0.0.0:" & Config.Port & If(String.IsNullOrEmpty(Config.EndPoint), "", "/" & Config.EndPoint))
                Return ServerData
            End Get
        End Property
        Private ReadOnly Property ConfigPath As String
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
                        WSBASE.Config = New ConfigModel()
                        WriteLine("输出默认配置 >> " & configData.ToString())
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
                File.WriteAllText(ConfigPath, configData.ToString())
                WriteLine("写入配置文件 >> ""plugins\PFWebsocket\config.json""")
            End Set
        End Property
        Public Class ConfigModel
            Public Port As String = "29132", EndPoint As String = "mcws", Password As String = "pwd"
            Public CMDTimeout As Double = 3000
            Public CMDInterval As Double = 200
#If DEBUG Then
            Public EnableDebugOutput As Boolean = True
#Else
             Public EnableDebugOutput As Boolean = false
#End If
            Public PlayerLeftCallback As Boolean = True
            Public PlayerJoinCallback As Boolean = True
            Public PlayerMessageCallback As Boolean = True
            Public PlayerCmdCallback As Boolean = True
            Public Overrides Function ToString() As String
                Return JObject.FromObject(Me).ToString(Formatting.None)
            End Function
        End Class
    End Module
End Namespace
