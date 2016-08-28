#include "stdafx.h"
#include "Analyzer.h"


using namespace std;
using namespace cv;
static string dbgFolder = "d:\\temp\\";
static int innerRadius = 30;

Analyzer::Analyzer()
{
}


Analyzer::~Analyzer()
{
}





//void  Analyzer::FindContours(const cv::Mat& thresholdImg,
//	std::vector<std::vector<cv::Point>
//	>& contours,
//	int min, int max)
//{
//	std::vector< std::vector<cv::Point> > allContours;
//
//	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
//	contours.clear();
//	for (size_t i = 0; i<allContours.size(); i++)
//	{
//		int contourSize = allContours[i].size();
//		
//		if (contourSize > min && contourSize < max)
//		{
//			contours.push_back(allContours[i]);
//		}
//	}
//}

std::string WStringToString(const std::wstring &wstr)
{
	std::string str(wstr.length(), ' ');
	std::copy(wstr.begin(), wstr.end(), str.begin());
	return str;
}



double  Analyzer::GetDistance(double x1, double y1, double x2, double y2)
{
	double xx = (x1 - x2)*(x1 - x2);
	double yy = (y1 - y2)*(y1 - y2);
	return sqrt(xx + yy);
}

void Analyzer::RemovePtsNotInCircle(Mat& src)
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


bool CompareX(Point x, Point y) { return x.x<y.x; }

int Analyzer::GetWidth(vector<Point> pts)
{
	int right = max_element(pts.begin(), pts.end(), CompareX)->x;
	int left = min_element(pts.begin(), pts.end(), CompareX)->x;
	return right - left;
}


cv::Point Analyzer::GetMassCenter(vector<cv::Point>& pts)
{
	int size = pts.size();
	float totalX = 0.0, totalY = 0.0;
	for (int i = 0; i<size; i++) {
		totalX += pts[i].x;
		totalY += pts[i].y;
	}
	return cv::Point(totalX / size, totalY / size); // condition: size != 0
}


void Analyzer::ThresholdByRed(Mat& hue, Mat& val)
{
	int height = hue.rows;
	int width = hue.cols;
	int channels = hue.channels();
	int nc = width * channels;
	for (int y = 0; y < height; y++)
	{
		uchar *dataHue = hue.ptr(y);
		uchar *dataVal = val.ptr(y);
		for (int x = 0; x < nc; x += channels)
		{
			int hue = dataHue[x];
			int val = dataVal[x];
			int changedVal = hue < 15 && val < 100 ? 0 : 255;

			for (int i = 0; i < channels; i++)
				dataHue[i] = changedVal;


		}
	}
}

std::vector<cv::Point> Analyzer::FindMaxContour(Mat& src)
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


//}
//
//void Analyzer::Rotate90(cv::Mat &matImage, bool cw){
//	//1=CW, 2=CCW, 3=180
//	if (cw){
//		transpose(matImage, matImage);
//		flip(matImage, matImage, 1); //transpose+flip(1)=CW
//	}
//	else {
//		transpose(matImage, matImage);
//		flip(matImage, matImage, 0); //transpose+flip(0)=CCW     
//	}
//}
//
//
//void Analyzer::CountRed(int x, int y, Point ptCenter, Circle& c, bool bLight)
//{
//	double dis = cv::norm(cv::Mat(ptCenter), Mat(Point(x, y)));
//	if (dis < c.radius)
//	{
//		if (bLight)
//			lightRedCnt++;
//		else
//			darkRedCnt++;
//	}
//}
//
//void Analyzer::CountLightRed(int x, int y, Point ptCenter, Circle& c)
//{
//	CountRed(x, y, ptCenter, c, true);
//}
//
//void Analyzer::CountDarkRed(int x, int y, Point ptCenter, Circle& c)
//{
//	CountRed(x, y, ptCenter, c, false);
//}
//
//void Analyzer::GoThrough(Mat& sub, Circle &c)
//{
//	int height = sub.rows;
//	int width = sub.cols;
//	int channels = sub.channels();
//	int nc = width * channels;
//	Point ptCenter = Point(width / 2, height / 2);
//	for (int y = 0; y < height; y++)
//	{
//		uchar *data = sub.ptr(y);
//		int col = 0;
//		for (int x = 0; x < nc; x += channels)
//		{
//			int r = data[x + 2];
//			int g = data[x + 1];
//			int b = data[x];
//			if (r > g && r > b)
//			{
//				if (g > 50 || b > 50)
//				{
//					CountLightRed(col, y, ptCenter, c);
//				}
//				else if (g < 30 && b < 30)
//				{
//					CountDarkRed(col, y, ptCenter, c);
//				}
//				col++;
//				continue;
//			}
//			col++;
//		}
//	}
//}

Rect Analyzer::GetRect(Circle circle, Size imgSize)
{

	double xStart, yStart, width, height;
	xStart = circle.x - circle.radius;
	yStart = circle.y - circle.radius;
	xStart = max(xStart, 0.0);
	yStart = max(yStart, 0.0);
	double dis2XBound = imgSize.width - xStart;
	double dis2YBound = imgSize.height - yStart;
	double unit = circle.radius * 2;
	double w = min(dis2XBound, unit);
	double h = min(dis2YBound, unit);
	Rect rc(xStart, yStart, w, h);
	return rc;
}


int Analyzer::AnalysisSub(Mat& sub, vector<cv::Point2f>& pts)
{

#ifdef _DEBUG
	static int id = 1;
	Mat org = sub.clone();
	wstringstream ss;
	ss << "D:\\temp\\hue" << id<< ".jpg";
	wstring ws = ss.str();
	string sHue = WStringToString(ws);
	ss.str(L"");
	ss << "D:\\temp\\org" << id << ".jpg";
	ws = ss.str();
	string sOrg = WStringToString(ws);
	ss.str(L"");
	ss << "D:\\temp\\sat" << id++ << ".jpg";
	ws = ss.str();
	string sSat = WStringToString(ws);
#endif
	//ThresholdByRed(sub);
	//Mat gray;
	//cvtColor(sub, gray, COLOR_BGR2GRAY);
	//adaptiveThreshold(gray, gray, 255, ADAPTIVE_THRESH_MEAN_C, THRESH_BINARY, 3, 5);
	//imwrite(sGray, gray);
	cvtColor(sub, sub, COLOR_BGR2HLS);
	vector<Mat> channels;
	split(sub, channels);
	Mat& hue = channels[0];
	Mat& light = channels[1];
	Mat& saturation = channels[2];
	Mat binary;

	//cvtColor(sub, gray, COLOR_BGR2GRAY);

	threshold(hue, hue, 15, 255, THRESH_BINARY_INV);
	threshold(light, light, 130, 255, THRESH_BINARY_INV);
	//adaptiveThreshold(gray, gray, 255, ADAPTIVE_THRESH_MEAN_C, THRESH_BINARY, 3, 5);
	threshold(saturation, saturation, 0, 255, CV_THRESH_OTSU);

	//equalizeHist(saturation, saturation);
	imwrite(sSat, saturation);
	threshold(saturation, saturation, 80, 255, THRESH_BINARY);

	bitwise_and(hue, light, hue);
	bitwise_and(hue, saturation, binary);
	imwrite(sHue, hue);

	vector<cv::Point> contour = FindMaxContour(binary);
	//vector<cv::Contours
	std::vector< std::vector<cv::Point> > contours;
	contours.push_back(contour);
	drawContours(org, contours, 0,Scalar(255,0,0),1);
	//imwrite(sOrg, org);
	if (contour.size() == 0)
	{

		pts.push_back(Point(-3, -3));
		pts.push_back(Point(3, -3));
		pts.push_back(Point(3, 3));
		pts.push_back(Point(-3, 3));
		return 0;
	}

	auto rotatedRect = minAreaRect(contour);
	Point2f vertices[4];
	rotatedRect.points(vertices);
	double maxDistance = 0;

	int height = sub.rows;
	int width = sub.cols;
	int xCenter = width / 2;
	int yCenter = height / 2;

	for (int i = 0; i < 4; i++)
	{
		auto pt1 = vertices[i];
		auto pt2 = vertices[(i + 1) % 4];
		auto distance = GetDistance(pt1.x, pt1.y, pt2.x, pt2.y);
		if (distance > maxDistance)
			maxDistance = distance;
		pts.push_back(Point(vertices[i].x - xCenter, vertices[i].y - yCenter));
	}
	return maxDistance;
}



vector<int> Analyzer::Analysis(string sFile, cv::Rect2f boundRect, vector<Circle> rois, vector<vector<cv::Point2f>>& rotatedRects)
{
	Mat img = imread(sFile);
	vector<int> results;
	for (int i = 0; i < rois.size(); i++)
	{
		Rect rc = GetRect(rois[i], img.size());
		vector<cv::Point2f> rotatedRect;
		int val = AnalysisSub(img(rc), rotatedRect);
		results.push_back(val);
		rotatedRects.push_back(rotatedRect);

	}

	return results;
}

