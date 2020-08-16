' 
'  由SharpDevelop创建。
'  用户： BDSNetRunner
'  日期: 2020/7/17
'  时间: 16:27
'  
'  要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
' 
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Namespace CSR
    ''' <summary>
    ''' API接口定义
    ''' </summary>
    Public Class MCCSAPI
        <DllImport("kernel32.dll")>
        Private Shared Function LoadLibrary(ByVal path As String) As IntPtr
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function GetProcAddress(ByVal [lib] As IntPtr, ByVal funcName As String) As IntPtr
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function FreeLibrary(ByVal [lib] As IntPtr) As Boolean
        End Function

        Private ReadOnly mVersion As String
        ''' <summary>
        ''' 插件版本
        ''' </summary>
        Public ReadOnly Property VERSION As String
            Get
                Return mVersion
            End Get
        End Property

        Private ReadOnly mcommercial As Boolean
        ''' <summary>
        ''' 平台类型
        ''' </summary>
        Public ReadOnly Property COMMERCIAL As Boolean
            Get
                Return mcommercial
            End Get
        End Property

        ' 注册事件回调管理，避免回收
        Private callbks As Dictionary(Of String, ArrayList) = New Dictionary(Of String, ArrayList)()
        Private hLib As IntPtr

        Public Sub New(ByVal DLLPath As String, ByVal ver As String, ByVal commercial As Boolean)
            mVersion = ver
            mcommercial = commercial
            hLib = LoadLibrary(DLLPath)

            If hLib <> IntPtr.Zero Then
                initApis()
            End If
        End Sub

        Protected Overrides Sub Finalize()
            FreeLibrary(hLib)
        End Sub
        '将要执行的函数转换为委托
        Private Function Invoke(Of T As [Delegate])(ByVal APIName As String) As T
            Dim api = GetProcAddress(hLib, APIName)
            If api <> IntPtr.Zero Then Return Marshal.GetDelegateForFunctionPointer(api, GetType(T))
            '若.net framework版本高于4.5.1可用以下替换以上
            'return Marshal.GetDelegateForFunctionPointer<T>(api);
            Console.WriteLine("Get Api {0} failed.", APIName)
            Return Nothing
        End Function

        ''' <summary>
        ''' 事件处理函数类型
        ''' </summary>
        ''' <paramname="e">原始数据</param>
        ''' <returns>是否继续/拦截(before事件有效)</returns>
        Public Delegate Function EventCab(ByVal e As Events) As Boolean
        Private Delegate Function ADDACTEVENTFUNC(ByVal key As String, ByVal cb As EventCab) As Boolean
        Private caddBeforeActEvent, caddAfterActEvent, cremoveBeforeAct, cremoveAfterAct As ADDACTEVENTFUNC
        Private Delegate Function CSHOOKFUNC(ByVal rva As Integer, ByVal hook As IntPtr, <Out> ByRef org As IntPtr) As Boolean
        Private ccshook As CSHOOKFUNC
        Private Delegate Function CSUNHOOKFUNC(ByVal hook As IntPtr, <Out> ByRef org As IntPtr) As Boolean
        Private ccsunhook As CSUNHOOKFUNC
        Private Delegate Function DLSYMFUNC(ByVal rva As Integer) As IntPtr
        Private cdlsym As DLSYMFUNC
        Private Delegate Sub SETSHAREPTRFUNC(ByVal key As String, ByVal sdata As IntPtr)
        Private csetshareptr As SETSHAREPTRFUNC
        Private Delegate Function GETSHAREPTRFUNC(ByVal key As String) As IntPtr
        Private cgetSharePtr, cremoveSharePtr As GETSHAREPTRFUNC

        Public Enum CommandPermissionLevel
            Any = 0
            GameMasters = 1
            Admin = 2
            Host = 3
            Owner = 4
            Internal = 5
        End Enum

        Public Enum CommandVisibilityFlag As Byte
            Visible = 0
            HiddenFromCommandBlockOrigin = 2
            HiddenFromPlayerOrigin = 4
            Hidden = 6
        End Enum

        Public Enum CommandUsageFlag As Byte
            Normal = 0
            Test = 1
        End Enum

        Public Enum CommandSyncFlag As Byte
            Synced = 0
            Local = 8
        End Enum

        Public Enum CommandExecuteFlag As Byte
            Allowed = 0
            Disallowed = &H10
        End Enum

        Public Enum CommandTypeFlag As Byte
            None = 0
            Message = &H20
        End Enum

        Public Enum CommandCheatFlag As Byte
            Cheat = 0
            NotCheat = &H40
        End Enum

        Private Delegate Sub SETCOMMANDDESCRIBEFUNC(ByVal key As String, ByVal description As String, ByVal level As CommandPermissionLevel, ByVal flag1 As Byte, ByVal flag2 As Byte)
        Private csetCommandDescribe As SETCOMMANDDESCRIBEFUNC
        Private Delegate Function RUNCMDFUNC(ByVal cmd As String) As Boolean
        Private cruncmd, cremovePlayerBossBar, cremovePlayerSidebar As RUNCMDFUNC
        Private Delegate Sub LOGOUTFUNC(ByVal cmdout As String)
        Private clogout As LOGOUTFUNC
        Private Delegate Function GETONLINEPLAYERSFUNC() As Std_String
        Private cgetOnLinePlayers As GETONLINEPLAYERSFUNC
        Private Delegate Function GETSTRUCTUREFUNC(ByVal did As Integer, ByVal jsonposa As String, ByVal jsonposb As String, ByVal exent As Boolean, ByVal exblk As Boolean) As Std_String
        Private cgetStructure As GETSTRUCTUREFUNC
        Private Delegate Function SETSTRUCTUREFUNC(ByVal jdata As String, ByVal did As Integer, ByVal jsonposa As String, ByVal rot As Byte, ByVal exent As Boolean, ByVal exblk As Boolean) As Boolean
        Private csetStructure As SETSTRUCTUREFUNC
        Private Delegate Function RENAMEBYUUIDFUNC(ByVal uuid As String, ByVal newName As String) As Boolean
        Private creNameByUuid, csetPlayerAbilities, csetPlayerTempAttributes, csetPlayerMaxAttributes, csetPlayerItems, caddPlayerItemEx, csetPlayerEffects, ctalkAs, cruncmdAs, csetPlayerPermissionAndGametype As RENAMEBYUUIDFUNC
        Private Delegate Function GETPLAYERABILITIESFUNC(ByVal uuid As String) As Std_String
        Private cgetPlayerAbilities, cgetPlayerAttributes, cgetPlayerMaxAttributes, cgetPlayerItems, cgetPlayerSelectedItem, cgetPlayerEffects, cselectPlayer, cgetPlayerPermissionAndGametype As GETPLAYERABILITIESFUNC
        Private Delegate Function ADDPLAYERITEMFUNC(ByVal uuid As String, ByVal id As Integer, ByVal aux As Short, ByVal count As Byte) As Boolean
        Private caddPlayerItem As ADDPLAYERITEMFUNC
        Private Delegate Function SETPLAYERBOSSBARFUNC(ByVal uuid As String, ByVal title As String, ByVal percent As Single) As Boolean
        Private csetPlayerBossBar As SETPLAYERBOSSBARFUNC
        Private Delegate Function TRANSFERSERVERFUNC(ByVal uuid As String, ByVal addr As String, ByVal port As Integer) As Boolean
        Private ctransferserver As TRANSFERSERVERFUNC
        Private Delegate Function TELEPORTFUNC(ByVal uuid As String, ByVal x As Single, ByVal y As Single, ByVal z As Single, ByVal did As Integer) As Boolean
        Private cteleport As TELEPORTFUNC
        Private Delegate Function SENDSIMPLEFORMFUNC(ByVal uuid As String, ByVal title As String, ByVal content As String, ByVal buttons As String) As UInteger
        Private csendSimpleForm As SENDSIMPLEFORMFUNC
        Private Delegate Function SENDMODALFORMFUNC(ByVal uuid As String, ByVal title As String, ByVal content As String, ByVal button1 As String, ByVal button2 As String) As UInteger
        Private csendModalForm As SENDMODALFORMFUNC
        Private Delegate Function SENDCUSTOMFORMFUNC(ByVal uuid As String, ByVal json As String) As UInteger
        Private csendCustomForm As SENDCUSTOMFORMFUNC
        Private Delegate Function RELEASEFORMFUNC(ByVal fid As UInteger) As Boolean
        Private creleaseForm As RELEASEFORMFUNC
        Private Delegate Function SETPLAYERSIDEBARFUNC(ByVal uuid As String, ByVal title As String, ByVal list As String) As Boolean
        Private csetPlayerSidebar As SETPLAYERSIDEBARFUNC
        Private Delegate Function GETSCOREBOARDVALUEFUNC(ByVal uuid As String, ByVal objname As String) As Integer
        Private cgetscoreboardValue As GETSCOREBOARDVALUEFUNC
        Private Delegate Function GETEXTRAAPI(ByVal apiname As String) As IntPtr
        Private cgetExtraAPI As GETEXTRAAPI

        ' 转换附加函数指针
        Private Function ConvertExtraFunc(Of T As [Delegate])(ByVal apiname As String) As T
            If cgetExtraAPI IsNot Nothing Then
                Dim f = cgetExtraAPI(apiname)

                If f <> IntPtr.Zero Then
                    Return Marshal.GetDelegateForFunctionPointer(f, GetType(T))
                    '若.net framework版本高于4.5.1可用以下替换以上
                    'return Marshal.GetDelegateForFunctionPointer<T>(f);
                End If
            End If

            Console.WriteLine("Get ExtraApi {0} failed.", apiname)
            Return Nothing
        End Function

        ' 初始化所有api函数
        Private Sub initApis()
            caddBeforeActEvent = Invoke(Of ADDACTEVENTFUNC)("addBeforeActListener")
            caddAfterActEvent = Invoke(Of ADDACTEVENTFUNC)("addAfterActListener")
            cremoveBeforeAct = Invoke(Of ADDACTEVENTFUNC)("removeBeforeActListener")
            cremoveAfterAct = Invoke(Of ADDACTEVENTFUNC)("removeAfterActListener")
            csetshareptr = Invoke(Of SETSHAREPTRFUNC)("setSharePtr")
            cgetSharePtr = Invoke(Of GETSHAREPTRFUNC)("getSharePtr")
            cremoveSharePtr = Invoke(Of GETSHAREPTRFUNC)("removeSharePtr")
            csetCommandDescribe = Invoke(Of SETCOMMANDDESCRIBEFUNC)("setCommandDescribeEx")
            cruncmd = Invoke(Of RUNCMDFUNC)("runcmd")
            clogout = Invoke(Of LOGOUTFUNC)("logout")
            cgetOnLinePlayers = Invoke(Of GETONLINEPLAYERSFUNC)("getOnLinePlayers")
            cgetExtraAPI = Invoke(Of GETEXTRAAPI)("getExtraAPI")
            creNameByUuid = Invoke(Of RENAMEBYUUIDFUNC)("reNameByUuid")
            ctalkAs = Invoke(Of RENAMEBYUUIDFUNC)("talkAs")
            cruncmdAs = Invoke(Of RENAMEBYUUIDFUNC)("runcmdAs")
            csendSimpleForm = Invoke(Of SENDSIMPLEFORMFUNC)("sendSimpleForm")
            csendModalForm = Invoke(Of SENDMODALFORMFUNC)("sendModalForm")
            csendCustomForm = Invoke(Of SENDCUSTOMFORMFUNC)("sendCustomForm")
            creleaseForm = Invoke(Of RELEASEFORMFUNC)("releaseForm")
            cselectPlayer = Invoke(Of GETPLAYERABILITIESFUNC)("selectPlayer")
            caddPlayerItem = Invoke(Of ADDPLAYERITEMFUNC)("addPlayerItem")
            cgetscoreboardValue = Invoke(Of GETSCOREBOARDVALUEFUNC)("getscoreboardValue")
            ccshook = Invoke(Of CSHOOKFUNC)("cshook")
            ccsunhook = Invoke(Of CSUNHOOKFUNC)("csunhook")
            cdlsym = Invoke(Of DLSYMFUNC)("dlsym")

#Region "非社区部分内容"
            If COMMERCIAL Then
                cgetStructure = ConvertExtraFunc(Of GETSTRUCTUREFUNC)("getStructure")
                csetStructure = ConvertExtraFunc(Of SETSTRUCTUREFUNC)("setStructure")
                cgetPlayerAbilities = ConvertExtraFunc(Of GETPLAYERABILITIESFUNC)("getPlayerAbilities")
                csetPlayerAbilities = ConvertExtraFunc(Of RENAMEBYUUIDFUNC)("setPlayerAbilities")
                cgetPlayerAttributes = ConvertExtraFunc(Of GETPLAYERABILITIESFUNC)("getPlayerAttributes")
                csetPlayerTempAttributes = ConvertExtraFunc(Of RENAMEBYUUIDFUNC)("setPlayerTempAttributes")
                cgetPlayerMaxAttributes = ConvertExtraFunc(Of GETPLAYERABILITIESFUNC)("getPlayerMaxAttributes")
                csetPlayerMaxAttributes = ConvertExtraFunc(Of RENAMEBYUUIDFUNC)("setPlayerMaxAttributes")
                cgetPlayerItems = ConvertExtraFunc(Of GETPLAYERABILITIESFUNC)("getPlayerItems")
                csetPlayerItems = ConvertExtraFunc(Of RENAMEBYUUIDFUNC)("setPlayerItems")
                cgetPlayerSelectedItem = ConvertExtraFunc(Of GETPLAYERABILITIESFUNC)("getPlayerSelectedItem")
                caddPlayerItemEx = ConvertExtraFunc(Of RENAMEBYUUIDFUNC)("addPlayerItemEx")
                cgetPlayerEffects = ConvertExtraFunc(Of GETPLAYERABILITIESFUNC)("getPlayerEffects")
                csetPlayerEffects = ConvertExtraFunc(Of RENAMEBYUUIDFUNC)("setPlayerEffects")
                csetPlayerBossBar = ConvertExtraFunc(Of SETPLAYERBOSSBARFUNC)("setPlayerBossBar")
                cremovePlayerBossBar = ConvertExtraFunc(Of RUNCMDFUNC)("removePlayerBossBar")
                ctransferserver = ConvertExtraFunc(Of TRANSFERSERVERFUNC)("transferserver")
                cteleport = ConvertExtraFunc(Of TELEPORTFUNC)("teleport")
                csetPlayerSidebar = ConvertExtraFunc(Of SETPLAYERSIDEBARFUNC)("setPlayerSidebar")
                cremovePlayerSidebar = ConvertExtraFunc(Of RUNCMDFUNC)("removePlayerSidebar")
                cgetPlayerPermissionAndGametype = ConvertExtraFunc(Of GETPLAYERABILITIESFUNC)("getPlayerPermissionAndGametype")
                csetPlayerPermissionAndGametype = ConvertExtraFunc(Of RENAMEBYUUIDFUNC)("setPlayerPermissionAndGametype")
            End If
#End Region
        End Sub

        ' 保管一个事件
        Private Sub addcb(ByVal k As String, ByVal cb As EventCab)
            Dim al As ArrayList

            If callbks.TryGetValue(k, al) Then
                If al IsNot Nothing Then
                    al.Add(cb)
                Else
                    al = New ArrayList()
                    al.Add(cb)
                    callbks(k) = al
                End If
            Else
                al = New ArrayList()
                al.Add(cb)
                callbks(k) = al
            End If
        End Sub
        ' 移除一个事件处理
        Private Sub removecb(ByVal k As String, ByVal cb As EventCab)
            Dim al As ArrayList

            If callbks.TryGetValue(k, al) Then
                If al IsNot Nothing Then al.Remove(cb)
            End If
        End Sub

        ''' <summary>
        ''' 设置事件发生前监听
        ''' </summary>
        ''' <paramname="key"></param>
        ''' <paramname="cb"></param>
        ''' <returns></returns>
        Public Function addBeforeActListener(ByVal key As String, ByVal cb As EventCab) As Boolean
            Dim r = caddBeforeActEvent IsNot Nothing AndAlso caddBeforeActEvent(key, cb)

            If r Then
                Dim k = "Before" & key
                addcb(k, cb)
            End If

            Return r
        End Function

        ''' <summary>
        ''' 设置事件发生后监听
        ''' </summary>
        ''' <paramname="key"></param>
        ''' <paramname="cb"></param>
        ''' <returns></returns>
        Public Function addAfterActListener(ByVal key As String, ByVal cb As EventCab) As Boolean
            Dim r = caddAfterActEvent IsNot Nothing AndAlso caddAfterActEvent(key, cb)

            If r Then
                Dim k = "After" & key
                addcb(k, cb)
            End If

            Return r
        End Function

        ''' <summary>
        ''' 移除事件发生前监听
        ''' </summary>
        ''' <paramname="key"></param>
        ''' <paramname="cb"></param>
        ''' <returns></returns>
        Public Function removeBeforeActListener(ByVal key As String, ByVal cb As EventCab) As Boolean
            Dim r = cremoveBeforeAct IsNot Nothing AndAlso cremoveBeforeAct(key, cb)

            If r Then
                Dim k = "Before" & key
                removecb(k, cb)
            End If

            Return r
        End Function

        ''' <summary>
        ''' 移除事件发生后监听
        ''' </summary>
        ''' <paramname="key"></param>
        ''' <paramname="cb"></param>
        ''' <returns></returns>
        Public Function removeAfterActListener(ByVal key As String, ByVal cb As EventCab) As Boolean
            Dim r = cremoveAfterAct IsNot Nothing AndAlso cremoveAfterAct(key, cb)

            If r Then
                Dim k = "After" & key
                removecb(k, cb)
            End If

            Return r
        End Function

        ''' <summary>
        ''' 设置共享数据（指针）<br/>
        ''' 注：会替换掉旧数据
        ''' </summary>
        ''' <paramname="key">关键字</param>
        ''' <paramname="data">数据/函数指针</param>
        Public Sub setSharePtr(ByVal key As String, ByVal data As IntPtr)
            If csetshareptr IsNot Nothing Then csetshareptr(key, data)
        End Sub
        ''' <summary>
        ''' 获取共享数据（指针）
        ''' </summary>
        ''' <paramname="key">关键字</param>
        ''' <returns></returns>
        Public Function getSharePtr(ByVal key As String) As IntPtr
            Return If(cgetSharePtr IsNot Nothing, cgetSharePtr(key), IntPtr.Zero)
        End Function
        ''' <summary>
        ''' 移除共享数据（指针）
        ''' </summary>
        ''' <paramname="key">关键字</param>
        ''' <returns></returns>
        Public Function removeSharePtr(ByVal key As String) As IntPtr
            Return If(cremoveSharePtr IsNot Nothing, cremoveSharePtr(key), IntPtr.Zero)
        End Function
        ''' <summary>
        ''' 设置一个指令说明<br/>
        ''' 备注：延期注册的情况，可能不会改变客户端界面
        ''' </summary>
        ''' <paramname="key">命令</param>
        ''' <paramname="description">描述</param>
        ''' <paramname="level">执行要求等级</param>
        ''' <paramname="flag1">命令类型1</param>
        ''' <paramname="flag2">命令类型2</param>
        Public Sub setCommandDescribeEx(ByVal key As String, ByVal description As String, ByVal level As CommandPermissionLevel, ByVal flag1 As Byte, ByVal flag2 As Byte)
            If csetCommandDescribe IsNot Nothing Then csetCommandDescribe(key, description, level, flag1, flag2)
        End Sub
        ''' <summary>
        ''' 设置一个全局指令描述
        ''' </summary>
        ''' <paramname="key">命令</param>
        ''' <paramname="description">描述</param>
        Public Sub setCommandDescribe(ByVal key As String, ByVal description As String)
            setCommandDescribeEx(key, description, CommandPermissionLevel.Any, CommandCheatFlag.NotCheat, CommandVisibilityFlag.Visible)
        End Sub

        ''' <summary>
        ''' 执行后台指令
        ''' </summary>
        ''' <paramname="cmd">语法正确的MC指令</param>
        ''' <returns>是否正常执行</returns>
        Public Function runcmd(ByVal cmd As String) As Boolean
            Return cruncmd IsNot Nothing AndAlso cruncmd(cmd)
        End Function

        ''' <summary>
        ''' 发送一条命令输出消息（可被拦截）<br/>
        ''' 注：末尾附带换行符
        ''' </summary>
        ''' <paramname="cmdout">待发送的命令输出字符串</param>
        Public Sub logout(ByVal cmdout As String)
            If clogout IsNot Nothing Then clogout(cmdout)
        End Sub

        ''' <summary>
        ''' 获取在线玩家列表
        ''' </summary>
        ''' <returns></returns>
        Public Function getOnLinePlayers() As String
            Try
                Return If(cgetOnLinePlayers IsNot Nothing, StrTool.c_str(cgetOnLinePlayers()), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function

        ''' <summary>
        ''' 获取一个结构
        ''' </summary>
        ''' <paramname="did">地图维度</param>
        ''' <paramname="jsonposa">坐标JSON字符串</param>
        ''' <paramname="jsonposb">坐标JSON字符串</param>
        ''' <paramname="exent">是否导出实体</param>
        ''' <paramname="exblk">是否导出方块</param>
        ''' <returns>结构json字符串</returns>
        Public Function getStructure(ByVal did As Integer, ByVal jsonposa As String, ByVal jsonposb As String, ByVal exent As Boolean, ByVal exblk As Boolean) As String
            Try
                Return If(cgetStructure IsNot Nothing, StrTool.c_str(cgetStructure(did, jsonposa, jsonposb, exent, exblk)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 设置一个结构到指定位置<br/>
        ''' 注：旋转类型包含4种有效旋转类型
        ''' </summary>
        ''' <paramname="jdata">结构JSON字符串</param>
        ''' <paramname="did">地图维度</param>
        ''' <paramname="jsonposa">起始点坐标JSON字符串</param>
        ''' <paramname="rot">旋转类型</param>
        ''' <paramname="exent">是否导入实体</param>
        ''' <paramname="exblk">是否导入方块</param>
        ''' <returns>是否设置成功</returns>
        Public Function setStructure(ByVal jdata As String, ByVal did As Integer, ByVal jsonposa As String, ByVal rot As Byte, ByVal exent As Boolean, ByVal exblk As Boolean) As Boolean
            Return csetStructure IsNot Nothing AndAlso csetStructure(jdata, did, jsonposa, rot, exent, exblk)
        End Function

        ''' <summary>
        ''' 重命名一个指定的玩家名<br/>
        ''' 注：该函数可能不会变更客户端实际显示名
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="newName">新的名称</param>
        ''' <returns>是否命名成功</returns>
        Public Function reNameByUuid(ByVal uuid As String, ByVal newName As String) As Boolean
            Return creNameByUuid IsNot Nothing AndAlso creNameByUuid(uuid, newName)
        End Function

        ''' <summary>
        ''' 获取玩家能力表<br/>
        ''' 注：含总计18种能力值
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>能力json字符串</returns>
        Public Function getPlayerAbilities(ByVal uuid As String) As String
            Try
                Return If(cgetPlayerAbilities IsNot Nothing, StrTool.c_str(cgetPlayerAbilities(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 设置玩家能力表<br/>
        ''' 注：该函数可能不会变更客户端实际显示能力
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="abdata">新能力json数据字符串</param>
        ''' <returns>是否设置成功</returns>
        Public Function setPlayerAbilities(ByVal uuid As String, ByVal abdata As String) As Boolean
            Return csetPlayerAbilities IsNot Nothing AndAlso csetPlayerAbilities(uuid, abdata)
        End Function

        ''' <summary>
        ''' 获取玩家属性表<br/>
        ''' 注：总计14种生物属性，含部分有效玩家属性
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>属性json字符串</returns>
        Public Function getPlayerAttributes(ByVal uuid As String) As String
            Try
                Return If(cgetPlayerAttributes IsNot Nothing, StrTool.c_str(cgetPlayerAttributes(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 设置玩家属性临时值表<br/>
        ''' 注：该函数可能不会变更客户端实际显示值
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="newTempAttributes">新属性临时值json数据字符串</param>
        ''' <returns>是否设置成功</returns>
        Public Function setPlayerTempAttributes(ByVal uuid As String, ByVal newTempAttributes As String) As Boolean
            Return csetPlayerTempAttributes IsNot Nothing AndAlso csetPlayerTempAttributes(uuid, newTempAttributes)
        End Function
        ''' <summary>
        ''' 获取玩家属性上限值表
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>属性上限值json字符串</returns>
        Public Function getPlayerMaxAttributes(ByVal uuid As String) As String
            Try
                Return If(cgetPlayerMaxAttributes IsNot Nothing, StrTool.c_str(cgetPlayerMaxAttributes(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 设置玩家属性上限值表<br/>
        ''' 注：该函数可能不会变更客户端实际显示值
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="newMaxAttributes">新属性上限值json数据字符串</param>
        ''' <returns>是否设置成功</returns>
        Public Function setPlayerMaxAttributes(ByVal uuid As String, ByVal newMaxAttributes As String) As Boolean
            Return csetPlayerMaxAttributes IsNot Nothing AndAlso csetPlayerMaxAttributes(uuid, newMaxAttributes)
        End Function

        ''' <summary>
        ''' 获取玩家所有物品列表<br/>
        ''' 注：玩家物品包括末影箱、装备、副手和背包四项物品的nbt描述型数据列表。nbt被序列化数据类型的tag所描述，总计12种有效tag，所对应值可序列化为json数据，亦可反序列化为nbt
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>物品列表json字符串</returns>
        Public Function getPlayerItems(ByVal uuid As String) As String
            Try
                Return If(cgetPlayerItems IsNot Nothing, StrTool.c_str(cgetPlayerItems(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 设置玩家所有物品列表<br/>
        ''' 注：特定条件下可能不会变更游戏内实际物品
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="newItems">新物品列表json数据字符串</param>
        ''' <returns>是否设置成功</returns>
        Public Function setPlayerItems(ByVal uuid As String, ByVal newItems As String) As Boolean
            Return csetPlayerItems IsNot Nothing AndAlso csetPlayerItems(uuid, newItems)
        End Function
        ''' <summary>
        ''' 获取玩家当前选中项信息<br/>
        ''' 注：选中项包含选中框所处位置，以及选中物品的nbt描述型数据
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>当前选中项信息json字符串</returns>
        Public Function getPlayerSelectedItem(ByVal uuid As String) As String
            Try
                Return If(cgetPlayerSelectedItem IsNot Nothing, StrTool.c_str(cgetPlayerSelectedItem(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 增加玩家一个物品<br/>
        ''' 注：特定条件下可能不会变更游戏内实际物品
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="item">物品json数据字符串</param>
        ''' <returns>是否添加成功</returns>
        Public Function addPlayerItemEx(ByVal uuid As String, ByVal item As String) As Boolean
            Return caddPlayerItemEx IsNot Nothing AndAlso caddPlayerItemEx(uuid, item)
        End Function
        ''' <summary>
        ''' 增加玩家一个物品<br/>
        ''' 注：特定条件下可能不会变更游戏内实际物品
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="id">物品id值</param>
        ''' <paramname="aux">物品特殊值</param>
        ''' <paramname="count">数量</param>
        ''' <returns>是否增加成功</returns>
        Public Function addPlayerItem(ByVal uuid As String, ByVal id As Integer, ByVal aux As Short, ByVal count As Byte) As Boolean
            Return caddPlayerItem IsNot Nothing AndAlso caddPlayerItem(uuid, id, aux, count)
        End Function
        ''' <summary>
        ''' 获取玩家所有效果列表
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>效果列表json字符串</returns>
        Public Function getPlayerEffects(ByVal uuid As String) As String
            Try
                Return If(cgetPlayerEffects IsNot Nothing, StrTool.c_str(cgetPlayerEffects(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 设置玩家所有效果列表<br/>
        ''' 注：特定条件下可能不会变更游戏内实际界面
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="newEffects">新效果列表json数据字符串</param>
        ''' <returns>是否设置成功</returns>
        Public Function setPlayerEffects(ByVal uuid As String, ByVal newEffects As String) As Boolean
            Return csetPlayerEffects IsNot Nothing AndAlso csetPlayerEffects(uuid, newEffects)
        End Function

        ''' <summary>
        ''' 设置玩家自定义血条
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="title">血条标题</param>
        ''' <paramname="percent">血条百分比</param>
        ''' <returns></returns>
        Public Function setPlayerBossBar(ByVal uuid As String, ByVal title As String, ByVal percent As Single) As Boolean
            Return csetPlayerBossBar IsNot Nothing AndAlso csetPlayerBossBar(uuid, title, percent)
        End Function
        ''' <summary>
        ''' 清除玩家自定义血条
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>是否清除成功</returns>
        Public Function removePlayerBossBar(ByVal uuid As String) As Boolean
            Return cremovePlayerBossBar IsNot Nothing AndAlso cremovePlayerBossBar(uuid)
        End Function

        ''' <summary>
        ''' 查询在线玩家基本信息
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>玩家基本信息json字符串</returns>
        Public Function selectPlayer(ByVal uuid As String) As String
            Try
                Return If(cselectPlayer IsNot Nothing, StrTool.c_str(cselectPlayer(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e)
            End Try

            Return String.Empty
        End Function

        ''' <summary>
        ''' 传送玩家至指定服务器
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="addr">待传服务器</param>
        ''' <paramname="port">端口</param>
        ''' <returns>是否传送成功</returns>
        Public Function transferserver(ByVal uuid As String, ByVal addr As String, ByVal port As Integer) As Boolean
            Return ctransferserver IsNot Nothing AndAlso ctransferserver(uuid, addr, port)
        End Function
        ''' <summary>
        ''' 传送玩家至指定坐标和维度
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="x"></param>
        ''' <paramname="y"></param>
        ''' <paramname="z"></param>
        ''' <paramname="did">维度ID</param>
        ''' <returns>是否传送成功</returns>
        Public Function teleport(ByVal uuid As String, ByVal x As Single, ByVal y As Single, ByVal z As Single, ByVal did As Integer) As Boolean
            Return cteleport IsNot Nothing AndAlso cteleport(uuid, x, y, z, did)
        End Function
        ''' <summary>
        ''' 模拟玩家发送一个文本
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="msg">待模拟发送的文本</param>
        ''' <returns>是否发送成功</returns>
        Public Function talkAs(ByVal uuid As String, ByVal msg As String) As Boolean
            Return ctalkAs IsNot Nothing AndAlso ctalkAs(uuid, msg)
        End Function
        ''' <summary>
        ''' 模拟玩家执行一个指令
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="cmd">待模拟执行的指令</param>
        ''' <returns>是否发送成功</returns>
        Public Function runcmdAs(ByVal uuid As String, ByVal cmd As String) As Boolean
            Return cruncmdAs IsNot Nothing AndAlso cruncmdAs(uuid, cmd)
        End Function

        ''' <summary>
        ''' 向指定的玩家发送一个简单表单
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="title">表单标题</param>
        ''' <paramname="content">内容</param>
        ''' <paramname="buttons">按钮文本数组字符串</param>
        ''' <returns>创建的表单id，为 0 表示发送失败</returns>
        Public Function sendSimpleForm(ByVal uuid As String, ByVal title As String, ByVal content As String, ByVal buttons As String) As UInteger
            Return If(csendSimpleForm IsNot Nothing, csendSimpleForm(uuid, title, content, buttons), 0)
        End Function
        ''' <summary>
        ''' 向指定的玩家发送一个模式对话框
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="title">表单标题</param>
        ''' <paramname="content">内容</param>
        ''' <paramname="button1">按钮1标题（点击该按钮selected为true）</param>
        ''' <paramname="button2">按钮2标题（点击该按钮selected为false）</param>
        ''' <returns>创建的表单id，为 0 表示发送失败</returns>
        Public Function sendModalForm(ByVal uuid As String, ByVal title As String, ByVal content As String, ByVal button1 As String, ByVal button2 As String) As UInteger
            Return If(csendModalForm IsNot Nothing, csendModalForm(uuid, title, content, button1, button2), 0)
        End Function
        ''' <summary>
        ''' 向指定的玩家发送一个自定义表单
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="json">自定义表单的json字符串<br/>
        ''' （要使用自定义表单类型，参考nk、pm格式或minebbs专栏）</param>
        ''' <returns>创建的表单id，为 0 表示发送失败</returns>
        Public Function sendCustomForm(ByVal uuid As String, ByVal json As String) As UInteger
            Return If(csendCustomForm IsNot Nothing, csendCustomForm(uuid, json), 0)
        End Function
        ''' <summary>
        ''' 放弃一个表单<br/>
        ''' 注：已被接收到的表单会被自动释放
        ''' </summary>
        ''' <paramname="formid">表单id</param>
        ''' <returns>是否释放成功</returns>
        Public Function releaseForm(ByVal formid As UInteger) As Boolean
            Return creleaseForm IsNot Nothing AndAlso creleaseForm(formid)
        End Function

        ''' <summary>
        ''' 设置玩家自定义侧边栏临时计分板<br/>
        ''' 注：列表总是从第1行开始，总计不超过15行
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="title">侧边栏标题</param>
        ''' <paramname="list">列表字符串数组</param>
        ''' <returns>是否设置成功</returns>
        Public Function setPlayerSidebar(ByVal uuid As String, ByVal title As String, ByVal list As String) As Boolean
            Return csetPlayerSidebar IsNot Nothing AndAlso csetPlayerSidebar(uuid, title, list)
        End Function
        ''' <summary>
        ''' 清除玩家自定义侧边栏
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>是否清除成功</returns>
        Public Function removePlayerSidebar(ByVal uuid As String) As Boolean
            Return cremovePlayerSidebar IsNot Nothing AndAlso cremovePlayerSidebar(uuid)
        End Function

        ''' <summary>
        ''' 获取玩家权限与游戏模式<br/>
        ''' 注：OP命令等级包含6个有效等级[op-permission-level]，权限包含3种有效权限[permissions.json]，游戏模式包含5种有效模式[gamemode]
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <returns>权限与模式的json字符串</returns>
        Public Function getPlayerPermissionAndGametype(ByVal uuid As String) As String
            Try
                Return If(cgetPlayerPermissionAndGametype IsNot Nothing, StrTool.c_str(cgetPlayerPermissionAndGametype(uuid)), String.Empty)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return String.Empty
        End Function
        ''' <summary>
        ''' 设置玩家权限与游戏模式<br/>
        ''' 注：特定条件下可能不会变更游戏内实际能力
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="newModes">新权限或模式json数据字符串</param>
        ''' <returns>是否设置成功</returns>
        Public Function setPlayerPermissionAndGametype(ByVal uuid As String, ByVal newModes As String) As Boolean
            Return csetPlayerPermissionAndGametype IsNot Nothing AndAlso csetPlayerPermissionAndGametype(uuid, newModes)
        End Function

        ' 社区贡献

        ''' <summary>
        ''' 获取指定玩家指定计分板上的数值<br/>
        ''' 注：特定情况下会自动创建计分板
        ''' </summary>
        ''' <paramname="uuid">在线玩家的uuid字符串</param>
        ''' <paramname="objname">计分板登记的名称</param>
        ''' <returns>获取的目标值，若目标不存在则返回0</returns>
        Public Function getscoreboard(ByVal uuid As String, ByVal objname As String) As Integer
            Return If(cgetscoreboardValue IsNot Nothing, cgetscoreboardValue(uuid, objname), 0)
        End Function


        ' 底层相关

        ''' <summary>
        ''' 设置一个钩子
        ''' </summary>
        ''' <paramname="rva">原型函数相对地址</param>
        ''' <paramname="hook">新函数</param>
        ''' <paramname="org">待保存原型函数的指针</param>
        ''' <returns></returns>
        Public Function cshook(ByVal rva As Integer, ByVal hook As IntPtr, <Out> ByRef org As IntPtr) As Boolean
            Dim sorg = IntPtr.Zero
            Dim ret = ccshook IsNot Nothing AndAlso ccshook(rva, hook, sorg)
            org = sorg
            Return ret
        End Function
        ''' <summary>
        ''' 卸载一个钩子
        ''' </summary>
        ''' <paramname="hook">待卸载的函数</param>
        ''' <paramname="org">已保存了原型函数的指针</param>
        ''' <returns></returns>
        Public Function csunhook(ByVal hook As IntPtr, ByRef org As IntPtr) As Boolean
            Dim sorg = org
            Dim ret = ccsunhook IsNot Nothing AndAlso ccsunhook(hook, sorg)
            org = sorg
            Return ret
        End Function
        ''' <summary>
        ''' 取相对地址对应的实际指针
        ''' </summary>
        ''' <paramname="rva"></param>
        ''' <returns></returns>
        Public Function dlsym(ByVal rva As Integer) As IntPtr
            Return If(cdlsym IsNot Nothing, cdlsym(rva), IntPtr.Zero)
        End Function
    End Class
End Namespace
