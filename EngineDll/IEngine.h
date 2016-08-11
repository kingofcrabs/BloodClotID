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

	public ref class MRect
	{
	public:
		MPoint^ ptStart;
		MPoint^ ptEnd;
		MRect(MPoint^ ptS, MPoint^ ptE)
		{
			ptStart = gcnew MPoint(ptS->x,ptS->y);
			ptEnd = gcnew MPoint(ptE->x,ptE->y);
		}
	};

	public ref class MSize
	{
	public:
		int x;
		int y;
	
		MSize(int xx, int yy)
		{
			x = xx;
			y = yy;
		}
	};

	public ref class RotatedRect
	{
	public:
		array<MSize^>^ points;
		RotatedRect(array<MSize^>^ pts)
		{
			points = gcnew array<MSize^>(pts->Length);
			for (int i = 0; i < points->Length; i++)
			{
				points[i] = gcnew MSize(pts[i]->x,pts[i]->y);
			}
		}
	};

	public ref class AnalysisResult
	{
	public: 
		RotatedRect^ rect;
		int val;
		double radius;
		AnalysisResult(RotatedRect^ rc, int v,double r)
		{
			rect = rc;
			val = v;
			radius = r;
		}
	};




	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		cv::Rect2f Convert2Rect2f(MRect^ rc);
		array<AnalysisResult^>^ Analysis(System::String^ sFile, array<ROI^>^ rois,MRect^ rc);
	private :
		EngineImpl* m_EngineImpl;
	};



	
	

}

