Imports System.Net
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

        ''' <summary>
        ''' 地址标识
        ''' </summary>
        ''' <returns></returns>
        Public Function SFingerPrint() As String
            Dim BaseText As String = ""
            For Each address In Dns.GetHostEntry(Dns.GetHostName()).AddressList
                BaseText += address.ToString()
            Next
            Return GetMD5(BaseText)
        End Function
        ''' <summary>
        ''' 字符串转UNICODE代码
        ''' </summary>
        ''' <param name="String"></param>
        ''' <returns></returns>
        Public Function StringToUnicode(ByVal s As String) As String '字符串转UNICODE代码
            Dim charbuffers As Char() = s.ToCharArray()
            Dim buffer As Byte()
            Dim sb As StringBuilder = New StringBuilder()
            For i = 0 To charbuffers.Length - 1
                buffer = Encoding.Unicode.GetBytes(charbuffers(i).ToString())
                sb.Append(String.Format("\u{0:X2}{1:X2}", buffer(1), buffer(0)))
            Next
            Return sb.ToString()
        End Function
    End Module
End Namespace
