#pragma once



class Circle
{
public:
	int x;
	int y;
	int radius;
	Circle(int xx, int yy, int rr)
	{
		x = xx;
		y = yy;
		radius = rr;
	}
};


class Analyzer
{
public:
	Analyzer();
	~Analyzer();
	std::vector<int> Analysis(std::string sFile, cv::Rect2f rc, std::vector<Circle> rois, std::vector<std::vector<cv::Point2f>>&);
private:
	cv::Rect GetRect(Circle circle, cv::Size imgSize);
	int AnalysisSub(cv::Mat& sub, std::vector<cv::Point2f>& rotatedRect);


	void RemovePtsNotInCircle(cv::Mat& src);
	void ThresholdByRed(cv::Mat& hue, cv::Mat& val);
	std::vector<cv::Point> FindMaxContour(cv::Mat& src);


	void Rotate90(cv::Mat &matImage, bool cw);
	cv::Point GetMassCenter(std::vector<cv::Point>& pts);
	void GetCircleROI(cv::Mat& src);
	int GetWidth(std::vector<cv::Point> pts);
	double  GetDistance(double x1, double y1, double x2, double y2);
	std::string workingFolder;
	cv::Point ptMass;
};

