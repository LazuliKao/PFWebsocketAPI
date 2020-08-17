Imports System.Security
Imports System.Text

Namespace PFWebsocketBase
    Public Module WSTools
        Public Function GetMD5(ByVal sDataIn As String) As String
            Dim md5 As Cryptography.MD5CryptoServiceProvider = New Cryptography.MD5CryptoServiceProvider()
            Dim bytValue, bytHash As Byte()
            bytValue = Encoding.UTF8.GetBytes(sDataIn)
            bytHash = md5.ComputeHash(bytValue)
            md5.Clear()
            Dim sTemp = ""
            For i = 0 To bytHash.Length - 1
                sTemp += bytHash(i).ToString("X").PadLeft(2, "0"c)
            Next
            Return sTemp.ToUpper()
        End Function
    End Module
End Namespace
