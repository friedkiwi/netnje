using System;

namespace JonSkeet.Ebcdic
{
	/// <summary>
	/// Exception thrown if the embedded resource describing the
	/// EBCDIC encodings is missing or invalid.
	/// </summary>
	internal class InvalidEbcdicDataException : Exception
	{
		internal InvalidEbcdicDataException(string reason) : base (reason)
		{
		}
	}
}
