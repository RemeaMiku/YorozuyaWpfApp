using System;

namespace Yorozuya.WpfApp.Common.Exceptions;

public class ApiResponseException(string message) : Exception(message)
{
}