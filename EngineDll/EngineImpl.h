#pragma once
#include "stdafx.h"


class MCircle
{
public:
	int x;
	int y;
	int radius;
	MCircle(int xx, int yy, int rr)
	{
		x = xx;
		y = yy;
		radius = rr;
	}
};


class EngineImpl
{
public:
	EngineImpl();
	std::vector<int> Analysis(std::string sFile, std::vector<MCircle> rois, std::vector<std::vector<cv::Point2f>>&);
	std::vector<int> Analysis(uchar* pRedData, uchar* pGreenData, uchar* pBlueData, int width, int height, std::vector<MCircle> rois, std::vector<std::vector<cv::Point2f>>& rotatedRects);
private:
	cv::Rect GetRect(MCircle circle, cv::Size imgSize);
	int AnalysisSub(cv::Mat& sub,int id, std::vector<cv::Point2f>& rotatedRect);
	
	
	void RemovePtsNotInCircle(cv::Mat& src);
	void ThresholdByRed(cv::Mat& hue, cv::Mat& val);
	std::vector<cv::Point> FindMaxContour(cv::Mat& src);


	void Rotate90(cv::Mat &matImage, bool cw);
	cv::Point GetMassCenter(std::vector<cv::Point>& pts);
	void GetCircleROI(cv::Mat& src);
	int GetWidth(std::vector<cv::Point> pts);
	//void  FindContours(const cv::Mat& thresholdImg, std::vector<std::vector<cv::Point>>& contours, int min, int max = 999999);
	double  GetDistance(double x1, double y1, double x2, double y2);



	/*void GoThrough(cv::Mat& sub, Circle &c);
	void CountRed(int x, int y, cv::Point ptCenter, Circle& c, bool bLight);
	void CountLightRed(int x, int y, cv::Point ptCenter, Circle& c);
	void CountDarkRed(int x, int y, cv::Point ptCenter, Circle& c);*/
	std::string workingFolder;
	//std::vector<cv::Point> edgeContour;
	int lightRedCnt;
	int darkRedCnt;
	cv::Point ptMass;
};

