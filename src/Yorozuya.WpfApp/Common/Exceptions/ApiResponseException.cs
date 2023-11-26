using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yorozuya.WpfApp.Common.Exceptions;

public class ApiResponseException(string message) : Exception(message)
{
}