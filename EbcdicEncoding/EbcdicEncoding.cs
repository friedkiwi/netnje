using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Globalization;

namespace JonSkeet.Ebcdic
{
	/// <summary>
	/// EBCDIC encoding class. Instances of this class are obtained using
	/// the static GetEncoding method - the names of all supported encodings
	/// are returned by the AllNames property. When encoding, characters
	/// which aren't in the specified EBCDIC set are converted to the byte
	/// representing '?' in the specified EBCDIC set if available, or 0 otherwise.
	/// When decoding, bytes which aren't a recognised part of the specified
	/// EBCDIC encoding are decoded as '?'. Shift-in and shift-out are
	/// mapped directly to their unicode values, with no actual shifting.
	/// This class is thus unsafe when used to decode byte streams which
	/// rely on shifting into a double-byte character set. All methods
	/// in this class are thread-safe.
	/// </summary>
    public class EbcdicEncoding : Encoding
    {
        #region Instance fields
        /// <summary>
        /// Map from each byte to the relevant char. A value of 0
        /// (aside from for element 0) indicates that the byte is
        /// not specified as part of the encoding. This is specified
        /// in the constructor.
        /// </summary>
        readonly char[] byteToCharMap;
        /// <summary>
        /// The character to byte maps for each character. 
        /// Each element in this array is either an array of 256 
        /// bytes, or null. To find the appropriate map
        /// for a character, only consider its high 8 bits. If a map
        /// exists for that element, the byte to use is the element
        /// within that map for the character's low 8 bits. If the
        /// byte is 0, that indicates no mapping for the character,
        /// as does the map itself being null for that block of
        /// 256 characters. Effectively, this provides a reasonably
        /// fast sparse map implementation.
        /// </summary>
        readonly byte[][] charBlockToByteBlockMap = new byte[256][];
        string name;

        /// <summary>
        /// Byte returned when a character which doesn't
        /// appear in the encoding is presented for encoding.
        /// Set in the constructor, after the character maps
        /// have been loaded.
        /// </summary>
        byte unknownCharacterByte;
        #endregion

        #region Static fields
        /// <summary>
        /// The names of all available encodings.
        /// </summary>
        static string[] allNames;
        /// <summary>
        /// A map from name to encoding. The name is
        /// in upper case, to allow easy case-insensitive matching
        /// in GetEncoding.
        /// </summary>
        static readonly IDictionary encodingMap=new Hashtable();
        #endregion

        #region Informational properties
        /// <summary>
        /// Returns the name of this encoding, which will start with "EBCDIC-"
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Read-only property returning the names of all valid EBCDIC encodings
        /// which can be requested.
        /// </summary>
        public static string[] AllNames
        {
            get
            {
                return (string[]) allNames.Clone();
            }
        }
        #endregion

        #region Method to fetch the specified encoding
        /// <summary>
        /// Returns an EbcdicEncoding for the specified name.
        /// </summary>
        /// <param name="name">The name of an EbcdicEncoding.</param>
        /// <returns>The requested encoding.</returns>
        /// <exception cref="NotSupportedException">
        /// The specified encoding name is not supported.
        /// </exception>
        public static new EbcdicEncoding GetEncoding(string name)
        {
            EbcdicEncoding ret = (EbcdicEncoding) encodingMap[name.ToUpper(CultureInfo.InvariantCulture)];
            if (ret==null)
                throw new NotSupportedException("No EBCDIC encoding named "+name+" found.");
            return ret;
        }
        #endregion

        #region Loading of supported encodings (triggered by type initialization)
        static EbcdicEncoding()
        {
            // Just in case an exception is thrown, ignored, and then
            // further attempts are made to use the class...
            allNames = new string[0];

            using (Stream data = Assembly.GetExecutingAssembly().
                GetManifestResourceStream("netnje.EbcdicEncoding.ebcdic.dat"))
            {
                if (data==null)
                    throw new InvalidEbcdicDataException ("EBCDIC encodings resource not found.");
                
                int encodings = data.ReadByte();
                if (encodings==-1)
                {
                    throw new InvalidEbcdicDataException ("EBCDIC encodings resource empty.");
                }

                allNames = new string[encodings];
                for (int i=0; i < encodings; i++)
                {
                    int nameLength = data.ReadByte();
                    if (nameLength==-1)
                    {
                        throw new InvalidEbcdicDataException ("EBCDIC encodings resource truncated.");
                    }
                    string name = "EBCDIC-"+
                        Encoding.ASCII.GetString (ReadFully(data, nameLength), 0, nameLength);
                    allNames[i]=name;
                    byte[] rawMap = ReadFully (data, 512);
                    char[] map = new char[256];
                    for (int j=0; j < 256; j++)
                    {
                        map[j]=(char)((rawMap[j*2]<<8) | rawMap[j*2+1]);
                    }
                    encodingMap[name.ToUpper(CultureInfo.InvariantCulture)]=new EbcdicEncoding(name, map);
                }
                if (data.ReadByte()!=-1)
                    throw new InvalidEbcdicDataException("EBCDIC encodings resource contains unused data.");
            }
        }
        #endregion

        #region Construction
		EbcdicEncoding(string name, char[] byteToCharMap)
		{
            this.name = name;
            this.byteToCharMap = byteToCharMap;
            ConstructCharToByteMaps();
            // This ends up with unknownCharacterByte either as 0
            // or the encoded form for '?', depending on whether or
            // not '?' is in the character set.
            unknownCharacterByte=Encode ('?');
		}

        /// <summary>
        /// Constructs the reverse mapping to easily map from
        /// a character to a byte.
        /// </summary>
        void ConstructCharToByteMaps()
        {
            for (int i=0; i < 256; i++)
            {
                char c = byteToCharMap[i];
                byte[] map = charBlockToByteBlockMap[c>>8];
                if (map==null)
                {
                    map = new byte[256];
                    charBlockToByteBlockMap[c>>8]=map;
                }
                map[c&0xff]=(byte)i;
            }
        }
        #endregion

        #region Actual encoding and decoding methods
        /// <summary>
        /// Encodes a single character to a byte. If the character doesn't
        /// appear in the encoded character set, the byte representing
        /// '?' is returned if '?' appears in the encoded character set, or
        /// 0 otherwise.
        /// </summary>
        /// <param name="character">The character to encode.</param>
        /// <returns>The single byte encoded representation of the character.</returns>
        byte Encode (char character)
        {
            byte ret;
            byte[] map = charBlockToByteBlockMap[character>>8];
            if (map==null)
                ret=0;
            else
                ret=map[character&0xff];
            if (ret==0 && character != 0)
                return unknownCharacterByte;
            return ret;
        }

        /// <summary>
        /// Decodes a single byte to a character . If the byte is unknown
        /// in this encoding, '?' is returned.
        /// </summary>
        /// <param name="byteValue">The byte to decode.</param>
        /// <returns>The decoded character.</returns>
        char Decode (byte byteValue)
        {
            char ret = byteToCharMap[byteValue];
            // Check for unknown character
            if (ret==0 && byteValue != 0)
                ret = '?';
            return ret;
        }
        #endregion

        #region System.Text.Encoding methods
        /// <summary>
        /// Returns the number of bytes required to encode a range of characters 
        /// in the specified character array.
        /// </summary>
        /// <param name="chars">The character array to encode.</param>
        /// <param name="index">The starting index of the character array to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>The number of bytes required to encode the specified range of characters.</returns>
        /// <exception cref="ArgumentNullException">chars is a null reference</exception>
        /// <exception cref="ArgumentOutOfRangeException">index or count is less than zero,
        /// or index and count do not denote a valid range in the character array.
        /// </exception>
        public override int GetByteCount (char[] chars, int index, int count)
        {
            ValidateParameters (chars, index, count, "GetByteCount");
            return count;
        }

        /// <summary>
        /// Encodes a range of characters from a character array into a byte array.
        /// </summary>
        /// <param name="chars">The character array to encode.</param>
        /// <param name="charIndex">The starting index of the character array to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The byte array where the resulting encoding is stored.</param>
        /// <param name="byteIndex">The starting index of the resulting encoding in the byte array.</param>
        /// <returns>The number of bytes stored in array bytes.</returns>
        /// <exception cref="ArgumentException">
        /// bytes does not contain sufficient space to store the encoded characters.
        /// </exception>
        /// <exception cref="ArgumentNullException">Either chars or bytes is a null reference</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// charIndex, charCount or byteIndex is less than zero, or charIndex and charCount do not 
        /// denote a valid range in the character array.
        /// </exception>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            ValidateParameters (chars, charIndex, charCount, "GetBytes");
            ValidateParameters (bytes, byteIndex, 0, "GetBytes");
            if (byteIndex+charCount > bytes.Length)
                throw new ArgumentException ("Byte array passed to GetBytes is too short");

            for (int i=0; i < charCount; i++)
                bytes[byteIndex+i] = Encode(chars[charIndex+i]);

            return charCount;
        }
        
        /// <summary>
        /// Returns the maximum number of bytes required to encode a given number of characters.
        /// </summary>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <returns>The maximum number of bytes required for encoding a given
        /// number of characters.</returns>
        public override int GetMaxByteCount (int charCount)
        {
            return charCount;
        }

        /// <summary>
        /// Returns the number of characters produced by decoding a range of elements
        /// in the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array to decode.</param>
        /// <param name="index">The starting index of the byte array to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>The number of characters produced by decoding a range of bytes in 
        /// the specified byte array.</returns>
        /// <exception cref="ArgumentNullException">chars is a null reference</exception>
        /// <exception cref="ArgumentOutOfRangeException">index or count is less than zero,
        /// or index and count do not denote a valid range in the character array.
        /// </exception>
        public override int GetCharCount (byte[] bytes, int index, int count)
        {
            ValidateParameters (bytes, index, count, "GetCharCount");
            return count;
        }

        /// <summary>
        /// Decodes a range of bytes in a byte array into a range of characters in a character array.
        /// </summary>
        /// <param name="bytes">The byte array to decode.</param>
        /// <param name="byteIndex">The starting index of the byte array to decode.</param>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <param name="chars">The character array where the resulting decoding is stored.</param>
        /// <param name="charIndex">The starting index of the resulting decoding in the character array.</param>
        /// <returns>The number of bytes stored in array bytes.</returns>
        /// <exception cref="ArgumentException">
        /// chars does not contain sufficient space to store the decoded characters.
        /// </exception>
        /// <exception cref="ArgumentNullException">Either chars or bytes is a null reference</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// byteIndex, byteCount or charIndex is less than zero, or byteIndex and byteCount do not 
        /// denote a valid range in the character array.
        /// </exception>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            ValidateParameters (bytes, byteIndex, byteCount, "GetChars");
            ValidateParameters (chars, charIndex, 0, "GetChars");
            if (charIndex+byteCount > chars.Length)
                throw new ArgumentException ("Character array passed to GetChars is too short");

            for (int i=0; i < byteCount; i++)
                chars[charIndex+i] = Decode(bytes[byteIndex+i]);

            return byteCount;
        }
        
        /// <summary>
        /// Returns the maximum number of characted producted by decoding a given number of bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <returns>The maximum number of characters produced by decoding a given
        /// number of bytes.</returns>
        public override int GetMaxCharCount (int byteCount)
        {
            return byteCount;
        }
        #endregion

        #region Utility methods
        static void ValidateParameters (Array array, int index, int count, string methodName)
        {
            if (array==null)
                throw new ArgumentNullException ("Null array passed to "+methodName);
            if (index < 0)
                throw new ArgumentOutOfRangeException ("Negative index passed to "+methodName);
            if (count < 0)
                throw new ArgumentOutOfRangeException ("Negative count passed to "+methodName);
            if (index + count > array.Length)
                throw new ArgumentOutOfRangeException ("index+count > length in "+methodName);
        }

        /// <summary>
        /// Reads the specified amount of data from the stream, throwing an exception
        /// if the end of the stream is reached first.
        /// </summary>
        /// <param name="stream">Stream to read data from</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>The data from the stream as a byte array</returns>
        static byte[] ReadFully (Stream stream, int count)
        {
            byte[] ret = new byte[count];
            int off=0;
            while (off < count)
            {
                int len = stream.Read (ret, off, count-off);
                if (len <= 0)
                    throw new InvalidEbcdicDataException("EBCDIC encodings resource truncated.");
                off += len;
            }
            return ret;
        }
        #endregion
	}
}
