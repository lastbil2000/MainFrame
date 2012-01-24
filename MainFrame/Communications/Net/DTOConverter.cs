using System.IO;
using System;
using ProtoBuf;

namespace MainFrame.Communication.Net
{

	public enum DTOTypes 
	{
		TestDTO =  0x00
	}

	public class DTOConverter
	{

		public DTOConverter ()
		{
		}
		
		public IDTO GetDTO(int dtoType, byte [] body) 
		{
			switch (dtoType)
			{
			case (int) DTOTypes.TestDTO:
				return Serializer.Deserialize<TestDTO>(new MemoryStream(body));
			default:
				throw new ApplicationException("No DTO type with id: " + dtoType.ToString() + " found.");
			}
		}
		
		public MemoryStream GetDTOStream (IDTO dto)
		{
			int dtoType = GetHeader(dto);
			
			if (dto == null)
				throw new NullReferenceException("DTO is null. Cannot. must not. damn you!");
			
			MemoryStream DTOWriteStream = new MemoryStream();
			
			switch (dtoType)
			{
			case (int) DTOTypes.TestDTO:
				Serializer.Serialize(DTOWriteStream, (TestDTO) dto);
			break;
			default:
				throw new ApplicationException("No DTO type with id: " + dtoType.ToString() + " found.");
			}
			
			return DTOWriteStream;
		}
		
		public UInt16 GetHeader (IDTO dto)
		{
			if (dto == null)
				throw new NullReferenceException("DTO is null. Cannot. must not. damn you!");
				
			if (dto is TestDTO)
				return (UInt16) DTOTypes.TestDTO;

			throw new ApplicationException("object: " + dto.GetType() + " found.");
					
		}
		
		
	}
}
