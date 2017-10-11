#pragma once
#include "EngineImpl.h"


namespace EngineDll
{

	public ref class MROI
	{
	public:
		int x;
		int y;
		int radius;
		
		MROI(int xx, int yy, int rr)
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
		array<MPoint^>^ points;
		RotatedRect(array<MPoint^>^ pts)
		{
			points = gcnew array<MPoint^>(pts->Length);
			for (int i = 0; i < points->Length; i++)
			{
				points[i] = gcnew MPoint(pts[i]->x, pts[i]->y);
			}
		}
	};

	public ref class MAnalysisResult
	{
	public: 
		RotatedRect^ rect;
		int val;
		double radius;
		MAnalysisResult(RotatedRect^ rc, int v, double r)
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
		array<MAnalysisResult^>^ Analysis(System::String^ sFile, array<MROI^>^ rois);
		array<MAnalysisResult^>^ Analysis(array<uchar>^ red, array<uchar>^ green, array<uchar>^ blue,int width, int height, array<MROI^>^ rois);
	private :
		EngineImpl* m_EngineImpl;
	};



	
	

}

