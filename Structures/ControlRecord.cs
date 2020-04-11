using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JonSkeet.Ebcdic;
using System.Net;


namespace netnje.Structures
{
    class ControlRecord
    {
		/*
		 *  Original C structure
		 * 
			#define	VMctl_SIZE	33
			struct	VMctl {			
				unsigned char type[8], 
				Rhost[8];   
				u_int32 Rip;        
				unsigned char Ohost[8]; 
				u_int32 Oip;        
				unsigned char R;        
			};
		 */

		public string RequestType { get; set; }
		public string SenderName { get; set; }
		public string SenderIP { get; set; }
		public string ReceiverName { get; set; }
		public string ReceiverIP { get; set; }
		public byte ReasonCode { get; set; }

		public ControlRecord(string RequestType, string SenderName, string SenderIP, string ReceiverName, string ReceiverIP, byte ReasonCode)
		{
			this.RequestType = RequestType;
			this.SenderName = SenderName;
			this.SenderIP = SenderIP;
			this.ReceiverName = ReceiverName;
			this.ReceiverIP = ReceiverIP;
			this.ReasonCode = ReasonCode;
		}

		public ControlRecord(byte[] ControlRecordBytes)
		{
			ParseControlRecord(ControlRecordBytes);
		}

		public void ParseControlRecord(byte[] ControlRecordBytes)
		{
			if (ControlRecordBytes.Length != 33)
			{
				throw new FormatException("Invalid ControlRecord structure.");
			}

			// allocate arrays to ingest the record
			byte[] requestType = new byte[8];
			byte[] senderName = new byte[8];
			byte[] receiverName = new byte[8];
			byte[] senderIP = new byte[4];
			byte[] receiverIP = new byte[4];

			Array.Copy(ControlRecordBytes, 0, requestType, 0, 8);
			Array.Copy(ControlRecordBytes, 8, senderName, 0, 8);
			Array.Copy(ControlRecordBytes, 20, receiverName, 0, 8);
			Array.Copy(ControlRecordBytes, 16, senderIP, 0, 4);
			Array.Copy(ControlRecordBytes, 28, senderIP, 0, 4);

			//de-EBCDIC-ize
			requestType = EbcdicEncoding.Convert(EbcdicEncoding.GetEncoding("EBCDIC-US"), Encoding.ASCII, requestType);
			senderName = EbcdicEncoding.Convert(EbcdicEncoding.GetEncoding("EBCDIC-US"), Encoding.ASCII, senderName);
			receiverName = EbcdicEncoding.Convert(EbcdicEncoding.GetEncoding("EBCDIC-US"), Encoding.ASCII, receiverName);

			this.RequestType = ASCIIEncoding.ASCII.GetString(requestType);
			this.SenderName = ASCIIEncoding.ASCII.GetString(senderName);
			this.ReceiverName = ASCIIEncoding.ASCII.GetString(receiverName);

			// get IPs
			IPAddress senderIPaddr = new IPAddress(senderIP);
			IPAddress receiverIPaddr = new IPAddress(receiverIP);

			this.ReceiverIP = receiverIPaddr.ToString();
			this.SenderIP = senderIPaddr.ToString();

			// get Reason Code
			this.ReasonCode = ControlRecordBytes[32];
		}

		public byte[] GetBytes()
		{
			byte[] controlRecordBytes = new byte[33];

			// encode control record bytes

			byte[] requestType = Encoding.ASCII.GetBytes(this.RequestType);
			byte[] senderName = Encoding.ASCII.GetBytes(this.SenderName);
			byte[] receiverName = Encoding.ASCII.GetBytes(this.ReceiverName);
			byte[] senderIP = BitConverter.GetBytes(Utils.stringToNjeIP(this.SenderIP));
			byte[] receiverIP = BitConverter.GetBytes(Utils.stringToNjeIP(this.ReceiverIP));
			requestType = EbcdicEncoding.Convert(Encoding.ASCII, EbcdicEncoding.GetEncoding("EBCDIC-US"), requestType);
			senderName = EbcdicEncoding.Convert(Encoding.ASCII, EbcdicEncoding.GetEncoding("EBCDIC-US"), senderName);
			receiverName = EbcdicEncoding.Convert(Encoding.ASCII, EbcdicEncoding.GetEncoding("EBCDIC-US"), receiverName);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(senderIP);
				Array.Reverse(receiverIP);
			}

			// create padded versions where necessary

			byte[] requestTypePad = { 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40 };
			byte[] senderNamePad = { 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40 };
			byte[] receiverNamePad = { 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40 };

			Array.Copy(requestType, requestTypePad, requestType.Length <= 8 ? requestType.Length : 8);
			Array.Copy(senderName, senderNamePad, requestType.Length <= 8 ? senderName.Length : 8);
			Array.Copy(receiverName, receiverNamePad, requestType.Length <= 8 ? receiverName.Length : 8);

			// assemble control record

			Array.Copy(requestTypePad, 0, controlRecordBytes, 0, 8);
			Array.Copy(senderNamePad, 0, controlRecordBytes, 8, 8);
			Array.Copy(senderIP, 0, controlRecordBytes, 16, 4);
			Array.Copy(receiverNamePad, 0, controlRecordBytes, 20, 8);
			Array.Copy(receiverIP, 0, controlRecordBytes, 28, 4);

			controlRecordBytes[32] = ReasonCode;

			return controlRecordBytes;
		}
    }
}
