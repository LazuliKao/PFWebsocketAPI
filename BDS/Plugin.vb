' 
'  由SharpDevelop创建。
'  用户： BDSNetRunner
'  日期: 2020/7/18
'  时间: 12:32
'  
'  要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
' 
Imports System

Namespace CSR
    Partial Class Plugin
        Private Shared mapi As MCCSAPI = Nothing
        ''' <summary>
        ''' 静态api对象
        ''' </summary>
        Public Shared ReadOnly Property api As MCCSAPI
            Get
                Return mapi
            End Get
        End Property
#Region "插件统一调用接口，请勿随意更改"
        Public Shared Function onServerStart(ByVal pathandversion As String) As Integer
            Dim path As String = Nothing, version As String = Nothing
            Dim commercial = False
            Dim pav = pathandversion.Split(","c)

            If pav.Length > 1 Then
                path = pav(0)
                version = pav(1)
                commercial = Equals(pav(pav.Length - 1), "1")
                mapi = New MCCSAPI(path, version, commercial)

                If mapi IsNot Nothing Then
                    onStart(mapi)
                    GC.KeepAlive(mapi)
                    Return 0
                End If
            End If

            Console.WriteLine("Load failed.")
            Return -1
        End Function
#End Region

        Protected Overrides Sub Finalize()
            'Console.WriteLine("[CSR Plugin] Ref released.");
        End Sub

#Region "必要接口 onStart ，由用户实现"
        ' public static void onStart(MCCSAPI api)
#End Region
    End Class
End Namespace
