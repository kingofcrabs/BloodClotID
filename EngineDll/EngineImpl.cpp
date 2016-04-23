#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;
static string dbgFolder = "d:\\temp\\";
static int innderRadius = 30;

EngineImpl::EngineImpl()
{

}

void  EngineImpl::FindContours(const cv::Mat& thresholdImg,
	std::vector<std::vector<cv::Point>
	>& contours,
	int min, int max)
{
	std::vector< std::vector<cv::Point> > allContours;

	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	contours.clear();
	for (size_t i = 0; i<allContours.size(); i++)
	{
		int contourSize = allContours[i].size();
		
		if (contourSize > min && contourSize < max)
		{
			contours.push_back(allContours[i]);
		}
	}
}
double inline __declspec (naked) __fastcall sqrt14(double n)
{
	_asm fld qword ptr[esp + 4]
		_asm fsqrt
	_asm ret 8
}

double  EngineImpl::GetDistance(double x1, double y1, double x2, double y2)
{
	double xx = (x1 - x2)*(x1 - x2);
	double yy = (y1 - y2)*(y1 - y2);
	return sqrt14(xx + yy);
}

void EngineImpl::RemovePtsNotInROI(Mat& src, CvPoint ptMass)
{
	int height = src.rows;
	int width = src.cols;
	int channels = src.channels();
	int nc = width * channels;

	for (int y = 0; y < height; y++)
	{
		uchar *data = src.ptr(y);
		for (int x = 0; x < width; x++)
		{
			if (GetDistance(x, y, ptMass.x, ptMass.y) > innderRadius)
			{
				int xStart = x*channels;
				for (int i = 0; i< channels; i++)
					data[xStart + i] = 0;
			}
		}
	}
	return;
}

bool CompareX(Point x,Point y) { return x.x<y.x; }

int EngineImpl::GetWidth(vector<Point> pts)
{
	int right = max_element(pts.begin(), pts.end(), CompareX)->x;
	int left = min_element(pts.begin(), pts.end(), CompareX)->x;
	return right - left;
}

void EngineImpl::GetCircleROI(Mat& src)
{
	Mat gray;
	cvtColor(src, gray, CV_BGR2GRAY);
	threshold(gray, gray, 200, 255, 0);
#if _DEBUG
	imwrite(dbgFolder + "gray.jpg", gray);
#endif
	vector<vector<cv::Point>> contours;
	int minPts = 1000;
	FindContours(gray, contours, minPts);
	if (contours.size() == 0)
	{
		return;
	}
	int max = 0;
	int index = 0;
	for (int i = 0; i< contours.size(); i++)
	{
		int width = GetWidth(contours[i]);
		if (width > max)
		{
			max = width;
			index = i;
		}
	}
	//edgeContour = contours[index];
#if _DEBUG	
	Mat tmp = src.clone();

	//for (int i = 0; i< contours.size(); i++)
	{
		drawContours(tmp, contours, index, Scalar(0, 255, 0), 2);
	}
	imwrite(dbgFolder + "circleROI.jpg", tmp);
#endif
}

cv::Point EngineImpl::GetMassCenter(vector<cv::Point>& pts)
{
	int size = pts.size();
	float totalX = 0.0, totalY = 0.0;
	for (int i = 0; i<size; i++) {
		totalX += pts[i].x;
		totalY += pts[i].y;
	}
	return cv::Point(totalX / size, totalY / size); // condition: size != 0
}




void EngineImpl::Rotate90(cv::Mat &matImage, bool cw){
	//1=CW, 2=CCW, 3=180
	if (cw){
		transpose(matImage, matImage);
		flip(matImage, matImage, 1); //transpose+flip(1)=CW
	}
	else {
		transpose(matImage, matImage);
		flip(matImage, matImage, 0); //transpose+flip(0)=CCW     
	}
}


void EngineImpl::CountRed(int x, int y, Point ptCenter, Circle& c, bool bLight)
{
	double dis = cv::norm(cv::Mat(ptCenter), Mat(Point(x, y)));
	if (dis < c.radius)
	{
		if (bLight)
			lightRedCnt++;
		else
			darkRedCnt++;
	}
}

void EngineImpl::CountLightRed(int x, int y, Point ptCenter, Circle& c)
{
	CountRed(x, y, ptCenter, c, true);
}

void EngineImpl::CountDarkRed(int x, int y, Point ptCenter, Circle& c)
{
	CountRed(x, y, ptCenter, c, false);
}

void EngineImpl::GoThrough(Mat& sub, Circle &c)
{
	int height = sub.rows;
	int width = sub.cols;
	int channels = sub.channels();
	int nc = width * channels;
	Point ptCenter = Point(width / 2, height / 2);
	for (int y = 0; y < height; y++)
	{
		uchar *data = sub.ptr(y);
		int col = 0;
		for (int x = 0; x < nc; x += channels)
		{
			int r = data[x + 2];
			int g = data[x + 1];
			int b = data[x];
			if (r > g && r > b)
			{
				if (g > 50 || b > 50)
				{
					CountLightRed(col, y, ptCenter, c);
				}
				else if (g < 30 && b < 30)
				{
					CountDarkRed(col, y, ptCenter, c);
				}
				col++;
				continue;
			}
			col++;
		}
	}
}

vector<int> EngineImpl::Analysis(string sFile, vector<Circle> roi)
{
	img = imread(sFile);
	vector<int> results;
	results.push_back(1);
	return results;
}

