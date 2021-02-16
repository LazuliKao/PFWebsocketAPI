Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports PFWebsocketAPI.PFWebsocketAPI.Model
Imports PFWebsocketBase

Namespace PFWebsocketAPI
    Friend Module IPTool
        Private ReadOnly IPList As New Dictionary(Of IntPtr, String)
        Friend Function GetPlayerIP(ptr As IntPtr, refresh As Boolean)
            If IPList.ContainsKey(ptr) Then
                If refresh Then IPList(ptr) = New CSR.CsPlayer(Program.api, ptr).IpPort
            Else
                Try
                    IPList.Add(ptr, New CSR.CsPlayer(Program.api, ptr).IpPort)
                Catch ex As System.NullReferenceException
                    IPList.Add(ptr, "unknown")
                End Try
            End If
            Return IPList(ptr)
        End Function
        Friend Function GetPlayerIP(ptr As IntPtr)
            Return GetPlayerIP(ptr, False)
        End Function
    End Module
    Friend Module PackOperation
        ''' <summary>
        ''' 读取元数据包
        ''' </summary>
        ''' <param name="message"></param>
        Friend Sub ReadPackInitial(message As String, con As Object)
            Try
                ReadPack(message, True, con)
            Catch ex As JsonReaderException
                Dim sendData = New CauseDecodeFailed("JSON格式错误:" & ex.Message).ToString
                If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                SendToCon(con, sendData)
            Catch ex As Exception
                Dim sendData = New CauseDecodeFailed("解析错误:" & ex.Message).ToString
                If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                SendToCon(con, sendData)
            End Try
        End Sub
        Friend Sub ReadPackNext(message As String, con As Object) '继续解析数据包（如解密后）
            ReadPack(message, False, con) 'False即表示不是第一层级的数据包
        End Sub
        Friend Sub ReadPack(message As String, IsFirstLayer As Boolean, con As Object)
            Dim jobj As JObject = JObject.Parse(message)
            Select Case [Enum].Parse(GetType(PackType), jobj.Value(Of String)("type"), True)'解析type对象，判断基础类型
                Case PackType.encrypted '作为加密包进行解密
                    ReadEncryptedPack(jobj, IsFirstLayer, con)
                Case PackType.pack '作为普通包解析
                    ReadOriginalPack(jobj, IsFirstLayer, con)
            End Select
        End Sub
        Friend Sub ReadEncryptedPack(jobj As JObject, IsFirstLayer As Boolean, con As Object) '读取加密包
            With New EncryptedPack(jobj)
#If DEBUG Then
                WriteLine($"解析{ .params.mode}加密包>元数据:{ .params.raw}")
#End If
                Dim decoded As String
                Try
                    decoded = .Decode(Config.Password)
                Catch ex As Exception
                    Dim fb = New CauseDecodeFailed("密匙验证失败！")
                    WriteLine("密文解密失败，请检查密匙是否正确!")
                    SendToCon(con, fb.ToString) '直接返回
                    Return
                End Try
                ReadPackNext(decoded, con) '嵌套方法
            End With
        End Sub
        Friend Sub ReadOriginalPack(jobj As JObject, IsFirstLayer As Boolean, con As Object) '读取普通包
#If Not DEBUG Then'调试状态允许执行未加密的权限包
            If IsFirstLayer Then '判断初始包，如果是未加密的初始包则不允许执行
                Dim fb = New CauseInvalidRequest("未加密的初始包不予执行！")
                WriteLine("未加密的初始包不予执行!")
                SendToCon(con, fb.ToString) '直接返回
                Return
            End If
#End If
            Select Case [Enum].Parse(GetType(ClientActionType), jobj.Value(Of String)("action"), True)
                Case ClientActionType.runcmdrequest
                    With New ActionRunCmd(jobj, con) '通过加载后的json初始化Action
                        Dim fb = .GetFeedback()
                        If .params.cmd.StartsWith("op ") OrElse .params.cmd.StartsWith("execute") AndAlso .params.cmd.IndexOf("op ") <> -1 Then
                            fb.params.result = "出于安全考虑，禁止远程执行op命令"
                            WriteLine("出于安全考虑，禁止远程执行op命令")
                            Dim sendData = fb.ToString
                            If Config.EncryptDataSent Then sendData = New EncryptedPack(EncryptionMode.aes256, sendData, Config.Password).ToString
                            SendToCon(con, sendData) '直接返回
                        End If
                        cmdQueue.Enqueue(fb)
                        CmdTimer_Elapsed(cmdTimer, Nothing)
                        If Not cmdTimer.Enabled Then cmdTimer.Start()
                    End With
                Case ClientActionType.broadcast
                    '待实现
                Case ClientActionType.tellraw
                    '待实现
            End Select
        End Sub
    End Module
End Namespace








Namespace PFWebsocketAPI.Model
    Public Module StringTools
        Public Function GetMD5(ByVal sDataIn As String) As String
            Dim md5 As New Security.Cryptography.MD5CryptoServiceProvider
            Dim bytValue, bytHash As Byte()
            bytValue = Text.Encoding.UTF8.GetBytes(sDataIn)
            bytHash = md5.ComputeHash(bytValue)
            md5.Clear()
            Dim sTemp = ""
            For i = 0 To bytHash.Length - 1
                sTemp += bytHash(i).ToString("X").PadLeft(2, "0"c)
            Next
            Return sTemp.ToUpper()
        End Function
        Public Function AESEncrypt(content As String, password As String) As String
            Dim md5 = GetMD5(password)
            Dim iv As String = md5.Substring(16)
            Dim key As String = md5.Remove(16)
            Return EasyEncryption.AES.Encrypt(content, key, iv)
        End Function
        Public Function AESDecrypt(content As String, password As String) As String
            Dim md5 = GetMD5(password)
            Dim iv As String = md5.Substring(16)
            Dim key As String = md5.Remove(16)
            Return EasyEncryption.AES.Decrypt(content, key, iv)
        End Function
    End Module
    Public Structure Vec3
        Public x, y, z As Single
    End Structure
    <JsonConverter(GetType(Converters.StringEnumConverter))>
    Public Enum PackType '基本包类型
        pack
        encrypted
    End Enum
    <JsonConverter(GetType(Converters.StringEnumConverter))>
    Public Enum EncryptionMode '加密模式
        aes256
        aes_cbc_pck7padding
    End Enum
    Friend MustInherit Class PackBase '基础类
        <JsonProperty(Order:=-3)>
        Public MustOverride ReadOnly Property type As PackType '包类型
        Public Overrides Function ToString() As String
            Return JsonConvert.SerializeObject(Me) '基类的转化为String重写方法
        End Function
        Public Function GetParams(Of T)(json As JObject) As T
            Return json("params").ToObject(Of T) '基类的获取参数表方法
        End Function
    End Class
    Friend Class EncryptedPack    '加密包
        Inherits PackBase

        Public Overrides ReadOnly Property type As PackType = PackType.encrypted
        Public params As ParamMap
        Friend Sub New(json As JObject) '通过已有json初始化对象（通常用作传入解析）
            params = GetParams(Of ParamMap)(json) '通过基类该方法获取参数表
        End Sub
        Friend Sub New(mode As EncryptionMode, from As String, password As String) '通过参数初始化包（通常用作发送前）
            Dim encrypted As String = ""
            Select Case mode'不同加密模式不同操作
                Case EncryptionMode.aes256
                    encrypted = SimpleAES.AES256.Encrypt(from, password)
                Case EncryptionMode.aes_cbc_pck7padding
                    encrypted = AESEncrypt(from, password)
            End Select
            params = New ParamMap With {.mode = mode, .raw = encrypted}
        End Sub
        Public Function Decode(password As String) As String '解密params.raw中的内容并返回
            Dim decrypted As String = ""
            Select Case params.mode'不同加密模式不同操作
                Case EncryptionMode.aes256
                    decrypted = SimpleAES.AES256.Decrypt(params.raw, password)
                Case EncryptionMode.aes_cbc_pck7padding
                    decrypted = AESDecrypt(params.raw, password)
            End Select
            If String.IsNullOrEmpty(decrypted) Then Throw New Exception("AES256 Decode failed!")
            Return decrypted
        End Function
        Friend Class ParamMap '对象参数表
            Public mode As EncryptionMode
            Public raw As String
        End Class
    End Class
    Friend Class OriginalPack   '普通包/解密后的包
        Inherits PackBase
        Public Overrides ReadOnly Property type As PackType = PackType.pack
    End Class

#Region "Server"
    <JsonConverter(GetType(Converters.StringEnumConverter))>
    Public Enum ServerCauseType
        chat
        join
        left
        cmd
        mobdie
        runcmdfeedback
        decodefailed
        invalidrequest
    End Enum
    Friend MustInherit Class ServerPackBase
        Inherits OriginalPack
        '<JsonProperty("cause")>
        <JsonProperty(Order:=-2)>
        Public MustOverride ReadOnly Property cause As ServerCauseType
    End Class
    Friend Class CauseJoin
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(sender As String, xuid As String, uuid As String, ip As String)
            params = New ParamMap With {.sender = sender, .xuid = xuid, .uuid = uuid, .ip = ip}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.join
        Public params As ParamMap
        Friend Class ParamMap
            Public sender, xuid, uuid, ip As String
        End Class
    End Class
    Friend Class CauseLeft
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(sender As String, xuid As String, uuid As String, ip As String)
            params = New ParamMap With {.sender = sender, .xuid = xuid, .uuid = uuid, .ip = ip}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.left
        Public params As ParamMap
        Friend Class ParamMap
            Public sender, xuid, uuid, ip As String
        End Class
    End Class
    Friend Class CauseChat
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(sender As String, text As String)
            params = New ParamMap With {.sender = sender, .text = text}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.chat
        Public params As ParamMap
        Friend Class ParamMap
            Public sender, text As String
        End Class
    End Class
    Friend Class CauseCmd
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(sender As String, text As String)
            params = New ParamMap With {.sender = sender, .text = text}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.cmd
        Public params As ParamMap
        Friend Class ParamMap
            Public sender, text As String
        End Class
    End Class
    Friend Class CauseMobDie
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(mobtype As String, mobname As String, dmcase As Integer, srctype As String, srcname As String, XYZ As Vec3)
            params = New ParamMap With {.mobtype = mobtype, .mobname = mobname, .dmcase = dmcase, .srctype = srctype, .srcname = srcname, .pos = XYZ}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.mobdie
        Public params As ParamMap
        Friend Class ParamMap
            Public mobtype, mobname As String, dmcase As Integer, srctype, srcname As String, pos As Vec3
        End Class
    End Class
    '命令返回
    Friend Class CauseRuncmdFeedback
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(id As String, cmd As String, result As String, con As Object)
            params = New ParamMap With {.id = id, .cmd = cmd, .result = result, .con = con}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.runcmdfeedback
        Public params As ParamMap
        Friend Class ParamMap
            Public id As String
            Public result As String
            <JsonIgnore>
            Friend cmd As String
            <JsonIgnore>
            Friend waiting As Integer = 0
            <JsonIgnore>
            Friend con As Object
        End Class
    End Class
    Friend Class CauseDecodeFailed
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(msg As String)
            params = New ParamMap With {.msg = msg}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.decodefailed
        Public params As ParamMap
        Friend Class ParamMap
            Public msg As String
        End Class
    End Class
    Friend Class CauseInvalidRequest
        Inherits ServerPackBase
        Friend Sub New(json As JObject)
            params = GetParams(Of ParamMap)(json)
        End Sub
        Friend Sub New(msg As String)
            params = New ParamMap With {.msg = msg}
        End Sub
        Public Overrides ReadOnly Property cause As ServerCauseType = ServerCauseType.invalidrequest
        Public params As ParamMap
        Friend Class ParamMap
            Public msg As String
        End Class
    End Class

#End Region
#Region "Client"
    <JsonConverter(GetType(Converters.StringEnumConverter))>
    Public Enum ClientActionType
        runcmdrequest
        broadcast
        tellraw
    End Enum
    Friend MustInherit Class ClientPackBase
        Inherits OriginalPack
        <JsonProperty(Order:=-2)>
        Public MustOverride ReadOnly Property action As ClientActionType
    End Class
    Friend Class ActionRunCmd
        Inherits ClientPackBase
        'Friend Sub New(json As JObject)
        '    params = GetParams(Of ParamMap)(json)
        'End Sub

        Friend Sub New(json As JObject, con As Object)
            params = GetParams(Of ParamMap)(json)
            params.con = con
        End Sub
        Friend Sub New(cmd As String, id As String, con As Object)
            params = New ParamMap With {.cmd = cmd, .id = id, .con = con}
        End Sub
        Public Overrides ReadOnly Property action As ClientActionType = ClientActionType.runcmdrequest
        Public params As ParamMap
        Friend Class ParamMap
            Public cmd, id As String
            <JsonIgnore>
            Friend con As Object
        End Class
        Friend Function GetFeedback() As CauseRuncmdFeedback
            Return New CauseRuncmdFeedback(params.id, params.cmd, Nothing, params.con)
        End Function
    End Class
#End Region
End Namespace
