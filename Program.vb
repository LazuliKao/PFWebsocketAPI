Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Text
Imports CSR
Imports System.Web.Script.Serialization

Namespace PFWebsocketAPI
    Friend Class Program
        Private Shared mcapi As MCCSAPI = Nothing

        Public Shared Sub init(ByVal api As MCCSAPI)
            mcapi = api
            Console.OutputEncoding = Encoding.UTF8
            ' 后台指令监听
            api.addBeforeActListener(onServerCmd, Function(x)
                                                      Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                      Dim se = TryCast(BaseEvent.getFrom(x), ServerCmdEvent)

                                                      If se IsNot Nothing Then
                                                          Console.WriteLine("后台指令={0}", se.cmd)
                                                      End If

                                                      Return True
                                                  End Function)
            ' 后台指令输出监听
            api.addBeforeActListener(onServerCmdOutput, Function(x)
                                                            Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                            Dim se = TryCast(BaseEvent.getFrom(x), ServerCmdOutputEvent)

                                                            If se IsNot Nothing Then
                                                                Console.WriteLine("后台指令输出={0}", se.output)
                                                            End If

                                                            Return True
                                                        End Function)
            ' 表单选择监听
            api.addAfterActListener(onFormSelect, Function(x)
                                                      Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                      Dim fe = TryCast(BaseEvent.getFrom(x), FormSelectEvent)
                                                      If fe Is Nothing Then Return True

                                                      If Not Equals(fe.selected, "null") Then
                                                          Console.WriteLine("玩家 {0} 选择了表单 id={1} ，selected={2}", fe.playername, fe.formid, fe.selected)
                                                      Else
                                                          Console.WriteLine("玩家 {0} 取消了表单 id={1}", fe.playername, fe.formid)
                                                      End If

                                                      Return False
                                                  End Function)
            ' 使用物品监听
            api.addAfterActListener(onUseItem, Function(x)
                                                   Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                   Dim ue = TryCast(BaseEvent.getFrom(x), UseItemEvent)

                                                   If ue IsNot Nothing AndAlso ue.RESULT Then
                                                       Console.WriteLine("玩家 {0} 对 {1} 的 ({2}, {3}, {4}) 处的 {5} 方块" & "操作了 {6} 物品。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname, ue.itemname)
                                                   End If

                                                   Return True
                                               End Function)
            ' 放置方块监听
            api.addAfterActListener(onPlacedBlock, Function(x)
                                                       Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                       Dim ue = TryCast(BaseEvent.getFrom(x), PlacedBlockEvent)

                                                       If ue IsNot Nothing AndAlso ue.RESULT Then
                                                           Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" & " 处放置了 {5} 方块。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname)
                                                       End If

                                                       Return True
                                                   End Function)
            ' 破坏方块监听
            api.addBeforeActListener(onDestroyBlock, Function(x)
                                                         Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                         Dim ue = TryCast(BaseEvent.getFrom(x), DestroyBlockEvent)

                                                         If ue IsNot Nothing Then
                                                             Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" & " 处破坏 {5} 方块。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname)
                                                         End If

                                                         Return True
                                                     End Function)
            ' 开箱监听
            api.addBeforeActListener(onStartOpenChest, Function(x)
                                                           Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                           Dim ue = TryCast(BaseEvent.getFrom(x), StartOpenChestEvent)

                                                           If ue IsNot Nothing Then
                                                               Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" & " 处打开 {5} 箱子。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname)
                                                           End If

                                                           Return True
                                                       End Function)
            ' 开桶监听
            api.addBeforeActListener(onStartOpenBarrel, Function(x)
                                                            Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                            Dim ue = TryCast(BaseEvent.getFrom(x), StartOpenBarrelEvent)

                                                            If ue IsNot Nothing Then
                                                                Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" & " 处打开 {5} 木桶。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname)
                                                            End If

                                                            Return True
                                                        End Function)
            ' 关箱监听
            api.addAfterActListener(onStopOpenChest, Function(x)
                                                         Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                         Dim ue = TryCast(BaseEvent.getFrom(x), StopOpenChestEvent)

                                                         If ue IsNot Nothing Then
                                                             Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" & " 处关闭 {5} 箱子。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname)
                                                         End If

                                                         Return True
                                                     End Function)
            ' 关桶监听
            api.addAfterActListener(onStopOpenBarrel, Function(x)
                                                          Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                          Dim ue = TryCast(BaseEvent.getFrom(x), StopOpenBarrelEvent)

                                                          If ue IsNot Nothing Then
                                                              Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" & " 处关闭 {5} 木桶。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname)
                                                          End If

                                                          Return True
                                                      End Function)
            ' 放入取出监听
            api.addAfterActListener(onSetSlot, Function(x)
                                                   Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                   Dim e = TryCast(BaseEvent.getFrom(x), SetSlotEvent)

                                                   If e IsNot Nothing Then
                                                       If e.itemcount > 0 Then
                                                           Console.WriteLine("玩家 {0} 在 {1} 槽放入了 {2} 个 {3} 物品。", e.playername, e.slot, e.itemcount, e.itemname)
                                                       Else
                                                           Console.WriteLine("玩家 {0} 在 {1} 槽取出了物品。", e.playername, e.slot)
                                                       End If
                                                   End If

                                                   Return True
                                               End Function)
            ' 切换维度监听
            api.addAfterActListener(onChangeDimension, Function(x)
                                                           Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                           Dim e = TryCast(BaseEvent.getFrom(x), ChangeDimensionEvent)

                                                           If e IsNot Nothing AndAlso e.RESULT Then
                                                               Console.WriteLine("玩家 {0} {1} 切换维度至 {2} 的 ({3},{4},{5}) 处。", e.playername, If(e.isstand, "", "悬空地"), e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z)
                                                           End If

                                                           Return True
                                                       End Function)
            ' 生物死亡监听
            api.addAfterActListener(onMobDie, Function(x)
                                                  Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                  Dim e = TryCast(BaseEvent.getFrom(x), MobDieEvent)

                                                  If e IsNot Nothing AndAlso Not String.IsNullOrEmpty(e.mobname) Then
                                                      Console.WriteLine(" {0} 在 {1} ({2:F2},{3:F2},{4:F2}) 处被 {5} 杀死了。", e.mobname, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z, e.srcname)
                                                  End If

                                                  Return True
                                              End Function)
            ' 玩家重生监听
            api.addAfterActListener(onRespawn, Function(x)
                                                   Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                   Dim e = TryCast(BaseEvent.getFrom(x), RespawnEvent)

                                                   If e IsNot Nothing AndAlso e.RESULT Then
                                                       Console.WriteLine("玩家 {0} 已于 {1} 的 ({2:F2},{3:F2},{4:F2}) 处重生。", e.playername, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z)
                                                   End If

                                                   Return True
                                               End Function)
            ' 聊天监听
            api.addAfterActListener(onChat, Function(x)
                                                Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                Dim e = TryCast(BaseEvent.getFrom(x), ChatEvent)

                                                If e IsNot Nothing Then
                                                    Console.WriteLine(" {0} {1} 说：{2}", e.playername, If(Not String.IsNullOrEmpty(e.target), "悄悄地对 " & e.target, ""), e.msg)
                                                End If

                                                Return True
                                            End Function)
            ' 输入文本监听
            api.addBeforeActListener(onInputText, Function(x)
                                                      Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                      Dim e = TryCast(BaseEvent.getFrom(x), InputTextEvent)

                                                      If e IsNot Nothing Then
                                                          Console.WriteLine(" <{0}> {1}", e.playername, e.msg)
                                                      End If

                                                      Return True
                                                  End Function)
            ' 输入指令监听
            api.addBeforeActListener(onInputCommand, Function(x)
                                                         Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                         Dim e = TryCast(BaseEvent.getFrom(x), InputCommandEvent)

                                                         If e IsNot Nothing Then
                                                             Console.WriteLine(" <{0}> {1}", e.playername, e.cmd)
                                                         End If

                                                         Return True
                                                     End Function)

            ' 世界范围爆炸监听，拦截
            api.addBeforeActListener(onLevelExplode, Function(x)
                                                         Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                         Dim e = TryCast(BaseEvent.getFrom(x), LevelExplodeEvent)

                                                         If e IsNot Nothing Then
                                                             Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图发生强度 {5} 的爆炸。", e.dimension, e.position.x, e.position.y, e.position.z, If(String.IsNullOrEmpty(e.entity), e.blockname, e.entity), e.explodepower)
                                                         End If

                                                         Return False
                                                     End Function)
            ' 
            ' 			// 玩家移动监听
            ' 			api.addAfterActListener(EventKey.onMove, x => {
            ' 				var e = BaseEvent.getFrom(x) as MoveEvent;
            ' 				if (e != null) {
            ' 					Console.WriteLine("玩家 {0} {1} 移动至 {2} ({3},{4},{5}) 处。",
            ' 						e.playername, (e.isstand) ? "":"悬空地", e.dimension,
            ' 						e.XYZ.x, e.XYZ.y, e.XYZ.z);
            ' 				}
            ' 				return false;
            ' 			});
            ' 			
            ' 玩家加入游戏监听
            api.addAfterActListener(onLoadName, Function(x)
                                                    Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                    Dim ue = TryCast(BaseEvent.getFrom(x), LoadNameEvent)

                                                    If ue IsNot Nothing Then
                                                        Console.WriteLine("玩家 {0} 加入了游戏，xuid={1}", ue.playername, ue.xuid)
                                                    End If

                                                    Return True
                                                End Function)
            ' 玩家离开游戏监听
            api.addAfterActListener(onPlayerLeft, Function(x)
                                                      Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                      Dim ue = TryCast(BaseEvent.getFrom(x), PlayerLeftEvent)

                                                      If ue IsNot Nothing Then
                                                          Console.WriteLine("玩家 {0} 离开了游戏，xuid={1}", ue.playername, ue.xuid)
                                                      End If

                                                      Return True
                                                  End Function)

            ' 攻击监听
            ' API 方式注册监听器
            api.addAfterActListener(onAttack, Function(x)
                                                  Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                  Dim ae As AttackEvent = TryCast(BaseEvent.getFrom(x), AttackEvent)

                                                  If ae IsNot Nothing Then
                                                      Dim str = "玩家 " & ae.playername & " 在 (" & ae.XYZ.x.ToString("F2") & "," & ae.XYZ.y.ToString("F2") & "," & ae.XYZ.z.ToString("F2") & ") 处攻击了 " & ae.actortype & " 。"
                                                      Console.WriteLine(str)
                                                      'Console.WriteLine("list={0}", api.getOnLinePlayers());
                                                      Dim ols As String = api.getOnLinePlayers()

                                                      If Not String.IsNullOrEmpty(ols) Then
                                                          Dim ser As JavaScriptSerializer = New JavaScriptSerializer()
                                                          Dim al = ser.Deserialize(Of ArrayList)(ols)
                                                          Dim uuid As Object = Nothing

                                                          For Each p As Dictionary(Of String, Object) In al
                                                              Dim name As Object

                                                              If p.TryGetValue("playername", name) Then
                                                                  If Equals(CStr(name), ae.playername) Then
                                                                      ' 找到
                                                                      p.TryGetValue("uuid", uuid)
                                                                      Exit For
                                                                  End If
                                                              End If
                                                          Next

                                                          If uuid IsNot Nothing Then
                                                              Dim id = api.sendSimpleForm(CStr(uuid), "致命选项", "test choose:", "[""生存"",""死亡"",""求助""]")
                                                              Console.WriteLine("创建需自行保管的表单，id={0}", id)
                                                              'api.transferserver((string)uuid, "www.xiafox.com", 19132);
                                                          End If
                                                      End If
                                                  Else
                                                      Console.WriteLine("Event convent fail.")
                                                  End If

                                                  Return True
                                              End Function)
#Region "非社区部分内容"
            If api.COMMERCIAL Then
                ' 生物伤害监听
                api.addBeforeActListener(onMobHurt, Function(x)
                                                        Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                        Dim e = TryCast(BaseEvent.getFrom(x), MobHurtEvent)

                                                        If e IsNot Nothing AndAlso Not String.IsNullOrEmpty(e.mobname) Then
                                                            Console.WriteLine(" {0} 在 {1} ({2:F2},{3:F2},{4:F2}) 即将受到来自 {5} 的 {6} 点伤害，类型 {7}", e.mobname, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z, e.srcname, e.dmcount, e.dmtype)
                                                        End If

                                                        Return True
                                                    End Function)
                ' 命令块执行指令监听，拦截
                api.addBeforeActListener(onBlockCmd, Function(x)
                                                         Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                         Dim e = TryCast(BaseEvent.getFrom(x), BlockCmdEvent)

                                                         If e IsNot Nothing Then
                                                             Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图执行指令 {5}", e.dimension, e.position.x, e.position.y, e.position.z, e.name, e.cmd)
                                                         End If

                                                         Return False
                                                     End Function)
                ' NPC执行指令监听，拦截
                api.addBeforeActListener(onNpcCmd, Function(x)
                                                       Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                       Dim e = TryCast(BaseEvent.getFrom(x), NpcCmdEvent)

                                                       If e IsNot DirectCast(Nothing, Object) Then
                                                           Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图执行第 {5} 条指令，指令集" & Microsoft.VisualBasic.Constants.vbLf & "{6}", e.dimension, e.position.x, e.position.y, e.position.z, e.npcname, e.actionid, e.actions)
                                                       End If

                                                       Return False
                                                   End Function)
                ' 更新命令方块监听
                api.addBeforeActListener(onCommandBlockUpdate, Function(x)
                                                                   Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result)
                                                                   Dim e = TryCast(BaseEvent.getFrom(x), CommandBlockUpdateEvent)

                                                                   If e IsNot Nothing Then
                                                                       Console.WriteLine(" {0} 试图修改位于 {1} ({2},{3},{4}) 的 {5} 的命令为 {6}", e.playername, e.dimension, e.position.x, e.position.y, e.position.z, If(e.isblock, "命令块", "命令矿车"), e.cmd)
                                                                   End If

                                                                   Return True
                                                               End Function)
            End If
#End Region


            ' Json 解析部分 使用JavaScriptSerializer序列化Dictionary或array即可

            'JavaScriptSerializer ser = new JavaScriptSerializer();
            'var data = ser.Deserialize<Dictionary<string, object>>("{\"x\":9}");
            'var ary = ser.Deserialize<ArrayList>("[\"x\",\"y\"]");
            'Console.WriteLine(data["x"]);
            'foreach(string v in ary) {
            '	Console.WriteLine(v);
            '}
            'data["y"] = 8;
            'string dstr = ser.Serialize(data);
            'Console.WriteLine(dstr);

            ' 高级玩法，硬编码方式注册hook
            THook.init(api)
        End Sub
    End Class
End Namespace

Namespace CSR
    Partial Class Plugin
        ''' <summary>
        ''' 通用调用接口，需用户自行实现
        ''' </summary>
        ''' <paramname="api">MC相关调用方法</param>
        Public Shared Sub onStart(ByVal api As MCCSAPI)
            ' TODO 此接口为必要实现
            PFWebsocketAPI.Program.init(api)
            Console.WriteLine("[Demo] CSR测试插件已装载。")
        End Sub
    End Class
End Namespace
