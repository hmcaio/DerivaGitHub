using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facebook.Client;

namespace Deriva
{
    static class FBMan
    {
        public static readonly string FacebookAppId = "667040683345718";
        internal static string AccessToken = String.Empty;
        internal static string FacebookId = String.Empty;
        public static bool isAuthenticated = false;
        public static FacebookSessionClient FacebookSessionClient = new FacebookSessionClient(FacebookAppId);

        public static byte[] bytes;
    }
}
