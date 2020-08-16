' 
'  由SharpDevelop创建。
'  用户： BDSNetRunner
'  日期: 2020/7/19
'  时间: 16:36
'  
'  要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
' 
Imports System
Imports System.Runtime.InteropServices
Imports System.Text

Namespace CSR
    Public Module EventKey
        ''' <summary>
        ''' 无作用
        ''' </summary>
        Public Const [Nothing] = "Nothing"
        ''' <summary>
        ''' onServerCmd - 后台指令监听
        ''' </summary>
        Public Const onServerCmd = "onServerCmd"
        ''' <summary>
        ''' onServerCmdOutput - 后台指令输出信息监听
        ''' </summary>
        Public Const onServerCmdOutput = "onServerCmdOutput"
        ''' <summary>
        ''' onFormSelect - 玩家选择GUI表单监听
        ''' </summary>
        Public Const onFormSelect = "onFormSelect"
        ''' <summary>
        ''' onUseItem - 使用物品监听
        ''' </summary>
        Public Const onUseItem = "onUseItem"
        ''' <summary>
        ''' onPlacedBlock - 放置方块监听
        ''' </summary>
        Public Const onPlacedBlock = "onPlacedBlock"
        ''' <summary>
        ''' onDestroyBlock - 破坏方块监听
        ''' </summary>
        Public Const onDestroyBlock = "onDestroyBlock"
        ''' <summary>
        ''' onStartOpenChest - 开箱监听
        ''' </summary>
        Public Const onStartOpenChest = "onStartOpenChest"
        ''' <summary>
        ''' onStartOpenBarrel - 开桶监听
        ''' </summary>
        Public Const onStartOpenBarrel = "onStartOpenBarrel"
        ''' <summary>
        ''' onStopOpenChest - 关箱监听
        ''' </summary>
        Public Const onStopOpenChest = "onStopOpenChest"
        ''' <summary>
        ''' onStopOpenBarrel - 关桶监听
        ''' </summary>
        Public Const onStopOpenBarrel = "onStopOpenBarrel"
        ''' <summary>
        ''' onSetSlot - 放入取出物品监听
        ''' </summary>
        Public Const onSetSlot = "onSetSlot"
        ''' <summary>
        ''' onChangeDimension - 切换维度监听
        ''' </summary>
        Public Const onChangeDimension = "onChangeDimension"
        ''' <summary>
        ''' onMobDie - 生物死亡监听
        ''' </summary>
        Public Const onMobDie = "onMobDie"
        ''' <summary>
        ''' onMobHurt - 生物受伤监听
        ''' </summary>
        Public Const onMobHurt = "onMobHurt"
        ''' <summary>
        ''' onRespawn - 玩家重生监听
        ''' </summary>
        Public Const onRespawn = "onRespawn"
        ''' <summary>
        ''' onChat - 聊天监听
        ''' </summary>
        Public Const onChat = "onChat"
        ''' <summary>
        ''' onInputText - 玩家输入文本监听
        ''' </summary>
        Public Const onInputText = "onInputText"
        ''' <summary>
        ''' onCommandBlockUpdate - 玩家更新命令方块监听
        ''' </summary>
        Public Const onCommandBlockUpdate = "onCommandBlockUpdate"
        ''' <summary>
        ''' onInputCommand - 玩家输入指令监听
        ''' </summary>
        Public Const onInputCommand = "onInputCommand"
        ''' <summary>
        ''' onBlockCmd - 命令方块(矿车)执行指令监听
        ''' </summary>
        Public Const onBlockCmd = "onBlockCmd"
        ''' <summary>
        ''' onNpcCmd - NPC执行指令监听
        ''' </summary>
        Public Const onNpcCmd = "onNpcCmd"
        ''' <summary>
        ''' onLoadName -  加载名字监听
        ''' </summary>
        Public Const onLoadName = "onLoadName"
        ''' <summary>
        ''' onPlayerLeft - 离开游戏监听
        ''' </summary>
        Public Const onPlayerLeft = "onPlayerLeft"
        ''' <summary>
        ''' onMove - 移动监听
        ''' </summary>
        Public Const onMove = "onMove"
        ''' <summary>
        ''' onAttack - 攻击监听
        ''' </summary>
        Public Const onAttack = "onAttack"
        ''' <summary>
        ''' onLevelExplode - 爆炸监听
        ''' </summary>
        Public Const onLevelExplode = "onLevelExplode"
    End Module

    Public Enum EventType
        [Nothing] = 0
        onServerCmd = 1
        onServerCmdOutput = 2
        onFormSelect = 3
        onUseItem = 4
        onPlacedBlock = 5
        onDestroyBlock = 6
        onStartOpenChest = 7
        onStartOpenBarrel = 8
        onStopOpenChest = 9
        onStopOpenBarrel = 10
        onSetSlot = 11
        onChangeDimension = 12
        onMobDie = 13
        onMobHurt = 14
        onRespawn = 15
        onChat = 16
        onInputText = 17
        onCommandBlockUpdate = 18
        onInputCommand = 19
        onBlockCmd = 20
        onNpcCmd = 21
        onLoadName = 22
        onPlayerLeft = 23
        onMove = 24
        onAttack = 25
        onLevelExplode = 26
    End Enum

    Public Enum ActMode
        BEFORE = 0
        AFTER = 1
    End Enum

    <StructLayoutAttribute(LayoutKind.Sequential)>
    Public Structure Events
        ''' <summary>
        ''' 事件类型
        ''' </summary>
        Public type As UShort
        ''' <summary>
        ''' 事件触发模式
        ''' </summary>
        Public mode As UShort
        ''' <summary>
        ''' 事件结果（注册After事件时，此值有效）
        ''' </summary>
        Public result As Integer
        ''' <summary>
        ''' 原始数据
        ''' </summary>
        Public data As IntPtr
    End Structure

    Public Structure BPos3
        Public x, y, z As Integer
    End Structure

    Public Structure Vec3
        Public x, y, z As Single
    End Structure

    <StructLayoutAttribute(LayoutKind.Sequential)>
    Public Structure Std_String
        Public data As IntPtr
        Public sd As ULong
        Public len As ULong
        Public uk3 As ULong
    End Structure

    Public Class StrTool
        <DllImport("kernel32.dll", CharSet:=CharSet.Ansi, ExactSpelling:=True)>
        Friend Shared Function lstrlenA(ByVal ptr As IntPtr) As Integer
        End Function

        ' 从目标位置获取utf8字符串
        Public Shared Function readUTF8str(ByVal ptr As IntPtr) As String
            If IntPtr.Zero = ptr Then
                Return Nothing
            End If

            Dim l = lstrlenA(ptr)

            If l = 0 Then
                Return String.Empty
            End If

            Dim b = New Byte(l - 1) {}
            Marshal.Copy(ptr, b, 0, l)
            Return Encoding.UTF8.GetString(b)
        End Function
        ' 四字节转浮点
        Public Shared Function itof(ByVal x As Integer) As Single
            Return BitConverter.ToSingle(BitConverter.GetBytes(x), 0)
        End Function
        ' std::string中读取c_str
        Public Shared Function c_str(ByVal s As Std_String) As String
            Try
                If s.len < 1 Then Return String.Empty

                If s.len < 16 Then
                    Dim c = BitConverter.GetBytes(CULng(s.data))
                    Dim d = BitConverter.GetBytes(s.sd)
                    Dim str = New Byte(15) {}
                    Array.Copy(c, str, 8)
                    Array.Copy(d, 0, str, 8, 8)
                    Return Encoding.UTF8.GetString(str, 0, s.len)
                End If

                Return readUTF8str(s.data)
            Catch e As Exception
                Console.WriteLine(e.StackTrace)
            End Try

            Return Nothing
        End Function
    End Class

    Public Class BaseEvent
        Protected mTYPE As EventType
        Protected mMODE As ActMode
        Protected mRESULT As Boolean
        ''' <summary>
        ''' 事件类型
        ''' </summary>
        Public ReadOnly Property TYPE As EventType
            Get
                Return mTYPE
            End Get
        End Property
        ''' <summary>
        ''' 事件触发模式
        ''' </summary>
        Public ReadOnly Property MODE As ActMode
            Get
                Return mMODE
            End Get
        End Property
        ''' <summary>
        ''' 事件结果（注册After事件时，此值有效）
        ''' </summary>
        Public ReadOnly Property RESULT As Boolean
            Get
                Return mRESULT
            End Get
        End Property

        ''' <summary>
        ''' 解析一个事件数据
        ''' </summary>
        ''' <paramname="e"></param>
        ''' <returns></returns>
        Public Shared Function getFrom(ByVal e As Events) As BaseEvent
            If e.data = IntPtr.Zero Then
                Console.WriteLine("Empty struct data.")
                Return Nothing
            End If

            Try

                Select Case e.type
                    Case EventType.onServerCmd
                        Return ServerCmdEvent.getFrom(e)
                    Case EventType.onServerCmdOutput
                        Return ServerCmdOutputEvent.getFrom(e)
                    Case EventType.onFormSelect
                        Return FormSelectEvent.getFrom(e)
                    Case EventType.onUseItem
                        Return UseItemEvent.getFrom(e)
                    Case EventType.onPlacedBlock
                        Return PlacedBlockEvent.getFrom(e)
                    Case EventType.onDestroyBlock
                        Return DestroyBlockEvent.getFrom(e)
                    Case EventType.onStartOpenChest
                        Return StartOpenChestEvent.getFrom(e)
                    Case EventType.onStartOpenBarrel
                        Return StartOpenBarrelEvent.getFrom(e)
                    Case EventType.onStopOpenChest
                        Return StopOpenChestEvent.getFrom(e)
                    Case EventType.onStopOpenBarrel
                        Return StopOpenBarrelEvent.getFrom(e)
                    Case EventType.onSetSlot
                        Return SetSlotEvent.getFrom(e)
                    Case EventType.onChangeDimension
                        Return ChangeDimensionEvent.getFrom(e)
                    Case EventType.onMobDie
                        Return MobDieEvent.getFrom(e)
                    Case EventType.onMobHurt
                        Return MobHurtEvent.getFrom(e)
                    Case EventType.onRespawn
                        Return RespawnEvent.getFrom(e)
                    Case EventType.onChat
                        Return ChatEvent.getFrom(e)
                    Case EventType.onInputText
                        Return InputTextEvent.getFrom(e)
                    Case EventType.onCommandBlockUpdate
                        Return CommandBlockUpdateEvent.getFrom(e)
                    Case EventType.onInputCommand
                        Return InputCommandEvent.getFrom(e)
                    Case EventType.onBlockCmd
                        Return BlockCmdEvent.getFrom(e)
                    Case EventType.onNpcCmd
                        Return NpcCmdEvent.getFrom(e)
                    Case EventType.onLoadName
                        Return LoadNameEvent.getFrom(e)
                    Case EventType.onPlayerLeft
                        Return PlayerLeftEvent.getFrom(e)
                    Case EventType.onMove
                        Return MoveEvent.getFrom(e)
                    Case EventType.onAttack
                        Return AttackEvent.getFrom(e)
                    Case EventType.onLevelExplode
                        ' do nothing
                        Return LevelExplodeEvent.getFrom(e)
                    Case Else
                End Select

            Catch ex As Exception
                Console.WriteLine(ex.StackTrace)
            End Try

            Return Nothing
        End Function

        ''' <summary>
        ''' 构造一个事件头部
        ''' </summary>
        ''' <paramname="e">原始事件数据</param>
        ''' <paramname="et">事件类型</param>
        ''' <paramname="t">类类型</param>
        ''' <returns></returns>
        Protected Shared Function createHead(ByVal e As Events, ByVal et As EventType, ByVal t As Type) As BaseEvent
            If CType(e.type, EventType) = et Then
                Dim o = t.Assembly.CreateInstance(t.FullName)
                Dim be = TryCast(o, BaseEvent)

                If be IsNot Nothing Then
                    be.mTYPE = CType(e.type, EventType)
                    be.mMODE = CType(e.mode, ActMode)
                    be.mRESULT = e.result = 1
                    Return be
                End If
            Else
                Console.WriteLine("Event type mismatch.")
            End If

            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' 后台指令监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class ServerCmdEvent
        Inherits BaseEvent

        Protected mcmd As String
        ''' <summary>
        ''' 指令数据
        ''' </summary>
        Public ReadOnly Property cmd As String
            Get
                Return mcmd
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As ServerCmdEvent
            Dim sce = TryCast(createHead(e, EventType.onServerCmd, GetType(ServerCmdEvent)), ServerCmdEvent)
            If sce Is Nothing Then Return sce
            Dim s = e.data  ' 此处为转换过程
            sce.mcmd = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            Return sce
        End Function
    End Class

    ''' <summary>
    ''' 后台指令输出信息监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class ServerCmdOutputEvent
        Inherits BaseEvent

        Protected moutput As String
        ''' <summary>
        ''' 指令数据
        ''' </summary>
        Public ReadOnly Property output As String
            Get
                Return moutput
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As ServerCmdOutputEvent
            Dim soe = TryCast(createHead(e, EventType.onServerCmdOutput, GetType(ServerCmdOutputEvent)), ServerCmdOutputEvent)
            If soe Is Nothing Then Return soe
            Dim s = e.data  ' 此处为转换过程
            soe.moutput = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            Return soe
        End Function
    End Class

    Public Class PlayerEvent
        Inherits BaseEvent

        Protected mplayername As String
        Protected mdimension As String
        Protected mXYZ As Vec3
        Protected mdimensionid As Integer
        Protected misstand As Boolean
        ''' <summary>
        ''' 玩家名字
        ''' </summary>
        Public ReadOnly Property playername As String
            Get
                Return mplayername
            End Get
        End Property
        ''' <summary>
        ''' 玩家所在维度
        ''' </summary>
        Public ReadOnly Property dimension As String
            Get
                Return mdimension
            End Get
        End Property
        ''' <summary>
        ''' 玩家所处位置
        ''' </summary>
        Public ReadOnly Property XYZ As Vec3
            Get
                Return mXYZ
            End Get
        End Property
        ''' <summary>
        ''' 玩家所在维度ID
        ''' </summary>
        Public ReadOnly Property dimensionid As Integer
            Get
                Return mdimensionid
            End Get
        End Property
        ''' <summary>
        ''' 玩家是否立足于方块/悬空
        ''' </summary>
        Public ReadOnly Property isstand As Boolean
            Get
                Return misstand
            End Get
        End Property

        Protected Sub loadData(ByVal s As IntPtr)
            ' 此处为转换过程
            mplayername = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            mdimension = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            mXYZ = CType(Marshal.PtrToStructure(s + 16, GetType(Vec3)), Vec3)
            mdimensionid = Marshal.ReadInt32(s, 28)
            misstand = Marshal.ReadByte(s, 32) = 1
        End Sub
    End Class

    ''' <summary>
    ''' 玩家选择GUI表单监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class FormSelectEvent
        Inherits PlayerEvent

        Protected muuid As String
        Protected mselected As String
        Protected mformid As Integer
        ''' <summary>
        ''' 玩家uuid信息
        ''' </summary>
        Public ReadOnly Property uuid As String
            Get
                Return muuid
            End Get
        End Property
        ''' <summary>
        ''' 表单回传的选择项信息
        ''' </summary>
        Public ReadOnly Property selected As String
            Get
                Return mselected
            End Get
        End Property
        ''' <summary>
        ''' 表单ID
        ''' </summary>
        Public ReadOnly Property formid As Integer
            Get
                Return mformid
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As FormSelectEvent
            Dim fse = TryCast(createHead(e, EventType.onFormSelect, GetType(FormSelectEvent)), FormSelectEvent)
            If fse Is Nothing Then Return fse
            Dim s = e.data  ' 此处为转换过程
            fse.loadData(s)
            fse.muuid = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            fse.mselected = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 48), IntPtr))
            fse.mformid = Marshal.ReadInt32(s, 56)
            Return fse
        End Function
    End Class

    ''' <summary>
    ''' 使用物品监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class UseItemEvent
        Inherits PlayerEvent

        Protected mitemname As String
        Protected mposition As BPos3
        Protected mitemid As Short
        Protected mitemaux As Short
        Protected mblockname As String
        Protected mblockid As Short
        ''' <summary>
        ''' 物品名称
        ''' </summary>
        Public ReadOnly Property itemname As String
            Get
                Return mitemname
            End Get
        End Property
        ''' <summary>
        ''' 操作方块所在位置
        ''' </summary>
        Public ReadOnly Property position As BPos3
            Get
                Return mposition
            End Get
        End Property
        ''' <summary>
        ''' 物品ID
        ''' </summary>
        Public ReadOnly Property itemid As Short
            Get
                Return mitemid
            End Get
        End Property
        ''' <summary>
        ''' 物品特殊值
        ''' </summary>
        Public ReadOnly Property itemaux As Short
            Get
                Return mitemaux
            End Get
        End Property
        ''' <summary>
        ''' 操作方块名称
        ''' </summary>
        Public ReadOnly Property blockname As String
            Get
                Return mblockname
            End Get
        End Property
        ''' <summary>
        ''' 操作方块id
        ''' </summary>
        Public ReadOnly Property blockid As Short
            Get
                Return mblockid
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As UseItemEvent
            Dim ue = TryCast(createHead(e, EventType.onUseItem, GetType(UseItemEvent)), UseItemEvent)
            If ue Is Nothing Then Return Nothing
            Dim s = e.data  ' 此处为转换过程
            ue.loadData(s)
            ue.mitemname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            ue.mposition = CType(Marshal.PtrToStructure(s + 48, GetType(BPos3)), BPos3)
            ue.mitemid = Marshal.ReadInt16(s, 60)
            ue.mitemaux = Marshal.ReadInt16(s, 62)
            ue.mblockname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 64), IntPtr))
            ue.mblockid = Marshal.ReadInt16(s, 80)
            Return ue
        End Function
    End Class

    Public Class BlockEvent
        Inherits PlayerEvent

        Protected mblockname As String
        Protected mposition As BPos3
        Protected mblockid As Short
        ''' <summary>
        ''' 方块名称
        ''' </summary>
        Public ReadOnly Property blockname As String
            Get
                Return mblockname
            End Get
        End Property
        ''' <summary>
        ''' 操作方块所在位置
        ''' </summary>
        Public ReadOnly Property position As BPos3
            Get
                Return mposition
            End Get
        End Property
        ''' <summary>
        ''' 方块ID
        ''' </summary>
        Public ReadOnly Property blockid As Short
            Get
                Return mblockid
            End Get
        End Property

        Protected Overloads Sub loadData(ByVal s As IntPtr)
            MyBase.loadData(s)
            ' 此处为转换过程
            mblockname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            mposition = CType(Marshal.PtrToStructure(s + 48, GetType(BPos3)), BPos3)
            mblockid = Marshal.ReadInt16(s, 60)
        End Sub
    End Class

    ''' <summary>
    ''' 放置方块监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class PlacedBlockEvent
        Inherits BlockEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As PlacedBlockEvent
            Dim ple = TryCast(createHead(e, EventType.onPlacedBlock, GetType(PlacedBlockEvent)), PlacedBlockEvent)
            If ple Is Nothing Then Return Nothing
            ple.loadData(e.data)
            Return ple
        End Function
    End Class

    ''' <summary>
    ''' 破坏方块监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class DestroyBlockEvent
        Inherits BlockEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As DestroyBlockEvent
            Dim ple = TryCast(createHead(e, EventType.onDestroyBlock, GetType(DestroyBlockEvent)), DestroyBlockEvent)
            If ple Is Nothing Then Return Nothing
            ple.loadData(e.data)
            Return ple
        End Function
    End Class

    ''' <summary>
    ''' 开箱监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class StartOpenChestEvent
        Inherits BlockEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As StartOpenChestEvent
            Dim ple = TryCast(createHead(e, EventType.onStartOpenChest, GetType(StartOpenChestEvent)), StartOpenChestEvent)
            If ple Is Nothing Then Return Nothing
            ple.loadData(e.data)
            Return ple
        End Function
    End Class

    ''' <summary>
    ''' 开桶监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class StartOpenBarrelEvent
        Inherits BlockEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As StartOpenBarrelEvent
            Dim ple = TryCast(createHead(e, EventType.onStartOpenBarrel, GetType(StartOpenBarrelEvent)), StartOpenBarrelEvent)
            If ple Is Nothing Then Return Nothing
            ple.loadData(e.data)
            Return ple
        End Function
    End Class

    ''' <summary>
    ''' 关箱监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class StopOpenChestEvent
        Inherits BlockEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As StopOpenChestEvent
            Dim ple = TryCast(createHead(e, EventType.onStopOpenChest, GetType(StopOpenChestEvent)), StopOpenChestEvent)
            If ple Is Nothing Then Return Nothing
            ple.loadData(e.data)
            Return ple
        End Function
    End Class

    ''' <summary>
    ''' 关桶监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class StopOpenBarrelEvent
        Inherits BlockEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As StopOpenBarrelEvent
            Dim ple = TryCast(createHead(e, EventType.onStopOpenBarrel, GetType(StopOpenBarrelEvent)), StopOpenBarrelEvent)
            If ple Is Nothing Then Return Nothing
            ple.loadData(e.data)
            Return ple
        End Function
    End Class

    ''' <summary>
    ''' 放入取出物品监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class SetSlotEvent
        Inherits PlayerEvent

        Protected mitemname As String
        Protected mblockname As String
        Protected mposition As BPos3
        Protected mitemcount As Integer
        Protected mslot As Integer
        Protected mitemaux As Short
        Protected mblockid As Short
        Protected mitemid As Short
        ''' <summary>
        ''' 物品名字
        ''' </summary>
        Public ReadOnly Property itemname As String
            Get
                Return mitemname
            End Get
        End Property
        ''' <summary>
        ''' 方块名称
        ''' </summary>
        Public ReadOnly Property blockname As String
            Get
                Return mblockname
            End Get
        End Property
        ''' <summary>
        ''' 操作方块所在位置
        ''' </summary>
        Public ReadOnly Property position As BPos3
            Get
                Return mposition
            End Get
        End Property
        ''' <summary>
        ''' 物品数量
        ''' </summary>
        Public ReadOnly Property itemcount As Integer
            Get
                Return mitemcount
            End Get
        End Property
        ''' <summary>
        ''' 操作格子位置
        ''' </summary>
        Public ReadOnly Property slot As Integer
            Get
                Return mslot
            End Get
        End Property
        ''' <summary>
        ''' 物品特殊值
        ''' </summary>
        Public ReadOnly Property itemaux As Short
            Get
                Return mitemaux
            End Get
        End Property
        ''' <summary>
        ''' 方块ID
        ''' </summary>
        Public ReadOnly Property blockid As Short
            Get
                Return mblockid
            End Get
        End Property
        ''' <summary>
        ''' 物品ID
        ''' </summary>
        Public ReadOnly Property itemid As Short
            Get
                Return mitemid
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As SetSlotEvent
            Dim le = TryCast(createHead(e, EventType.onSetSlot, GetType(SetSlotEvent)), SetSlotEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data  ' 此处为转换过程
            le.loadData(s)
            le.mitemname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            le.mblockname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 48), IntPtr))
            le.mposition = CType(Marshal.PtrToStructure(s + 56, GetType(BPos3)), BPos3)
            le.mitemcount = Marshal.ReadInt32(s, 68)
            le.mslot = Marshal.ReadInt32(s, 72)
            le.mitemaux = Marshal.ReadInt16(s, 76)
            le.mblockid = Marshal.ReadInt16(s, 78)
            le.mitemid = Marshal.ReadInt16(s, 80)
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 切换维度监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class ChangeDimensionEvent
        Inherits PlayerEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As ChangeDimensionEvent
            Dim le = TryCast(createHead(e, EventType.onChangeDimension, GetType(ChangeDimensionEvent)), ChangeDimensionEvent)
            If le Is Nothing Then Return Nothing
            le.loadData(e.data)
            Return le
        End Function
    End Class

    Public Class HurtEvent
        Inherits BaseEvent

        Protected mmobname As String
        Protected mplayername As String
        Protected mdimension As String
        Protected mmobtype As String
        Protected msrcname As String
        Protected msrctype As String
        Protected mXYZ As Vec3
        Protected mdimensionid As Integer
        Protected mdmcase As Integer
        Protected misstand As Boolean
        ''' <summary>
        ''' 生物名称
        ''' </summary>
        Public ReadOnly Property mobname As String
            Get
                Return mmobname
            End Get
        End Property
        ''' <summary>
        ''' 玩家名字（若为玩家死亡，附加此信息）
        ''' </summary>
        Public ReadOnly Property playername As String
            Get
                Return mplayername
            End Get
        End Property
        ''' <summary>
        ''' 玩家所在维度（附加信息）
        ''' </summary>
        Public ReadOnly Property dimension As String
            Get
                Return mdimension
            End Get
        End Property
        ''' <summary>
        ''' 生物类型
        ''' </summary>
        Public ReadOnly Property mobtype As String
            Get
                Return mmobtype
            End Get
        End Property
        ''' <summary>
        ''' 伤害源名称
        ''' </summary>
        Public ReadOnly Property srcname As String
            Get
                Return msrcname
            End Get
        End Property
        ''' <summary>
        ''' 伤害源类型
        ''' </summary>
        Public ReadOnly Property srctype As String
            Get
                Return msrctype
            End Get
        End Property
        ''' <summary>
        ''' 生物所在位置
        ''' </summary>
        Public ReadOnly Property XYZ As Vec3
            Get
                Return mXYZ
            End Get
        End Property
        ''' <summary>
        ''' 生物所处维度ID
        ''' </summary>
        Public ReadOnly Property dimensionid As Integer
            Get
                Return mdimensionid
            End Get
        End Property
        ''' <summary>
        ''' 伤害具体原因ID
        ''' </summary>
        Public ReadOnly Property dmcase As Integer
            Get
                Return mdmcase
            End Get
        End Property
        ''' <summary>
        ''' 玩家是否立足于方块/悬空（附加信息）
        ''' </summary>
        Public ReadOnly Property isstand As Boolean
            Get
                Return misstand
            End Get
        End Property

        Protected Sub loadData(ByVal s As IntPtr)
            ' 此处为转换过程
            mmobname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            mplayername = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            mdimension = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 16), IntPtr))
            mmobtype = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 24), IntPtr))
            msrcname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 32), IntPtr))
            msrctype = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            mXYZ = CType(Marshal.PtrToStructure(s + 48, GetType(Vec3)), Vec3)
            mdimensionid = Marshal.ReadInt32(s, 60)
            mdmcase = Marshal.ReadInt32(s, 64)
            misstand = Marshal.ReadByte(s, 68) = 1
        End Sub
    End Class

    ''' <summary>
    ''' 生物死亡监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class MobDieEvent
        Inherits HurtEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As MobDieEvent
            Dim le = TryCast(createHead(e, EventType.onMobDie, GetType(MobDieEvent)), MobDieEvent)
            If le Is Nothing Then Return Nothing
            le.loadData(e.data)
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 生物受伤监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class MobHurtEvent
        Inherits HurtEvent

        Protected mdmtype As String
        Protected mhealth As Single
        Protected mdmcount As Integer
        ''' <summary>
        ''' 生物受伤类型
        ''' </summary>
        Public ReadOnly Property dmtype As String
            Get
                Return mdmtype
            End Get
        End Property
        ''' <summary>
        ''' 生物血量
        ''' </summary>
        Public ReadOnly Property health As Single
            Get
                Return mhealth
            End Get
        End Property
        ''' <summary>
        ''' 生物受伤具体值
        ''' </summary>
        Public ReadOnly Property dmcount As Integer
            Get
                Return mdmcount
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As MobHurtEvent
            Dim le = TryCast(createHead(e, EventType.onMobHurt, GetType(MobHurtEvent)), MobHurtEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data
            le.loadData(s)
            le.mdmtype = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 72), IntPtr))
            le.mhealth = StrTool.itof(Marshal.ReadInt32(s, 80))
            le.mdmcount = Marshal.ReadInt32(s, 84)
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 玩家重生监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class RespawnEvent
        Inherits PlayerEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As RespawnEvent
            Dim le = TryCast(createHead(e, EventType.onRespawn, GetType(RespawnEvent)), RespawnEvent)
            If le Is Nothing Then Return Nothing
            le.loadData(e.data)
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 聊天监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class ChatEvent
        Inherits BaseEvent

        Protected mplayername As String
        Protected mtarget As String
        Protected mmsg As String
        Protected mchatstyle As String
        ''' <summary>
        ''' 发言人名字（可能为服务器或命令方块发言）
        ''' </summary>
        Public ReadOnly Property playername As String
            Get
                Return mplayername
            End Get
        End Property
        ''' <summary>
        ''' 接收者名字
        ''' </summary>
        Public ReadOnly Property target As String
            Get
                Return mtarget
            End Get
        End Property
        ''' <summary>
        ''' 接收到的信息
        ''' </summary>
        Public ReadOnly Property msg As String
            Get
                Return mmsg
            End Get
        End Property
        ''' <summary>
        ''' 聊天类型
        ''' </summary>
        Public ReadOnly Property chatstyle As String
            Get
                Return mchatstyle
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As ChatEvent
            Dim le = TryCast(createHead(e, EventType.onChat, GetType(ChatEvent)), ChatEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data  ' 此处为转换过程
            le.mplayername = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            le.mtarget = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            le.mmsg = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 16), IntPtr))
            le.mchatstyle = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 24), IntPtr))
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 玩家输入文本监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class InputTextEvent
        Inherits PlayerEvent

        Protected mmsg As String
        ''' <summary>
        ''' 输入的文本
        ''' </summary>
        Public ReadOnly Property msg As String
            Get
                Return mmsg
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As InputTextEvent
            Dim le = TryCast(createHead(e, EventType.onInputText, GetType(InputTextEvent)), InputTextEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data
            le.loadData(s)
            le.mmsg = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 玩家更新命令方块监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class CommandBlockUpdateEvent
        Inherits PlayerEvent

        Protected mcmd As String
        Protected mactortype As String
        Protected mposition As BPos3
        Protected misblock As Boolean
        ''' <summary>
        ''' 将被更新的新指令
        ''' </summary>
        Public ReadOnly Property cmd As String
            Get
                Return mcmd
            End Get
        End Property
        ''' <summary>
        ''' 实体类型（若被更新的是非方块，附加此信息）
        ''' </summary>
        Public ReadOnly Property actortype As String
            Get
                Return mactortype
            End Get
        End Property
        ''' <summary>
        ''' 命令方块所在位置
        ''' </summary>
        Public ReadOnly Property position As BPos3
            Get
                Return mposition
            End Get
        End Property
        ''' <summary>
        ''' 是否是方块
        ''' </summary>
        Public ReadOnly Property isblock As Boolean
            Get
                Return misblock
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As CommandBlockUpdateEvent
            Dim le = TryCast(createHead(e, EventType.onCommandBlockUpdate, GetType(CommandBlockUpdateEvent)), CommandBlockUpdateEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data
            le.loadData(s)
            le.mcmd = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            le.mactortype = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 48), IntPtr))
            le.mposition = CType(Marshal.PtrToStructure(s + 56, GetType(BPos3)), BPos3)
            le.misblock = Marshal.ReadByte(s, 68) = 1
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 玩家输入指令监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class InputCommandEvent
        Inherits PlayerEvent

        Protected mcmd As String
        ''' <summary>
        ''' 玩家输入的指令
        ''' </summary>
        Public ReadOnly Property cmd As String
            Get
                Return mcmd
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As InputCommandEvent
            Dim le = TryCast(createHead(e, EventType.onInputCommand, GetType(InputCommandEvent)), InputCommandEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data
            le.loadData(s)
            le.mcmd = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 命令方块(矿车)执行指令监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class BlockCmdEvent
        Inherits BaseEvent

        Protected mcmd As String
        Protected mname As String
        Protected mdimension As String
        Protected mposition As BPos3
        Protected mdimensionid As Integer
        Protected mtickdelay As Integer
        Protected mtype As Integer
        ''' <summary>
        ''' 将被执行的指令
        ''' </summary>
        Public ReadOnly Property cmd As String
            Get
                Return mcmd
            End Get
        End Property
        ''' <summary>
        ''' 执行者自定义名称
        ''' </summary>
        Public ReadOnly Property name As String
            Get
                Return mname
            End Get
        End Property
        ''' <summary>
        ''' 命令块所处维度
        ''' </summary>
        Public ReadOnly Property dimension As String
            Get
                Return mdimension
            End Get
        End Property
        ''' <summary>
        ''' 执行者所在位置
        ''' </summary>
        Public ReadOnly Property position As BPos3
            Get
                Return mposition
            End Get
        End Property
        ''' <summary>
        ''' 命令块所处维度ID
        ''' </summary>
        Public ReadOnly Property dimensionid As Integer
            Get
                Return mdimensionid
            End Get
        End Property
        ''' <summary>
        ''' 命令设定的间隔时间
        ''' </summary>
        Public ReadOnly Property tickdelay As Integer
            Get
                Return mtickdelay
            End Get
        End Property
        ''' <summary>
        ''' 执行者类型
        ''' </summary>
        Public ReadOnly Property type As Integer
            Get
                Return mtype
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As BlockCmdEvent
            Dim le = TryCast(createHead(e, EventType.onBlockCmd, GetType(BlockCmdEvent)), BlockCmdEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data ' 此处为转换过程
            le.mcmd = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            le.mname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            le.mdimension = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 16), IntPtr))
            le.mposition = CType(Marshal.PtrToStructure(s + 24, GetType(BPos3)), BPos3)
            le.mdimensionid = Marshal.ReadInt32(s, 36)
            le.mtickdelay = Marshal.ReadInt32(s, 40)
            le.mtype = Marshal.ReadInt32(s, 44)
            Return le
        End Function
    End Class

    Public Class NpcCmdEvent
        Inherits BaseEvent

        Protected mnpcname As String
        Protected mentity As String
        Protected mdimension As String
        Protected mactions As String
        Protected mposition As Vec3
        Protected mactionid As Integer
        Protected mentityid As Integer
        Protected mdimensionid As Integer
        ''' <summary>
        ''' NPC名字
        ''' </summary>
        Public ReadOnly Property npcname As String
            Get
                Return mnpcname
            End Get
        End Property
        ''' <summary>
        ''' NPC实体标识名
        ''' </summary>
        Public ReadOnly Property entity As String
            Get
                Return mentity
            End Get
        End Property
        ''' <summary>
        ''' NPC所处维度
        ''' </summary>
        Public ReadOnly Property dimension As String
            Get
                Return mdimension
            End Get
        End Property
        ''' <summary>
        ''' 指令列表
        ''' </summary>
        Public ReadOnly Property actions As String
            Get
                Return mactions
            End Get
        End Property
        ''' <summary>
        ''' NPC所在位置
        ''' </summary>
        Public ReadOnly Property position As Vec3
            Get
                Return mposition
            End Get
        End Property
        ''' <summary>
        ''' 选择项
        ''' </summary>
        Public ReadOnly Property actionid As Integer
            Get
                Return mactionid
            End Get
        End Property
        ''' <summary>
        ''' NPC实体标识ID
        ''' </summary>
        Public ReadOnly Property entityid As Integer
            Get
                Return mentityid
            End Get
        End Property
        ''' <summary>
        ''' NPC所处维度ID
        ''' </summary>
        Public ReadOnly Property dimensionid As Integer
            Get
                Return mdimensionid
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As NpcCmdEvent
            Dim le = TryCast(createHead(e, EventType.onNpcCmd, GetType(NpcCmdEvent)), NpcCmdEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data ' 此处为转换过程
            le.mnpcname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            le.mentity = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            le.mdimension = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 16), IntPtr))
            le.mactions = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 24), IntPtr))
            le.mposition = CType(Marshal.PtrToStructure(s + 32, GetType(Vec3)), Vec3)
            le.mactionid = Marshal.ReadInt32(s, 44)
            le.mentityid = Marshal.ReadInt32(s, 48)
            le.mdimensionid = Marshal.ReadInt32(s, 52)
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 加载名字监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class LoadNameEvent
        Inherits BaseEvent

        Protected mplayername As String
        Protected muuid As String
        Protected mxuid As String
        Protected mability As String
        ''' <summary>
        ''' 玩家名字
        ''' </summary>
        Public ReadOnly Property playername As String
            Get
                Return mplayername
            End Get
        End Property
        ''' <summary>
        ''' 玩家uuid字符串
        ''' </summary>
        Public ReadOnly Property uuid As String
            Get
                Return muuid
            End Get
        End Property
        ''' <summary>
        ''' 玩家xuid字符串
        ''' </summary>
        Public ReadOnly Property xuid As String
            Get
                Return mxuid
            End Get
        End Property
        ''' <summary>
        ''' 玩家能力值列表（可选，商业版可用）
        ''' </summary>
        Public ReadOnly Property ability As String
            Get
                Return mability
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As LoadNameEvent
            Dim le = TryCast(createHead(e, EventType.onLoadName, GetType(LoadNameEvent)), LoadNameEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data ' 此处为转换过程
            le.mplayername = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            le.muuid = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            le.mxuid = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 16), IntPtr))
            le.mability = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 24), IntPtr))
            Return le
        End Function
    End Class

    ''' <summary>
    ''' 离开游戏监听<br/>
    ''' 拦截可否：否
    ''' </summary>
    Public Class PlayerLeftEvent
        Inherits LoadNameEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As PlayerLeftEvent
            Dim le = TryCast(createHead(e, EventType.onPlayerLeft, GetType(PlayerLeftEvent)), PlayerLeftEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data  ' 此处为转换过程
            le.mplayername = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            le.muuid = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            le.mxuid = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 16), IntPtr))
            le.mability = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 24), IntPtr))
            Return le
        End Function
    End Class

    Public Class MoveEvent
        Inherits PlayerEvent

        Public Shared Overloads Function getFrom(ByVal e As Events) As MoveEvent
            Dim ate = TryCast(createHead(e, EventType.onMove, GetType(MoveEvent)), MoveEvent)
            If ate Is Nothing Then Return Nothing
            ate.loadData(e.data)
            Return ate
        End Function
    End Class

    ''' <summary>
    ''' 玩家攻击监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class AttackEvent
        Inherits PlayerEvent

        Protected mactorname As String
        Protected mactortype As String
        Protected mactorpos As Vec3
        ''' <summary>
        ''' 被攻击实体名称
        ''' </summary>
        Public ReadOnly Property actorname As String
            Get
                Return mactorname
            End Get
        End Property
        ''' <summary>
        ''' 被攻击实体类型
        ''' </summary>
        Public ReadOnly Property actortype As String
            Get
                Return mactortype
            End Get
        End Property
        ''' <summary>
        ''' 被攻击实体所处位置
        ''' </summary>
        Public ReadOnly Property actorpos As Vec3
            Get
                Return mactorpos
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As AttackEvent
            Dim ate = TryCast(createHead(e, EventType.onAttack, GetType(AttackEvent)), AttackEvent)
            If ate Is Nothing Then Return Nothing
            Dim s = e.data  ' 此处为转换过程
            ate.loadData(s)
            ate.mactorname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 40), IntPtr))
            ate.mactortype = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 48), IntPtr))
            ate.mactorpos = CType(Marshal.PtrToStructure(s + 56, GetType(Vec3)), Vec3)
            Return ate
        End Function
    End Class

    ''' <summary>
    ''' 世界范围爆炸监听<br/>
    ''' 拦截可否：是
    ''' </summary>
    Public Class LevelExplodeEvent
        Inherits BaseEvent

        Protected mentity As String
        Protected mblockname As String
        Protected mdimension As String
        Protected mposition As Vec3
        Protected mentityid As Integer
        Protected mdimensionid As Integer
        Protected mexplodepower As Single
        Protected mblockid As Short
        ''' <summary>
        ''' 爆炸者实体标识名（可能为空）
        ''' </summary>
        Public ReadOnly Property entity As String
            Get
                Return mentity
            End Get
        End Property
        ''' <summary>
        ''' 爆炸方块名（可能为空）
        ''' </summary>
        Public ReadOnly Property blockname As String
            Get
                Return mblockname
            End Get
        End Property
        ''' <summary>
        ''' 爆炸者所处维度
        ''' </summary>
        Public ReadOnly Property dimension As String
            Get
                Return mdimension
            End Get
        End Property
        ''' <summary>
        ''' 爆炸点所在位置
        ''' </summary>
        Public ReadOnly Property position As Vec3
            Get
                Return mposition
            End Get
        End Property
        ''' <summary>
        ''' 爆炸者实体标识ID
        ''' </summary>
        Public ReadOnly Property entityid As Integer
            Get
                Return mentityid
            End Get
        End Property
        ''' <summary>
        ''' 爆炸者所处维度ID
        ''' </summary>
        Public ReadOnly Property dimensionid As Integer
            Get
                Return mdimensionid
            End Get
        End Property
        ''' <summary>
        ''' 爆炸强度
        ''' </summary>
        Public ReadOnly Property explodepower As Single
            Get
                Return mexplodepower
            End Get
        End Property
        ''' <summary>
        ''' 爆炸方块ID
        ''' </summary>
        Public ReadOnly Property blockid As Single
            Get
                Return mblockid
            End Get
        End Property

        Public Shared Overloads Function getFrom(ByVal e As Events) As LevelExplodeEvent
            Dim le = TryCast(createHead(e, EventType.onLevelExplode, GetType(LevelExplodeEvent)), LevelExplodeEvent)
            If le Is Nothing Then Return Nothing
            Dim s = e.data  ' 此处为转换过程
            le.mentity = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 0), IntPtr))
            le.mblockname = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 8), IntPtr))
            le.mdimension = StrTool.readUTF8str(CType(Marshal.ReadInt64(s, 16), IntPtr))
            le.mposition = CType(Marshal.PtrToStructure(s + 24, GetType(Vec3)), Vec3)
            le.mentityid = Marshal.ReadInt32(s, 36)
            le.mdimensionid = Marshal.ReadInt32(s, 40)
            le.mexplodepower = StrTool.itof(Marshal.ReadInt32(s, 44))
            le.mblockid = Marshal.ReadInt16(s, 48)
            Return le
        End Function
    End Class
End Namespace
