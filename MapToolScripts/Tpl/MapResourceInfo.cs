using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace Config
{
	[System.Serializable]
	public class MapResourceInfo
	{
		public int maskId;
		public int cellSize;
		public int resId;
		public int height;
		public int width;
		public int imgRowNum;
		public int imgColumnNum;
		public int smallWidth;
		public int smallHeight;
	}
}