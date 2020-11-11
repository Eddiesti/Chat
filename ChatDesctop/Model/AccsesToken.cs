using System;
using System.Collections.Generic;
using System.Text;

namespace ChatDesctop.Model
{
    public class AccessToken
    {
        public DateTime expiration { get; set; }
        public string token { get; set; }
    }
}
