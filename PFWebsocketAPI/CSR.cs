 

namespace CSR
{
    partial class Plugin
    {
        /// <summary>
        /// 通用调用接口，需用户自行实现
        /// </summary>
        /// <param name="api">MC相关调用方法</param>
        public static void onStart(MCCSAPI api)
        {
            // TODO 此接口为必要实现
            PFWebsocketAPI.Program.Init(api);
        }
    }
}
