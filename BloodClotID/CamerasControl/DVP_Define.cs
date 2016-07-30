using System.Runtime.InteropServices;
using System;
public enum emDSRunMode
{
    RUNMODE_PLAY = 0,
    RUNMODE_PAUSE = 1,
    RUNMODE_STOP = 2
}

public enum emDSCameraStatus
{
    STATUS_OK = 1,       // 动作成功
    STATUS_IN_PROCESS = 2,       // 正在通信
    STATUS_INTERNAL_ERROR = -1,      // 内部错误
    STATUS_NOT_SUPPORTED = -2,      // 摄像头不支持该功能
    STATUS_NOT_INITIALIZED = -3,      // 初始化未完成
    STATUS_PARAMETER_INVALID = -4,      // 参数无效
    STATUS_TIME_OUT = -1000,   // 通信超时错误
    STATUS_IO_ERROR = -1001,   // 硬件IO错误
    STATUS_NO_DEVICE_FOUND = -1100,   // 没有发现相机
    STATUS_NO_LOGIC_DEVICE_FOUND = -1101,   // 未找到逻辑设备
    STATUS_DEVICE_IS_OPENED = -1102,   // 摄像头已经打开
    STATUS_MEMORY_NOT_ENOUGH = -1200,   // 没有足够系统内存
    STATUS_FILE_CREATE_FAILED = -1300,   // 创建文件失败
    STATUS_FILE_INVALID = -1301,   // 文件格式无效
    STATUS_WRITE_PROTECTED = -1400,   // 写保护，不可写
    STATUS_GRAB_FRAME_ERROR = -1600,   // 数据捕捉失败
    STATUS_LOST_DATA = -1601,   // 帧数据部分丢失，不完整
    STATUS_EOF_ERROR = -1602    // 未接收到帧结束符
}


public enum emDSMirrorDirection
{
    MIRROR_DIRECTION_HORIZONTAL = 0,
    MIRROR_DIRECTION_VERTICAL = 1
}

public enum emDSFrameSpeed
{
    FRAME_SPEED_NORMAL,
    FRAME_SPEED_HIGH,
    FRAME_SPEED_SUPER
}

public enum emDSFileType
{
    FILE_JPG = 1,
    FILE_BMP = 2,
    FILE_RAW = 4,
    FILE_PNG = 8
}

public enum emDSDataType
{
    // mono 数据
    DATA_TYPE_MONO1P = 0,
    DATA_TYPE_MONO2P = 1,
    DATA_TYPE_MONO4P = 2,
    DATA_TYPE_MONO8 = 3,
    DATA_TYPE_MONO8S = 4,
    DATA_TYPE_MONO10 = 5,
    DATA_TYPE_MONO10_PACKED = 6,
    DATA_TYPE_MONO12 = 7,
    DATA_TYPE_MONO12_PACKED = 8,
    DATA_TYPE_MONO14 = 9,
    DATA_TYPE_MONO16 = 10,

    // bayer 数据类型
    DATA_TYPE_BAYGR8 = 11,
    DATA_TYPE_BAYRG8 = 12,
    DATA_TYPE_BAYGB8 = 13,
    DATA_TYPE_BAYBG8 = 14,
    DATA_TYPE_BAYGR10 = 15,
    DATA_TYPE_BAYRG10 = 16,
    DATA_TYPE_BAYGB10 = 17,
    DATA_TYPE_BAYBG10 = 18,
    DATA_TYPE_BAYGR12 = 19,
    DATA_TYPE_BAYRG12 = 20,
    DATA_TYPE_BAYGB12 = 21,
    DATA_TYPE_BAYBG12 = 22,
    DATA_TYPE_BAYGR10_PACKED = 23,
    DATA_TYPE_BAYRG10_PACKED = 24,
    DATA_TYPE_BAYGB10_PACKED = 25,
    DATA_TYPE_BAYBG10_PACKED = 27,
    DATA_TYPE_BAYGR12_PACKED = 28,
    DATA_TYPE_BAYRG12_PACKED = 29,
    DATA_TYPE_BAYGB12_PACKED = 30,
    DATA_TYPE_BAYBG12_PACKED = 31,
    DATA_TYPE_BAYGR16 = 32,
    DATA_TYPE_BAYRG16 = 33,
    DATA_TYPE_BAYGB16 = 34,
    DATA_TYPE_BAYBG16 = 35,

    // RGB 数据类型
    DATA_TYPE_RGB8 = 36,
    DATA_TYPE_BGR8 = 37,
    DATA_TYPE_RGBA8 = 38,
    DATA_TYPE_BGRA8 = 39,
    DATA_TYPE_RGB10 = 40,
    DATA_TYPE_BGR10 = 41,
    DATA_TYPE_RGB12 = 42,
    DATA_TYPE_BGR12 = 43,
    DATA_TYPE_RGB16 = 44,
    DATA_TYPE_RGB10V1_PACKED = 45,
    DATA_TYPE_RGB10P32 = 46,
    DATA_TYPE_RGB12V1_PACKED = 47,
    DATA_TYPE_RGB565P = 48,
    DATA_TYPE_BGR565P = 49,

    // YUV YCbCr
    DATA_TYPE_YUV411_8_UYYVYY = 50,
    DATA_TYPE_YUV422_8_UYVY = 51,
    DATA_TYPE_YUV422_8 = 52,
    DATA_TYPE_YUV8_UYV = 53,
    DATA_TYPE_YCBCR8_CBYCR = 54,
    DATA_TYPE_YCBCR422_8 = 55,
    DATA_TYPE_YCBCR422_8_CBYCRY = 56,
    DATA_TYPE_YCBCR411_8_CBYYCRYY = 57,
    DATA_TYPE_YCBCR601_8_CBYCR = 58,
    DATA_TYPE_YCBCR601_422_8 = 59,
    DATA_TYPE_YCBCR601_422_8_CBYCRY = 60,
    DATA_TYPE_YCBCR601_411_8_CBYYCRYY = 61,
    DATA_TYPE_YCBCR709_8_CBYCR = 62,
    DATA_TYPE_YCBCR709_422_8 = 63,
    DATA_TYPE_YCBCR709_422_8_CBYCRY = 64,
    DATA_TYPE_YCBCR709_411_8_CBYYCRYY = 65,

    // planar
    DATA_TYPE_RGB8_PLANAR = 80,
    DATA_TYPE_RGB10_PLANAR = 81,
    DATA_TYPE_RGB12_PLANAR = 82,
    DATA_TYPE_RGB16_PLANAR = 83
}

public enum emDSLightFrequency
{
    LIGHT_FREQUENCY_50HZ = 0,
    LIGHT_FREQUENCY_60HZ = 1
}

public enum emDSParameterTeam
{
    PARAMETER_TEAM_A = 0,
    PARAMETER_TEAM_B = 1,
    PARAMETER_TEAM_C = 2,
    PARAMETER_TEAM_D = 3,
    PARAMETER_TEAM_LAST = 254,
    PARAMETER_TEAM_DEFAULT = 255
}

//摄像头硬件信息
 [StructLayout(LayoutKind.Sequential)]
public struct tDSCameraDevInfo
{
    public uint uVendorID;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public Byte[] acVendorName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public Byte[] acProductSeries;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] acProductName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] acFriendlyName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public Byte[] acDevFileName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public Byte[] acFileName; 
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public Byte[] acFirmwareVersion;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public Byte[] acSensorType;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public Byte[] acPortType;
 }

public struct tDSROISize
{
    public int iWidth;
    public int iHeight;
    public int iHOffset;
    public int iVOffset;
}

public struct tDSImageSize
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public Byte[] acDescription;
    public Boolean bIsBIN;
    public Boolean bIsSkip;
    public Boolean bIsZoom;
    public int iResolutionMode;	
    public int iWidth;
    public int iHeight;
    public int iHOffset;
    public int iVOffset;
}

public struct tDSImageSizeRange
{
    public int iHeightMax;
    public int iHeightMin;
    public int iHeightDefault;
    public int iWidthMax;
    public int iWidthMin;
    public int iWidthDefault;
    public bool bRoi;
}

public struct tDSFrameSpeed
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acDescription;
}

public struct tDSTimeStamp
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acDescription;
}

public struct tDSExpose
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acDescription;
    public ulong uiTargetMin;
    public ulong uiTargetMax;
    public ulong uiTargetDefault;
    public float fAnalogGainMin;
    public float fAnalogGainMax;
    public float fAnalogGainDefault;
    public float fAnalogGainStep;
    public ulong uiExposeTimeMin;
    public ulong uiExposeTimeMax;
    public ulong uiExposeTimeDefault;
    public ulong uiExposeTimeStep;
}


public struct tDSExposeParam
{
    public bool bAutoEnable;
    public ulong iAutoExposeTarget;
    public float fAnalogGain;
    public ulong iExposeTime;
}

public struct tDSFrameCount
{
    public int iCap;
    public int iTotal;
    public int iLost;
}

public struct tDSMediaType
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acDescription;
    public int iMediaType;
}

public struct tDSResolutionMode
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acDescription;
    public bool bBin;
    public bool bSkip;
    public bool bZoom;
    public int iWidthMax;
    public int iHeightMax;
    public int iHSampling;                     // 水平采样模式，该值的最终解释由具体设备决定
    public int iVSampling;                     // 垂直采样模式，该值的最终解释由具体设备决定
    public int iReadOut;   
}


public struct tGammaRange
{
    public int iMin;
    public int iMax;
    public int iDefault;
}

public struct tContrastRange
{
    public int iMin;
    public int iMax;
    public int iDefault;
}

public struct tRgbGainRange
{
    public float fRGainMin;
    public float fRGainMax;
    public float fRGainDefault;
    public float fGGainMin;
    public float fGGainMax;
    public float fGGainDefault;
    public float fBGainMin;
    public float fBGainMax;
    public float fBGainDefault;
}

public struct tSaturationRange
{
    public int iMin;
    public int iMax;
    public int iDefault;
}

public struct tSharpnessRange
{
    public int iMin;
    public int iMax;
    public int iDefault;
}

public struct tNoiseReductionRange
{
    public int iMin;
    public int iMax;
    public int iDefault;
}

public struct tDSCameraProcCap
{
    public bool bAF;
    public bool bAE;
    public bool bAWB;
    public bool bOnceAF;
    public bool bOnceWB;
    public bool bOnceBB;

    public bool bRgbSum;
    public bool bRgbGain;
    public bool bRgbCorrection;
    public bool bSaturation;

    public bool bRgbMapping;
    public bool bGamma;
    public bool bContrast;

    public bool bVFlip;
    public bool bHFlip;
    public bool bMono;
    public bool bInverse;
    public bool bSharpness;
    public bool bNoiseReduction;
}

public struct tDSModeDesc
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acModeName;
}

public struct tDSSwitchDesc
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acSwitchName;
    public bool bDefault;
}

public struct tDSValueDesc
{
    public int iIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] acValueName;
    public int iDefault;
}

public struct tDSSensorInfo
{
    public uint uSensorType;	// 描述CMOS,CCD
    public uint uSensorCount;	// SENSOR芯片数量
    public bool bMono;			// 黑白/彩色
    public uint uPixForm;		// 像素排列，如RGRG,GRGR
    public int iFieldCount;
}

public struct tDSCameraCapability
{
    public tDSSensorInfo sSensorInfo;
    //public tDSCameraProcCap sCameraProcCap;

    /* 一些可调节参数范围限定和默认值 */
    public tDSImageSizeRange sImageSizeRange;		// 图像尺寸范围和默认值
    public tRgbGainRange sRgbGainRange;			// RGB增益调节范围和默认值
    public tSaturationRange sSaturationRange;		// 饱和度调节范围和默认值
    public tGammaRange sGammaRange;			// GAMMA调节范围和默认值
    public tContrastRange sContrastRange;			// 对比度调节范围和默认值
    public tSharpnessRange sSharpnessRange;		// 锐度调节范围和默认值
    public tNoiseReductionRange sNoiseReductionRange;	// 噪声抑制调节范围和默认值

    public int pExposeDesc;
    public int iExposeDesc;
    public int iExposeDefault;

    public int pTriggerDesc;			// 触发
    public int iTriggerDesc;
    public int iTriggerDefault;

    public int pImageSizeDesc;
    public int iImageSizeDec;
    public int iImageSizeDefault;

    public int pMediaTypeDesc;
    public int iMediaTypdeDesc;
    public int iMediaTypeDefault;

    public int pResolutionModeDesc;
    public int iResolutionModeDesc;
    public int iResolutionModeDefault;

    public int pFrameSpeedDesc;
    public int iFrameSpeedDesc;
    public int iFrameSpeedDefault;

    public int pTimeStampDesc;
    public int iTimeStampDesc;
    public int iTimeStampDefault;

    public int pPackLenDesc;
    public int iPackLenDesc;
    public int iPackLenDefault;
   
    public Boolean bRomForSave;

    public int pModeDesc;
    public int iModeDesc;
    public int iModeDefault;

    public int pSwitchDesc;
    public int iSwitchDesc;

    public int pValueDesc;
    public int iValueDesc;
}

public struct tDSFrameInfo
{
    public uint uiMediaType;							// RAW/YUV/RGB，按照DS_DVP_DEFINE.h中定义
    public uint uBytes;									// 图像数据字节数
    public uint uiHeight;								// 高度
    public uint uiWidth;								// 宽度
    public uint uHOff;									// 水平起始位置,ROI时起作用
    public uint uVOff;									// 垂直其实位置,ROI时起作用
    public Boolean bHFlip;								// 指示是否水平翻转
    public Boolean bVFlip;								// 指示是否垂直翻转
    public Boolean bTriggered;							// 指示是否为触发帧
    public Boolean bTimeStampValid;					// 指示时间戳有效
    public Boolean bExpTimeValid;						// 指示曝光时间有效
    public uint uFrameID;
    public UInt64 uiTimeStamp;				    		// 时间戳，单位：由具体设备确定
    public UInt64 uiExpTime;							// 曝光时间，单位：由具体设备确定
}



