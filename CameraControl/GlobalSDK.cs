using System;
using System.Runtime.InteropServices;


public class Camera
{
    public delegate int DelegateProc(int m_iCam, ref Byte pbyBuffer, ref tDSFrameInfo sFrInfo);
    [DllImport("kernel32.dll")]
    public static extern void CopyMemory(IntPtr Destination, int Source, int Length);
    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSaveBMPFile(int iCameraID, String lpszFileName, int pbyRGB24, ref tDSFrameInfo psFrInfo);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetDevList(IntPtr pCameraInfo, ref int piNums);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CameraISP(int m_iCameraID, ref Byte pbyRAW, ref tDSFrameInfo psFrInfo);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraDisplayRGB24(int m_iCameraID, int pbyRGB24, ref tDSFrameInfo psFrInfo);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraInit(DelegateProc pCallbackFunction, String lpszFriendlyName, IntPtr hWndDisplay, ref int piCameraID);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetDisplaySize(int m_iCameraID, int iWidth, int iHeight);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraPlay(int m_iCameraID);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraStop(int m_iCameraID);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraUnInit(int m_iCameraID);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetCapability(int m_iCameraID, ref tDSCameraCapability sDSCameraCap);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetImageSizeSel(int m_iCameraID, ref int piResel, Boolean bCaputer);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetImageSizeSel(int m_iCameraID, int iResel, Boolean bCaputer);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetAeState(int m_iCameraID, ref Boolean pbAeState);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetAeState(int m_iCameraID, Boolean byAeState);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetExposureTime(int m_iCameraID, ref UInt64 puExposureTime, ref UInt64 puExpTimeMax, ref UInt64 puExpTimeMin);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetExposureTime(int m_iCameraID, UInt64 uExposureTime);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetAntiFlick(int m_iCameraID, ref Boolean pbEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetAntiFlick(int m_iCameraID, Boolean bEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetColorEnhancement(int m_iCameraID, ref Boolean pbEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetColorEnhancement(int m_iCameraID, Boolean bEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetGain(int m_iCameraID, ref float pfRGain, ref float pfGGain, ref float pfBGain);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetGain(int m_iCameraID, float fRGain, float fGGain, float fBGain);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetGamma(int m_iCameraID, ref Byte pbyGamma);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetGamma(int m_iCameraID, Byte byGamma);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetContrast(int m_iCameraID, ref Byte pbyContrast);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetContrast(int m_iCameraID, Byte byContrast);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetSaturation(int m_iCameraID, ref Byte pbySaturation);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetSaturation(int m_iCameraID, Byte bySaturation);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetEdgeEnhance(int m_iCameraID, ref Boolean pbEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetEdgeEnhance(int m_iCameraID, Boolean bEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetEdgeGain(int m_iCameraID, ref Byte pbyEdgeGain);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetEdgeGain(int m_iCameraID, Byte byEdgeGain);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetNoiseReduction(int m_iCameraID, ref Boolean pbReduction, ref int piReduction);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetNoiseReduction(int m_iCameraID, Boolean bReduction, int iReduction);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetMirror(int m_iCameraID, emDSMirrorDirection emDir, ref Boolean pbEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetMirror(int m_iCameraID, emDSMirrorDirection emDir, Boolean bEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetMonochrome(int m_iCameraID, ref Boolean pbEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetMonochrome(int m_iCameraID, Boolean bEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetInverse(int m_iCameraID, ref Boolean pbEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetInverse(int m_iCameraID, Boolean bEnable);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetOnceWB(int m_iCameraID);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetAeTarget(int m_iCameraID, ref Byte pbyAeTarget);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetAeTarget(int m_iCameraID, Byte byAeTarget);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetAnalogGain(int m_iCameraID, ref float pfAnalogGain);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetAnalogGain(int m_iCameraID, float fAnalogGain);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetLightFrequency(int m_iCameraID, ref emDSLightFrequency pemFrequency);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetLightFrequency(int m_iCameraID, emDSLightFrequency emFrequency);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetFrameSpeed(int m_iCameraID, ref emDSFrameSpeed pemFrameSpeed);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetFrameSpeed(int m_iCameraID, emDSFrameSpeed emFrameSpeed);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSaveParameter(int m_iCameraID, emDSParameterTeam emTeam);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraReadParameter(int m_iCameraID, emDSParameterTeam emTeam);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetCurrentParameterTeam(int m_iCameraID, ref emDSParameterTeam pemTeam);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraLoadDefaultParameter(int m_iCameraID);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraCaptureFile(int m_iCameraID, string lpszFileName, Byte byFileType, Byte byQuality);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraStartRecordVideo(int m_iCameraID, string lpszRecordPath);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraStopRecordVideo(int m_iCameraID);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraGetRecordAVIQuality(int m_iCameraID, ref int piQuality);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetRecordAVIQuality(int m_iCameraID, int iQuality);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetRecordEncoder(int m_iCameraID, int iCodeType);
    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraSetImageSize(int iCameraID, Boolean bReserve,
                                            int iHOff,
                                            int iVOff,
                                            int iWidth,
                                            int iHeight,
                                            int bCapture);

    [DllImport("DVP_CAMSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern emDSCameraStatus CameraRecordFrame(int m_iCameraID, int pbyRGB, ref tDSFrameInfo psFrInfo);
}