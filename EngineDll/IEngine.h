#pragma once
#include "EngineImpl.h"


namespace EngineDll
{
	public ref class MPoint
	{
	public:
		bool isCurrent;
		int x;
		int y;
		int ID = -1;
		int size;
		MPoint(int xx, int yy)
		{
			x = xx;
			y = yy;
			this->size = size;
			isCurrent = false;
		}
		MPoint(int xx, int yy, int id)
		{
			x = xx;
			y = yy;
			ID = id;
			isCurrent = false;
		}
	};

	public ref class RefPositions
	{
	public:
		int top;
		int left;
		int right;
		int bottom;
	};
	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		void Load(System::String^ sFile);
		System::String^ MarkClones(ConstrainSettings^ ConstrainSettings, RefPositions^ calibPositions, int% actualCnt, array<MPoint^>^ %features);
	private :
		EngineImpl* m_EngineImpl;
	};



	
	

}

