
Imports MaterialDesignExtensions.Controls
Imports Ookii.Dialogs.Wpf

Partial Public Class Setup
    Private Sub CompleteButton_Click(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub MaterialWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        Using dialog As TaskDialog = New TaskDialog()
            dialog.WindowTitle = "请配置好后点击""输入完成"""
            dialog.MainInstruction = "请配置好后点击""输入完成"""
            dialog.Content = " 请配置好后点击确定，本窗体下次使用不再弹出，否则你需要手动修改配置文件(或删除配置文件即可再次弹出本窗体)"
            dialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks
            Dim continueButton As TaskDialogButton = New TaskDialogButton("继续配置") With {
                .CommandLinkNote = "配置好后即可使用" + vbCrLf}
            Dim cancelButton As TaskDialogButton = New TaskDialogButton("确认关闭快速配置窗口") With {
                .CommandLinkNote = "你需要手动修改配置文件(或删除配置文件即可再次弹出本窗体)"}
            dialog.Buttons.Add(continueButton)
            dialog.Buttons.Add(cancelButton)
            If dialog.ShowDialog(Me) IsNot cancelButton Then
                e.Cancel = True
            End If
        End Using
    End Sub
End Class
