using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		}

		public byte[] GetBytes()
		{

		}
    }
}
