using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PFWebsocketAPI.PFWebsocketAPI.Model;
using PFWebsocketBase;

namespace PFWebsocketAPI.PFWebsocketAPI
{
    internal static class IPTool
    {
        private readonly static Dictionary<IntPtr, string> IPList = new Dictionary<IntPtr, string>();

        internal static object GetPlayerIP(IntPtr ptr, bool refresh)
        {
            if (IPList.ContainsKey(ptr))
            {
                if (refresh)
                    IPList[ptr] = new CSR.CsPlayer(PFWebsocketAPI.Program.api, ptr).IpPort;
            }
            else
            {
                try
                {
                    IPList.Add(ptr, new CSR.CsPlayer(PFWebsocketAPI.Program.api, ptr).IpPort);
                }
                catch (NullReferenceException ex)
                {
                    IPList.Add(ptr, "unknown");
                }
            }

            return IPList[ptr];
        }

        internal static object GetPlayerIP(IntPtr ptr)
        {
            return GetPlayerIP(ptr, false);
        }
    }

    internal static class PackOperation
    {
        /// <summary>
        /// 读取元数据包
        /// </summary>
        /// <param name="message"></param>
        internal static void ReadPackInitial(string message, object con)
        {
            try
            {
                ReadPack(message, true, con);
            }
            catch (JsonReaderException ex)
            {
                string sendData = new CauseDecodeFailed("JSON格式错误:" + ex.Message).ToString();
                if (WSBASE.Config.EncryptDataSent)
                    sendData = new EncryptedPack(EncryptionMode.aes256, sendData, WSBASE.Config.Password).ToString();
                WSACT.SendToCon(con, sendData);
            }
            catch (Exception ex)
            {
                string sendData = new CauseDecodeFailed("解析错误:" + ex.Message).ToString();
                if (WSBASE.Config.EncryptDataSent)
                    sendData = new EncryptedPack(EncryptionMode.aes256, sendData, WSBASE.Config.Password).ToString();
                WSACT.SendToCon(con, sendData);
            }
        }

        internal static void ReadPackNext(string message, object con) // 继续解析数据包（如解密后）
        {
            ReadPack(message, false, con); // False即表示不是第一层级的数据包
        }

        internal static void ReadPack(string message, bool IsFirstLayer, object con)
        {
            var jobj = JObject.Parse(message);
            switch (Enum.Parse(typeof(PackType), jobj.Value<string>("type"), true))// 解析type对象，判断基础类型
            {
                case PackType.encrypted: // 作为加密包进行解密
                    {
                        ReadEncryptedPack(jobj, IsFirstLayer, con);
                        break;
                    }

                case PackType.pack: // 作为普通包解析
                    {
                        ReadOriginalPack(jobj, IsFirstLayer, con);
                        break;
                    }
            }
        }

        internal static void ReadEncryptedPack(JObject jobj, bool IsFirstLayer, object con) // 读取加密包
        {
            {
                var withBlock = new EncryptedPack(jobj);
                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                string decoded;
                try
                {
                    decoded = withBlock.Decode(WSBASE.Config.Password);
                }
                catch (Exception ex)
                {
                    var fb = new CauseDecodeFailed("密匙验证失败！");
                    PFWebsocketAPI.Program.WriteLine("密文解密失败，请检查密匙是否正确!");
                    WSACT.SendToCon(con, fb.ToString()); // 直接返回
                    return;
                }

                ReadPackNext(decoded, con); // 嵌套方法
            }
        }

        internal static void ReadOriginalPack(JObject jobj, bool IsFirstLayer, object con) // 读取普通包
        {
            /* TODO ERROR: Skipped IfDirectiveTrivia */
            if (IsFirstLayer) // 判断初始包，如果是未加密的初始包则不允许执行
            {
                var fb = new CauseInvalidRequest("未加密的初始包不予执行！");
                PFWebsocketAPI.Program.WriteLine("未加密的初始包不予执行!");
                WSACT.SendToCon(con, fb.ToString()); // 直接返回
                return;
            }
            /* TODO ERROR: Skipped EndIfDirectiveTrivia */
            switch (Enum.Parse(typeof(ClientActionType), jobj.Value<string>("action"), true))
            {
                case ClientActionType.runcmdrequest:
                    {
                        {
                            var withBlock = new ActionRunCmd(jobj, con); // 通过加载后的json初始化Action
                            var fb = withBlock.GetFeedback();
                            if (withBlock.@params.cmd.StartsWith("op ") || withBlock.@params.cmd.StartsWith("execute") && withBlock.@params.cmd.IndexOf("op ") != -1)
                            {
                                fb.@params.result = "出于安全考虑，禁止远程执行op命令";
                                PFWebsocketAPI.Program.WriteLine("出于安全考虑，禁止远程执行op命令");
                                string sendData = fb.ToString();
                                if (WSBASE.Config.EncryptDataSent)
                                    sendData = new EncryptedPack(EncryptionMode.aes256, sendData, WSBASE.Config.Password).ToString();
                                WSACT.SendToCon(con, sendData); // 直接返回
                            }

                            PFWebsocketAPI.Program.cmdQueue.Enqueue(fb);
                            PFWebsocketAPI.Program.CmdTimer_Elapsed(PFWebsocketAPI.Program.cmdTimer, null);
                            if (!PFWebsocketAPI.Program.cmdTimer.Enabled)
                                PFWebsocketAPI.Program.cmdTimer.Start();
                        }

                        break;
                    }

                case ClientActionType.broadcast:
                    {
                        break;
                    }
                // 待实现
                case ClientActionType.tellraw:
                    {
                        break;
                    }
                    // 待实现
            }
        }
    }
}

namespace PFWebsocketAPI.PFWebsocketAPI.Model
{
    public static class StringTools
    {
        public static string GetMD5(string sDataIn)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0, loopTo = bytHash.Length - 1; i <= loopTo; i++)
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            return sTemp.ToUpper();
        }

        public static object AESEncrypt(string content, string password)
        {
            string md5 = GetMD5(content);
            string iv = md5.Substring(16);
            string key = md5.Remove(16);
            return EasyEncryption.AES.Encrypt(content, key, iv);
        }

        public static object AESDecrypt(string content, string password)
        {
            string md5 = GetMD5(content);
            string iv = md5.Substring(16);
            string key = md5.Remove(16);
            return EasyEncryption.AES.Decrypt(content, key, iv);
        }
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))] // 基本包类型
    public enum PackType
    {
        pack,
        encrypted
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))] // 加密模式
    public enum EncryptionMode
    {
        aes256,
        aes_cbc_pck7padding
    }

    internal abstract class PackBase // 基础类
    {
        [JsonProperty(Order = -3)]
        public abstract PackType type { get; } // 包类型

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this); // 基类的转化为String重写方法
        }

        public T GetParams<T>(JObject json)
        {
            return json["params"].ToObject<T>(); // 基类的获取参数表方法
        }
    }

    internal class EncryptedPack : PackBase    // 加密包
    {
        public override string ToString()
        {
            return new JObject() { new JProperty("type", type), new JProperty("params", new JObject() { new JProperty("mode", @params.mode), new JProperty("raw", @params.raw) }) }.ToString(Formatting.None);
        }

        public override PackType type { get; private set; } = PackType.encrypted;

        public ParamMap @params;

        internal EncryptedPack(JObject json) // 通过已有json初始化对象（通常用作传入解析）
        {
            @params = GetParams<ParamMap>(json); // 通过基类该方法获取参数表
        }

        internal EncryptedPack(EncryptionMode mode, string from, string password) // 通过参数初始化包（通常用作发送前）
        {
            string encrypted = "";
            switch (mode)// 不同加密模式不同操作
            {
                case EncryptionMode.aes256:
                    {
                        encrypted = SimpleAES.AES256.Encrypt(from, password);
                        break;
                    }

                case EncryptionMode.aes_cbc_pck7padding:
                    {
                        encrypted = Conversions.ToString(StringTools.AESEncrypt(from, password));
                        break;
                    }
            }

            @params = new ParamMap() { mode = mode, raw = encrypted };
        }

        public string Decode(string password) // 解密params.raw中的内容并返回
        {
            string decrypted = "";
            switch (@params.mode)// 不同加密模式不同操作
            {
                case EncryptionMode.aes256:
                    {
                        decrypted = SimpleAES.AES256.Decrypt(@params.raw, password);
                        break;
                    }

                case EncryptionMode.aes_cbc_pck7padding:
                    {
                        decrypted = Conversions.ToString(StringTools.AESDecrypt(@params.raw, password));
                        break;
                    }
            }

            if (string.IsNullOrEmpty(decrypted))
                throw new Exception("AES256 Decode failed!");
            return decrypted;
        }

        internal class ParamMap // 对象参数表
        {
            public EncryptionMode mode;
            public string raw;
        }
    }

    internal class OriginalPack : PackBase   // 普通包/解密后的包
    {
        public override PackType type { get; private set; } = PackType.pack;
    }

    /* TODO ERROR: Skipped RegionDirectiveTrivia */
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum ServerCauseType
    {
        chat,
        join,
        left,
        cmd,
        runcmdfeedback,
        decodefailed,
        invalidrequest
    }

    internal abstract class ServerPackBase : OriginalPack
    {
        // <JsonProperty("cause")>
        [JsonProperty(Order = -2)]
        public abstract ServerCauseType cause { get; }
    }

    internal class CauseJoin : ServerPackBase
    {
        internal CauseJoin(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseJoin(string sender, string xuid, string uuid, string ip)
        {
            @params = new ParamMap() { sender = sender, xuid = xuid, uuid = uuid, ip = ip };
        }

        public override ServerCauseType cause { get; private set; } = ServerCauseType.join;

        public ParamMap @params;

        internal class ParamMap
        {
            public string sender, xuid, uuid, ip;
        }
    }

    internal class CauseLeft : ServerPackBase
    {
        internal CauseLeft(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseLeft(string sender, string xuid, string uuid, string ip)
        {
            @params = new ParamMap() { sender = sender, xuid = xuid, uuid = uuid, ip = ip };
        }

        public override ServerCauseType cause { get; private set; } = ServerCauseType.left;

        public ParamMap @params;

        internal class ParamMap
        {
            public string sender, xuid, uuid, ip;
        }
    }

    internal class CauseChat : ServerPackBase
    {
        internal CauseChat(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseChat(string sender, string text)
        {
            @params = new ParamMap() { sender = sender, text = text };
        }

        public override ServerCauseType cause { get; private set; } = ServerCauseType.chat;

        public ParamMap @params;

        internal class ParamMap
        {
            public string sender, text;
        }
    }

    internal class CauseCmd : ServerPackBase
    {
        internal CauseCmd(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseCmd(string sender, string text)
        {
            @params = new ParamMap() { sender = sender, text = text };
        }

        public override ServerCauseType cause { get; private set; } = ServerCauseType.cmd;

        public ParamMap @params;

        internal class ParamMap
        {
            public string sender, text;
        }
    }
    // 命令返回
    internal class CauseRuncmdFeedback : ServerPackBase
    {
        internal CauseRuncmdFeedback(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseRuncmdFeedback(string id, string cmd, string result, object con)
        {
            @params = new ParamMap() { id = id, cmd = cmd, result = result, con = con };
        }

        public override ServerCauseType cause { get; private set; } = ServerCauseType.runcmdfeedback;

        public ParamMap @params;

        internal class ParamMap
        {
            public string id;
            public string result;
            [JsonIgnore]
            internal string cmd;
            [JsonIgnore]
            internal int waiting = 0;
            [JsonIgnore]
            internal object con;
        }
    }

    internal class CauseDecodeFailed : ServerPackBase
    {
        internal CauseDecodeFailed(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseDecodeFailed(string msg)
        {
            @params = new ParamMap() { msg = msg };
        }

        public override ServerCauseType cause { get; private set; } = ServerCauseType.decodefailed;

        public ParamMap @params;

        internal class ParamMap
        {
            public string msg;
        }
    }

    internal class CauseInvalidRequest : ServerPackBase
    {
        internal CauseInvalidRequest(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseInvalidRequest(string msg)
        {
            @params = new ParamMap() { msg = msg };
        }

        public override ServerCauseType cause { get; private set; } = ServerCauseType.invalidrequest;

        public ParamMap @params;

        internal class ParamMap
        {
            public string msg;
        }
    }

    /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum ClientActionType
    {
        runcmdrequest,
        broadcast,
        tellraw
    }

    internal abstract class ClientPackBase : OriginalPack
    {
        [JsonProperty(Order = -2)]
        public abstract ClientActionType action { get; }
    }

    internal class ActionRunCmd : ClientPackBase
    {
        // Friend Sub New(json As JObject)
        // params = GetParams(Of ParamMap)(json)
        // End Sub
        public override string ToString()
        {
            return new JObject() { new JProperty("action", action), new JProperty("type", type), new JProperty("params", new JObject() { new JProperty("cmd", @params.cmd), new JProperty("id", @params.id) }) }.ToString(Formatting.None);
        }

        internal ActionRunCmd(JObject json, object con)
        {
            @params = GetParams<ParamMap>(json);
            @params.con = con;
        }

        internal ActionRunCmd(string cmd, string id, object con)
        {
            @params = new ParamMap() { cmd = cmd, id = id, con = con };
        }

        public override ClientActionType action { get; private set; } = ClientActionType.runcmdrequest;

        public ParamMap @params;

        internal class ParamMap
        {
            public string cmd, id;
            [JsonIgnore]
            internal object con;
        }

        internal CauseRuncmdFeedback GetFeedback()
        {
            return new CauseRuncmdFeedback(@params.id, @params.cmd, null, @params.con);
        }
    }
    /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
}