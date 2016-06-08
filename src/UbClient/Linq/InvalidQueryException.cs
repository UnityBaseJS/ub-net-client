using System;

namespace Softengi.UbClient.Linq
{
	internal class InvalidQueryException : Exception
	{
		public InvalidQueryException(string message)
		{
			_message = message + " ";
		}

		public override string Message => "The client query is invalid: " + _message;

		private readonly string _message;
	}
}