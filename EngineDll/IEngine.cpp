
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

	MAnalysisResult^ IEngine::Analysis(array<uchar>^ red, array<uchar>^ green, array<uchar>^ blue,  int width, int height)
	{
		
		
		std::vector<cv::Point2f> pts;
		pin_ptr<uchar> pinRed = &red[0];
		pin_ptr<uchar> pinGreen = &green[0];
		pin_ptr<uchar> pinBlue = &blue[0];

		uchar * pRedData = pinRed;
		uchar * pGreenData = pinGreen;
		uchar * pBlueData = pinBlue;

		int len = m_EngineImpl->Analysis(pRedData, pGreenData, pBlueData, width, height, pts);
		array<MPoint^>^ points = gcnew array<MPoint^>(4);
		
		for (int ii = 0; ii < 4; ii++)
		{
			auto pt = pts[ii];
			points[ii] = gcnew MPoint(pt.x, pt.y);
		}

		RotatedRect^ rc = gcnew RotatedRect(points);
		auto result = gcnew MAnalysisResult(rc, len, 225);
		return result;
	}

	cv::Rect2f IEngine::Convert2Rect2f(MRect^ rc)
	{
		cv::Point2f ptStart(rc->ptStart->x, rc->ptStart->y);
		cv::Point2f ptEnd(rc->ptEnd->x, rc->ptEnd->y);
		return cv::Rect2f(ptStart, ptEnd);
	}

	array<MAnalysisResult^>^ IEngine::Analysis(System::String^ sFile, array<MROI^>^ rois)
	{
		std::string nativeFileName = msclr::interop::marshal_as< std::string >(sFile);
		std::vector<MCircle> circles;
		for each(MROI^ roi in rois)
		{
			MCircle circle(roi->x, roi->y, roi->radius);
			circles.push_back(circle);
		}
		std::vector<std::vector<cv::Point2f>> rotatedRects;
		//cv::Rect2f rect = Convert2Rect2f(rc);
		std::vector<int> results = m_EngineImpl->Analysis(nativeFileName,circles, rotatedRects);
		
		array<MAnalysisResult^>^ vals = gcnew array<MAnalysisResult^>(results.size());
		for (int i = 0; i < results.size(); i++)
		{
			array<MPoint^>^ points = gcnew array<MPoint^>(4);
			auto rotatedRect = rotatedRects[i];
			for (int ii = 0; ii < 4; ii++)
			{
				auto pt = rotatedRect[ii];
				points[ii] = gcnew MPoint(pt.x, pt.y);
			}
			
			RotatedRect^ rc = gcnew RotatedRect(points);
			vals[i] = gcnew MAnalysisResult(rc, results[i], circles[i].radius);
		}
		return vals;
	}

}
