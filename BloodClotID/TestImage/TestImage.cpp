// TestImage.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include <tchar.h>
#include <opencv2\opencv.hpp>
#include <iostream>
#include <algorithm>
#include <numeric>
using namespace std;
using namespace cv;


double inline __declspec (naked) __fastcall sqrt14(double n)
{
	_asm fld qword ptr[esp + 4]
		_asm fsqrt
	_asm ret 8
}

double  GetDistance(double x1, double y1, double x2, double y2)
{
	double xx = (x1 - x2)*(x1 - x2);
	double yy = (y1 - y2)*(y1 - y2);
	return sqrt14(xx + yy);
}


void RemovePtsNotInCircle(Mat& src)
{
	int height = src.rows;
	int width = src.cols;
	int channels = src.channels();
	int nc = width * channels;
	int xx = (width + 1) / 2;
	int yy = (height + 1) / 2;
	int innerRadius = min(xx, yy);
	for (int y = 0; y < height; y++)
	{
		uchar *data = src.ptr(y);
		for (int x = 0; x < width; x++)
		{
			if (GetDistance(x, y, xx, yy) > innerRadius)
			{
				int xStart = x*channels;
				for (int i = 0; i< channels; i++)
					data[xStart + i] = 255;
			}
		}
	}
	return;
}


void FilterRed(Mat& sub)
{
	int height = sub.rows;
	int width = sub.cols;
	int channels = sub.channels();
	int nc = width * channels;
	for (int y = 0; y < height; y++)
	{
		uchar *data = sub.ptr(y);
		for (int x = 0; x < nc; x += channels)
		{
			int r = data[x + 2];
			int g = data[x + 1];
			int b = data[x];
			int val = r > 100 && r + r - g - b > 100 ? 0 : 255;
			
				for (int i = 0; i< channels; i++)
					data[x+i] = val;
			
			
		}
	}
}


std::vector<cv::Point> FindMaxContour(Mat& src)
{

	std::vector< std::vector<cv::Point> > contours;

	cv::findContours(src, contours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	int maxSize = 0;
	vector<cv::Point> maxContour;
	int height = src.rows;
	int width = src.cols;
	int toobig = (height + width) * 1.5;
	for (size_t i = 0; i<contours.size(); i++)
	{
		int contourSize = contours[i].size();
		if (contourSize > toobig)
			continue;
		if (contourSize > maxSize)
		{
			maxSize = contourSize;
			maxContour = contours[i];
		}
	}
	return maxContour;
		 
}
#include <fstream>

int _tmain(int argc, _TCHAR* argv[])
{
	ofstream ofs("d:\\test.txt");
	int val = 333;
	ofs << L"1234" << val << endl;
	//Mat src = imread("D:\\temp\\test.png");
	//RemovePtsNotInCircle(src);
	//imwrite("D:\\temp\src.png",src);

	//vector<Mat> rgb(3);
	//FilterRed(src);
	//Mat gray,binary;
	//cvtColor(src, gray, COLOR_BGR2GRAY);
	//threshold(gray, binary, 100, 255, THRESH_BINARY);
	//imwrite("D:\\temp\\threshold.png", binary);
	//vector<cv::Point> contour = FindMaxContour(binary);
	//auto rotatedRect = minAreaRect(contour);
	//
	//Point2f vertices[4];
	//rotatedRect.points(vertices);
	//double maxDistance = 0;
	//for (int i = 0; i < 4; i++)
	//{
	//	auto pt1 = vertices[i];
	//	auto pt2 = vertices[(i + 1) % 4];
	//	auto distance =GetDistance(pt1.x, pt1.y, pt2.x, pt2.y);
	//	if (distance > maxDistance)
	//		maxDistance = distance;
	//	line(src,pt1 ,pt2 , Scalar(0, 0, 255));

	//}
	//	
	//imwrite("D:\\temp\\filter.png", src);
	return 0;
}

