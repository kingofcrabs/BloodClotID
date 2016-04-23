
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

	array<int>^ IEngine::Analysis(System::String^ sFile, array<ROI^>^ rois)
	{
		
		std::string nativeFileName = msclr::interop::marshal_as< std::string >(sFile);
		std::vector<Circle> circles;
		for each(ROI^ roi in rois)
		{
			Circle circle(roi->x, roi->y, roi->radius);
			circles.push_back(circle);
		}
		std::vector<int> results = m_EngineImpl->Analysis(nativeFileName, circles);
		
		array<int>^ vals = gcnew array<int>(results.size());
		for (int i = 0; i < results.size(); i++)
		{
			vals[i] = results[i];
		}
		return vals;
	}

}
