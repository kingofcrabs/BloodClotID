// LengthAnalyzer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <time.h> 
using namespace std;


void ReadCalibInfo(string sFile, cv::Rect2f& rect, vector<Circle>& circles)
{
	ifstream ifs(sFile);
	cv::Point2f ptTopLeft;
	cv::Point2f ptBottomRight;
	ifs >> ptTopLeft.x >> ptTopLeft.y >> ptBottomRight.x >> ptBottomRight.y;
	rect = cv::Rect2f(ptTopLeft, ptBottomRight);
	
	while (ifs)
	{
		
		int x, y, radius;
		ifs >> x >> y >> radius;
		if (ifs.fail())
			break;
		Circle circle(x, y, radius);
		circles.push_back(circle);
	}
}
string GetCurrentFolder(string sExeFile)
{
	int backSlashIndex = sExeFile.find_last_of('\\');
	string currentFolder = sExeFile.substr(0, backSlashIndex);
	return currentFolder + "\\";
}

string GetParentFolder(string sExeFile)
{
	int backSlashIndex = sExeFile.find_last_of('\\');
	string currentFolder = sExeFile.substr(0, backSlashIndex);
	backSlashIndex = currentFolder.find_last_of('\\');
	string parentFolder = currentFolder.substr(0, backSlashIndex);
	return parentFolder + "\\";
}

string GetCalibFile(string sParentFolder,int cameraID)
{
	stringstream ss;
	ss << sParentFolder << "Calib\\ROIs_" << cameraID << ".txt";
	return ss.str();
}

void WriteResult(string resultFile,vector<int>& lengths, vector<vector<cv::Point2f>>& rotatedRects)
{
	ofstream ofs(resultFile);
	for (int i = 0; i < lengths.size(); i++)
	{
		ofs << lengths[i] << " ";
		for (int j = 0; j < rotatedRects[i].size(); j++)
		{
			ofs << rotatedRects[i][j].x << " " << rotatedRects[i][j].y<< " ";
		}
		ofs << endl;
	}

}

int main(int argc, char* argv[])
{
	if (argc < 3)
		return -1;


	int cameraID = argv[1][0] - '0';
	string sExePath(argv[0]);
	string parentFolder = GetParentFolder(sExePath);
	string sCalibFile = GetCalibFile(parentFolder, cameraID);
	cv::Rect2f rect;
	vector<Circle> circles;
	ReadCalibInfo(sCalibFile, rect, circles);
	string sImageFile(argv[2]);
	string currentFolder = GetCurrentFolder(sImageFile);
	Analyzer analyzer;
	vector<vector<cv::Point2f>> rotatedRects;
	vector<int> lengths = analyzer.Analysis(sImageFile, rect, circles, rotatedRects);
	if (lengths.size() != rotatedRects.size())
		return -2;
	stringstream ss;
	ss << currentFolder << cameraID << ".txt";
	string resultFile = ss.str();
	WriteResult(resultFile,  lengths, rotatedRects);
	return 0;
}

