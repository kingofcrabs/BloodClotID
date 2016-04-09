#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;
static string dbgFolder = "d:\\temp\\";
static int innderRadius = 630;
static int outterRadius = 730;
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


vector<vector<cv::Point>> EngineImpl::MarkAllContours(Mat& src,ConstrainSettings^ constrainSettings, string filePath2Save)
{
	Mat tmp = src.clone();
	vector<vector<cv::Point>> contours;
	int minPts = constrainSettings->minSize;
	int maxPts = constrainSettings->maxSize;
	Mat gray;
	RemovePtsNotInROI(tmp, ptMass);
	cvtColor(tmp, gray, CV_BGR2GRAY);
	//equalizeHist(gray, gray);
	int blockSize = 25;
	int constValue = 10;
	Mat adaptive;
	adaptiveThreshold(gray, adaptive, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, blockSize, constValue);
	imwrite(dbgFolder + "adaptive.jpg", adaptive);
#if _DEBUG	//threshold(gray, gray, 200, 255, 0);
	erode(adaptive, adaptive, Mat(), Point(-1, -1), 3);
	imwrite(dbgFolder + "erode.jpg", adaptive);
#endif
	//threshold(gray, gray, 200, 255, 0);
	FindContours(adaptive, contours, minPts, maxPts);
	for (int i = 0; i< contours.size(); i++)
	{
		
		drawContours(tmp, contours, i, Scalar(0, 0, 255));
	}
	imwrite(filePath2Save, tmp);
	return contours;
}


string EngineImpl::MarkClones(ConstrainSettings^ constrains, std::vector<cv::Point>& centers, 
	cv::Point ptMass)
{
	this->ptMass = ptMass;
	string resultFile = workingFolder + "\\output\\clones.jpg";
	auto contours = MarkAllContours(img, constrains, resultFile);
	sort(contours.begin(), contours.end(), [](const vector<cv::Point> & a, const vector<cv::Point> & b) -> bool
	{
		return a.size() > b.size();
	});

	for (auto contour : contours)
	{
		cv::Point pt = GetMassCenter(contour);
		centers.push_back(pt);
	}
	return resultFile;
}

enum ChannelType
{
	blue = 0,
	red = 1,
	green = 2
};

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


//first convert the image into HSV format,call inRange to filter the color
vector<vector<cv::Point>> EngineImpl::MarkAllContoursGray(Mat& src, Mat& org)
{
	Mat gray = src.clone();
	Mat tmp = org.clone();

	vector<vector<cv::Point>> contours;
	int minPts = 10;
	int maxPts = 500;

	//adaptiveThreshold(gray, gray, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY_INV, 11, 5);
	threshold(gray, gray, 200, 255, 0);
#if _DEBUG
	imwrite(dbgFolder + "adaptiveYellow.jpg", gray);
#endif
	FindContours(gray, contours, minPts, maxPts);
	for (int i = 0; i< contours.size(); i++)
	{
		drawContours(tmp, contours, i, Scalar(0, 0, 255),1);
		
	}
#if _DEBUG
	imwrite(dbgFolder + "featuresYellow.jpg", tmp);
#endif
	return contours;
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


void EngineImpl::Load(string sFile)
{
	int index = sFile.rfind("\\Data");
	workingFolder = sFile.substr(0, index);
	img = imread(sFile);
	//pyrDown(img, img);
	const int marginX = 200;
	Rect roi(marginX, 0, img.cols - marginX*2, img.rows);
	img = img(roi);
	Rotate90(img, false);
	imwrite(sFile, img);
	//GetCircleROI(img);
}

void EngineImpl::FindRefPositions(int& top, int& left, int& bottom, int& right)
{
	Mat gray;
	Mat src = img.clone();
	//cvtColor(img, gray, CV_BGR2GRAY);
#if _DEBUG
	imwrite(dbgFolder + "beforeRemovePetriDish.jpg", src);
#endif
	circle(src, Point(src.cols / 2, src.rows / 2), outterRadius, Scalar(0,0, 0),CV_FILLED);
	cvtColor(src, gray, CV_BGR2GRAY);
	threshold(gray, gray, 200, 255, 0);
#if _DEBUG
	imwrite(dbgFolder + "removePetriDish.jpg", gray);
#endif
	vector<vector<cv::Point>> contours;
	int minPts = 10;
	int maxPts = 200;
	FindContours(gray, contours, minPts, maxPts);
	vector<Point> pts;
	for (auto contour : contours)
	{
		pts.push_back(GetMassCenter(contour));
	}
	int tmpTop = 10000;
	int tmpLeft = 10000;
	int tmpRight = 0;
	int tmpBottom = 0;
	for (auto pt : pts)
	{
		if (pt.y < tmpTop)
			tmpTop = pt.y;
		if (pt.y > tmpBottom)
			tmpBottom = pt.y;
		if (pt.x < tmpLeft)
			tmpLeft = pt.x;
		if (pt.x > tmpRight)
			tmpRight = pt.x;
	}
	top = tmpTop;
	bottom = tmpBottom;
	left = tmpLeft;
	right = tmpRight;
}

