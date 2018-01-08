using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QFrameworkOne.Module;

namespace PasswordNote
{
    class CAccount
    {
        public static string GetID() => HashHelper.Hash_MD5_16(DateTime.Now.ToString());
        public string ID;
        public string Platform;
        public string Account;
        public string Password;
        public string Remarks;
    }
}
