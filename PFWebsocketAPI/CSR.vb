
REM 注意工程根命名空间为CSR，保证接口可被调用
Namespace Global.CSR
    Public Class Plugin
        Shared mapi As MCCSAPI
#Region "初始化通用接口，请勿随意更改"
        Public Shared Function onServerStart(pathandversion As String) As Int32
            Dim path As String, Version As String
            Dim commercial = False
            Dim pav() = pathandversion.Split(",".ToCharArray())
            If pav.Length > 1 Then
                path = pav.GetValue(0).ToString()
                Version = pav.GetValue(1).ToString()
                commercial = pav.GetValue(pav.Length - 1).Equals("1")
                mapi = New MCCSAPI(path, Version, commercial)
                If Not mapi Is Nothing Then
                    onStart(mapi)
                    GC.KeepAlive(mapi)
                    Return 0
                End If
            End If
            Console.WriteLine("Load failed.")
            Return -1
        End Function
#End Region
        Private Shared Sub onStart(api As MCCSAPI)
            REM TODO  此处需要自行实现
            PFWebsocketAPI.PFWebsocketAPI.Program.Init(api)
        End Sub
    End Class
End Namespace