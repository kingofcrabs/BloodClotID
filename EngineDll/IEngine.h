#pragma once
#include "EngineImpl.h"


namespace EngineDll
{

	public ref class ROI
	{
	public:
		int x;
		int y;
		int radius;
		
		ROI(int xx, int yy, int rr)
		{
			x = xx;
			y = yy;
			radius = rr;
		}
	};




	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		array<int>^ Analysis(System::String^ sFile,array<ROI^>^ rois);
	private :
		EngineImpl* m_EngineImpl;
	};



	
	

}

