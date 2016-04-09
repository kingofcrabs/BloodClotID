#pragma once
#include "stdafx.h"

public ref class ConstrainSettings
{
public:
	int minSize;
	int maxSize;
	ConstrainSettings(int min, int max)
	{
		minSize = min;
		maxSize = max;
	}
};


class EngineImpl
{
public:
	void Load(std::string sFile);
	EngineImpl();
	std::string MarkClones(ConstrainSettings^ constrains, std::vector<cv::Point>& centers,cv::Point ptMass);
	void FindCalibPositions(int& top, int& left, int& bottom, int& right);
	void FindRefPositions(int& top, int& left, int& bottom, int& right);
private:
	void Rotate90(cv::Mat &matImage, bool cw);
	cv::Point GetMassCenter(std::vector<cv::Point>& pts);
	void GetCircleROI(cv::Mat& src);
	int GetWidth(std::vector<cv::Point> pts);
	void RemovePtsNotInROI(cv::Mat& src, CvPoint ptMass);
	
	void  FindContours(const cv::Mat& thresholdImg, std::vector<std::vector<cv::Point>>& contours, int min, int max = 999999);
	double  GetDistance(double x1, double y1, double x2, double y2);
	std::vector<std::vector<cv::Point>> MarkAllContoursGray(cv::Mat& src, cv::Mat& org);
	std::vector<std::vector<cv::Point>> MarkAllContours(cv::Mat& src,ConstrainSettings^ constrainSettings, std::string filePath2Save);
	std::string workingFolder;
	cv::Mat img;
	//std::vector<cv::Point> edgeContour;

	cv::Point ptMass;
};

