Imports System
Imports System.Collections
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Collections.Generic
Imports CSR

Namespace PFWebsocketAPI
    ''' <summary>
    ''' 此类为hook调用示范类，步骤如下：<br/>
    ''' 1. 定义目标函数原型的托管；<br/>
    ''' 2. 创建函数原型存储指针；<br/>
    ''' 3. 构造新方法，用以替代目标函数；<br/>
    ''' 4. 调用hook，将位于指定RVA位置的目标函数替换为新方法调用。<br/>
    ''' 其它方法可仿照此类进行构造，不再赘述。
    ''' </summary>
    Public Module THook
        Public RVAs As Dictionary(Of String, ArrayList) = New Dictionary(Of String, ArrayList)()
        Private mcapi As MCCSAPI = Nothing
        Friend Delegate Function CMD_REG_Func(ByVal a1 As ULong, ByVal a2 As ULong, ByVal a3 As ULong, ByVal level As Byte, ByVal f1 As Byte, ByVal f2 As Byte) As ULong
        Private _CS_REGISTERCOMMAND_org = IntPtr.Zero
        ''' <summary>
        ''' 新方法，修改注册的命令标识，所有指令全部增加一个无需作弊可用的flag
        ''' </summary>
        ''' <paramname="a1">CommandRegistry句柄</param>
        ''' <paramname="a2">指令原型</param>
        ''' <paramname="a3">指令描述</param>
        ''' <paramname="level">需求op等级</param>
        ''' <paramname="f1">指令属性flag1</param>
        ''' <paramname="f2">指令属性flag2</param>
        ''' <returns></returns>
        Private ReadOnly cs_reghookptr As CMD_REG_Func = Function(ByVal a1, ByVal a2, ByVal a3, ByVal level, ByVal f1, ByVal f2)
                                                             f1 = f1 Or MCCSAPI.CommandCheatFlag.NotCheat
                                                             Dim org = TryCast(Marshal.GetDelegateForFunctionPointer(_CS_REGISTERCOMMAND_org, GetType(CMD_REG_Func)), CMD_REG_Func)
                                                             Return org(a1, a2, a3, level, f1, f2)
                                                         End Function

        Friend Delegate Function ONCREATEPLAYER_Func(ByVal a1 As ULong, ByVal a2 As IntPtr) As ULong
        Private _CS_ONCREATEPLAYER_org = IntPtr.Zero
        ''' <summary>
        ''' 新方法，监听第一个玩家进入游戏，并设置玩家的OP等级
        ''' </summary>
        ''' <paramname="a1">ServerScoreboard句柄</param>
        ''' <paramname="player">player指针</param>
        ''' <returns></returns>
        Private ReadOnly cs_crthookptr As ONCREATEPLAYER_Func = Function(ByVal a1, ByVal player)
                                                                    Console.WriteLine("[c# hook] A player is join.")
                                                                    setPermission(mcapi, player, 3) ' 若参数为3，则op模式可使用kick
                                                                    Dim t As Thread = New Thread(AddressOf releasehook)         ' 延时卸载钩子，也可于当前线程末尾时机进行卸载，也可不卸载
                                                                    t.Start()
                                                                    Dim org As ONCREATEPLAYER_Func = TryCast(Marshal.GetDelegateForFunctionPointer(_CS_ONCREATEPLAYER_org, GetType(ONCREATEPLAYER_Func)), ONCREATEPLAYER_Func)
                                                                    Return org(a1, player)
                                                                End Function

        ' 初始化hook
        Public Sub init(ByVal api As MCCSAPI)
            mcapi = api
            ' 初始化RVA，或可远程获取
            Dim al As ArrayList = New ArrayList(New Integer() {&H00B9D4C0, &H00429850, &H004ECFD0})
            RVAs("1.16.1.2") = al
            Dim a2 As ArrayList = New ArrayList(New Integer() {&H00B9D100, &H00429820, &H004ECFA0})
            RVAs("1.16.10.2") = a2
            Dim a3 As ArrayList = New ArrayList(New Integer() {&H00BA3560, &H0042D250, &H004F0920})
            RVAs("1.16.20.3") = a3

            Try
                Dim rval As ArrayList = Nothing

                If RVAs.TryGetValue(api.VERSION, rval) Then
                    If rval IsNot Nothing AndAlso rval.Count > 0 Then
                        Dim tmpCrtorg = IntPtr.Zero
                        api.cshook(rval(0), Marshal.GetFunctionPointerForDelegate(cs_crthookptr), tmpCrtorg)    ' IDA ServerScoreboard::onPlayerJoined
                        _CS_ONCREATEPLAYER_org = tmpCrtorg
                        tmpCrtorg = IntPtr.Zero
                        api.cshook(rval(1), Marshal.GetFunctionPointerForDelegate(cs_reghookptr), tmpCrtorg)    ' IDA CommandRegistry::registerCommand
                        _CS_REGISTERCOMMAND_org = tmpCrtorg
                    End If
                End If

            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try
        End Sub

        ' 休眠1s后卸载钩子
        Private Sub releasehook()
            Thread.Sleep(1000)

            If mcapi IsNot Nothing Then
                Dim unhooked = mcapi.csunhook(Marshal.GetFunctionPointerForDelegate(cs_crthookptr), _CS_ONCREATEPLAYER_org)

                If unhooked Then
                    Console.WriteLine("[C# unhook] release hook join listen.")
                End If
            End If
        End Sub
    End Module

    ''' <summary>
    ''' 此类为symcall示范类，步骤如下：<br/>
    ''' 1. 创建函数原型；<br/>
    ''' 2. 获取目标RVA位置处的函数指针；<br/>
    ''' 3. 函数调用
    ''' </summary>
    Public Module Symcall
        Friend Delegate Sub SETPERMISSION_FUNC(ByVal p As IntPtr, ByVal per As Byte)
        ''' <summary>
        ''' SYMCALL方式设置玩家OP等级
        ''' </summary>
        ''' <paramname="api"></param>
        ''' <paramname="player">ServerPlayer指针</param>
        ''' <paramname="per">等级</param>
        Public Sub setPermission(ByVal api As MCCSAPI, ByVal player As IntPtr, ByVal per As Byte)
            Console.WriteLine("[CS] setPlayer OP level to {0}", per)
            Dim org = api.dlsym(RVAs(api.VERSION)(2))   ' IDA ServerPlayer::setPermissions
            Dim func = CType(Marshal.GetDelegateForFunctionPointer(org, GetType(SETPERMISSION_FUNC)), SETPERMISSION_FUNC)
            func(player, per)
        End Sub
    End Module
End Namespace
