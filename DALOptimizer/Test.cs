
using System;
using System.Collections.Generic;

namespace StringIndexOf
{
	/// <summary>
	/// This class contains various IndexOf calls.
	/// Only the ones in the first should be detected as missing a StringComparison.
	/// </summary>
	public class Test
	{
       public void StringIndexOfStringCalls(List<string> list)
		{
			list[0].IndexOf(".com");
			list[0].IndexOf(".com", 0);
			list[0].IndexOf(".com", 0, 5);
			list[0].IndexOf(list[1], 0, 10);
            list[0].IndexOf(list[1], 3, 1);
            list[2].IndexOf(list[2], 3, 1);
            list[2].Insert(2,"5");
            list[2].Insert(2, "5");
            list[100].IndexOf(list[000000002], 3, 1);
		}
		
		public void StringIndexOfStringCallsWithComparison(List<string> list)
		{
			list[0].IndexOf(".com", StringComparison.OrdinalIgnoreCase);
			list[0].IndexOf(".com", 0, StringComparison.OrdinalIgnoreCase);
			list[0].IndexOf(".com", 0, 5, StringComparison.OrdinalIgnoreCase);
			list[0].IndexOf(list[1], 0, 10, StringComparison.OrdinalIgnoreCase);
            
		}
		
	}
}
