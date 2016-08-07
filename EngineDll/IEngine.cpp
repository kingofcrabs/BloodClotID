
#include "stdafx.h"
#include "IEngine.h"
#include <msclr\marshal_cppstd.h>
using namespace System;
using namespace System::IO;

namespace EngineDll
{
	IEngine::IEngine()
	{
		m_EngineImpl = new EngineImpl();
	}


	IEngine::~IEngine()
	{
		delete m_EngineImpl;
	}

	array<AnalysisResult^>^ IEngine::Analysis(System::String^ sFile, array<ROI^>^ rois)
	{
		
		std::string nativeFileName = msclr::interop::marshal_as< std::string >(sFile);
		std::vector<Circle> circles;
		for each(ROI^ roi in rois)
		{
			Circle circle(roi->x, roi->y, roi->radius);
			circles.push_back(circle);
		}
		std::vector<std::vector<cv::Point2f>> rotatedRects;
		std::vector<int> results = m_EngineImpl->Analysis(nativeFileName, circles, rotatedRects);
		
		array<AnalysisResult^>^ vals = gcnew array<AnalysisResult^>(results.size());
		for (int i = 0; i < results.size(); i++)
		{
			array<MSize^>^ points = gcnew array<MSize^>(4);
			auto rotatedRect = rotatedRects[i];
			for (int ii = 0; ii < 4; ii++)
			{
				auto pt = rotatedRect[ii];
				points[ii] = gcnew MSize(pt.x, pt.y);
			}
			
			RotatedRect^ rc = gcnew RotatedRect(points);
			vals[i] = gcnew AnalysisResult(rc, results[i], circles[i].radius);
		}
		return vals;
	}

}
