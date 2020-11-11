using System;

namespace ChatDesctop.Model
{
    public class AccessToken
    {
        public DateTime expiration { get; set; }
        public string token { get; set; }
    }
}
