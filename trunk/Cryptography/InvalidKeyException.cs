using System;
using System.Collections.Generic;
using System.Text;

namespace System.Common.Cryptography
{
    
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException(string message) : base(message)
        {
        }
    }
}
