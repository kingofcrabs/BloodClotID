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
					data[xStart + i] = 0;
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
			if ( r > 100 && r + r - g - b > 100)
			{
				for (int i = 0; i< channels; i++)
					data[x+i] = 0;
			}
			
		}
	}
}


int _tmain(int argc, _TCHAR* argv[])
{
	Mat src = imread("H:\\Projects\\BloodClotID.git\\trunk\\sample.png");
	RemovePtsNotInCircle(src);
	imwrite("h:\\opencvResult\\src.png",src);
	vector<Mat> rgb(3);
	FilterRed(src);
	imwrite("h:\\opencvResult\\filter.png",src);
	/*imwrite("h:\\opencvResult\\r.png",rgb[0]);
	imwrite("h:\\opencvResult\\g.png",rgb[1]);
	imwrite("h:\\opencvResult\\b.png",rgb[2]);*/
	return 0;
}

