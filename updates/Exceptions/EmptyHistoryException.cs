using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace updates.Exceptions
{
    class EmptyHistoryException : Exception
    {
        public EmptyHistoryException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
