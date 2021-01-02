
Imports System.Text.RegularExpressions
Imports System.Threading
Imports MaterialDesignExtensions.Controls
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Ookii.Dialogs.Wpf

Partial Public Class Setup
    'Public waitForComplete As AutoResetEvent = Nothing
    Public Shared configSuccess As Boolean = False
    Private Sub SetErr(ByVal msg As String)
        errTip.Text = "填写有误：" & msg
        errTip.Visibility = Visibility.Visible
    End Sub
    Private Sub CompleteButton_Click(sender As Object, e As RoutedEventArgs)
#Region "Main"
        errTip.Visibility = Visibility.Collapsed
        Try
            Dim wstest As Fleck.WebSocketServer = New Fleck.WebSocketServer(GetWsPreview)
        Catch ex As Exception
            SetErr(ex.ToString())
            Return
        End Try
#End Region
        configSuccess = True
        Dim __temp__ = ConfigDataSave
        'If waitForComplete IsNot Nothing Then
        '    waitForComplete.Set()
        'End If
        Close()
    End Sub
    Private Sub MaterialWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If Not configSuccess Then
            Using dialog As TaskDialog = New TaskDialog()
                dialog.WindowTitle = "请配置好后点击""输入完成"""
                dialog.MainInstruction = "请配置好后点击""输入完成"""
                dialog.Content = " 请配置好后点击确定，本窗体下次使用不再弹出，否则你需要手动修改配置文件(或删除配置文件即可再次弹出本窗体)"
                dialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks
                Dim continueButton As TaskDialogButton = New TaskDialogButton("继续配置") With {
                    .CommandLinkNote = "配置好后即可使用"}
                Dim cancelButton As TaskDialogButton = New TaskDialogButton("确认关闭快速配置窗口") With {
                    .CommandLinkNote = "你需要手动修改配置文件(或删除配置文件即可再次弹出本窗体)"}
                dialog.Buttons.Add(continueButton)
                dialog.Buttons.Add(cancelButton)
                If dialog.ShowDialog(Me) IsNot cancelButton Then
                    e.Cancel = True
                End If
            End Using
        End If
    End Sub
    Public ReadOnly Property GetWsPreview As String
        Get
            Return "ws://127.0.0.1:" & MainPort.Text + If(String.IsNullOrWhiteSpace(MainEndPoint.Text), Nothing, "/" & MainEndPoint.Text)
        End Get
    End Property
    Public Shared configData As ConfigModel
    Private ReadOnly Property ConfigDataSave As ConfigModel
        Get
            Try
                configData = New ConfigModel() With {
                                .Port = MainPort.Text,
                                .EndPoint = MainEndPoint.Text,
                                .Password = MainPassword.Text,
                                .CMDTimeout = AdvancedCmdTimeout.Text,
                                .CMDInterval = AdvancedCmdInterval.Text,
                                .QuietConsole = AdvancedQuietConsole.IsChecked = True,
                                .PlayerCmdCallback = AdvancedPlayerCmdCallbackCallback.IsChecked = True,
                                .PlayerJoinCallback = AdvancedPlayerJoinCallback.IsChecked = True,
                                .PlayerLeftCallback = AdvancedPlayerLeftCallback.IsChecked = True,
                                .PlayerMessageCallback = AdvancedPlayerMessageCallback.IsChecked = True,
                                .EnableDebugOutput = AdvancedEnableDebugOutput.IsChecked = True
                            }
            Catch
            End Try
            Return configData
        End Get
    End Property
    Private Sub WsInfo_TextChanged(sender As Object, e As TextChangedEventArgs)
        Try
            MainPreview.Text = GetWsPreview
        Catch
        End Try
    End Sub

    Private Sub TextNumberCheck_TextChanged(sender As TextBox, e As TextChangedEventArgs)
        If Regex.IsMatch(sender.Text, "[^0-9]") Then
            sender.Text = Regex.Replace(sender.Text, "[^0-9]", "")
        End If
        If Int(sender.Text) >= Integer.MaxValue Then
            sender.Text = Integer.MaxValue
        End If
    End Sub
End Class









Public Class ConfigModel
    Public Port As String = "29132", EndPoint As String = "mcws", Password As String = "commandpassword"
    Public CMDTimeout As Double = 1800
    Public CMDInterval As Double = 200
#If DEBUG Then
            Public EnableDebugOutput As Boolean = True
#Else
    Public EnableDebugOutput As Boolean = False
#End If
    Public PlayerLeftCallback As Boolean = True
    Public PlayerJoinCallback As Boolean = True
    Public PlayerMessageCallback As Boolean = True
    Public PlayerCmdCallback As Boolean = True
    Public QuietConsole As Boolean = False
    Public EncryptDataSent As Boolean = False
    Public Overrides Function ToString() As String
        Return JObject.FromObject(Me).ToString(Formatting.None)
    End Function
    Public Overloads Function ToString(formatting As Formatting) As String
        Return JObject.FromObject(Me).ToString(formatting)
    End Function
End Class
