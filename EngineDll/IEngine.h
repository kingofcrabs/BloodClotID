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

	public ref class MPoint
	{
	public:
		int x;
		int y;
	
		MPoint(int xx, int yy)
		{
			x = xx;
			y = yy;
		}
	};

	public ref class RotatedRect
	{
	public:
		array<MPoint^>^ points;
		RotatedRect(array<MPoint^>^ pts)
		{
			points = gcnew array<MPoint^>(pts->Length);
			for (int i = 0; i < points->Length; i++)
			{
				points[i] = gcnew MPoint(pts[i]->x,pts[i]->y);
			}
		}
	};

	public ref class AnalysisResult
	{
	public: 
		RotatedRect^ rect;
		int val;
		AnalysisResult(RotatedRect^ rc, int v)
		{
			rect = rc;
			val = v;
		}
	};




	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		array<AnalysisResult^>^ Analysis(System::String^ sFile,array<ROI^>^ rois);
	private :
		EngineImpl* m_EngineImpl;
	};



	
	

}

