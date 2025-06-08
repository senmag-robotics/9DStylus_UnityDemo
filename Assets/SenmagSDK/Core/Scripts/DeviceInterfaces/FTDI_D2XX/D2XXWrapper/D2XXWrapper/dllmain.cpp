// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

#include <vector>
#include <mutex>
#include "ftd2xx.h"

struct FTDI_DeviceContext {
    FT_HANDLE handle = nullptr;
    std::mutex mutex;
};

// Global vector of device contexts (smart pointers)
std::vector<std::unique_ptr<FTDI_DeviceContext>> g_devices;
std::mutex g_deviceListMutex;  // Mutex for accessing g_devices vector

extern "C" {
    __declspec(dllexport) int GetNumberOfDevices()
    {
        DWORD numDevices = 0;
        FT_STATUS status = FT_CreateDeviceInfoList(&numDevices);
        if (status == FT_OK)
            return (int)numDevices;
        else
            return -1; // error
    }

    // Open a device by its index and set baud rate
    __declspec(dllexport) int OpenFTDIPort(int deviceID, int baudrate)
    {
        if (deviceID < 0)
            return -1;

        std::lock_guard<std::mutex> listLock(g_deviceListMutex);

        // Resize vector to hold enough devices
        while ((int)g_devices.size() <= deviceID)
            g_devices.push_back(std::make_unique<FTDI_DeviceContext>());

        auto& ctx = g_devices[deviceID];
        std::lock_guard<std::mutex> deviceLock(ctx->mutex);

        if (ctx->handle != nullptr)
            return deviceID; // already open

        FT_STATUS result = FT_Open(deviceID, &ctx->handle);
        if (result != FT_OK)
            return -1;

        FT_SetDataCharacteristics(ctx->handle, FT_BITS_8, FT_STOP_BITS_1, FT_PARITY_NONE);
        FT_SetFlowControl(ctx->handle, FT_FLOW_NONE, 0, 0);
        FT_SetLatencyTimer(ctx->handle, 1);
        FT_SetTimeouts(ctx->handle, 0, 0);



        

        return deviceID;
    }

    // Close a device by deviceID
    __declspec(dllexport) void CloseFTDIPort(int deviceID)
    {
        std::lock_guard<std::mutex> listLock(g_deviceListMutex);

        if (deviceID < 0 || (size_t)deviceID >= g_devices.size())
            return;

        auto& ctx = g_devices[deviceID];
        std::lock_guard<std::mutex> deviceLock(ctx->mutex);

        if (ctx->handle != nullptr)
        {
            FT_Close(ctx->handle);
            ctx->handle = nullptr;
        }
    }

    // Write data to device
    __declspec(dllexport) int WriteFTDI(int deviceID, const unsigned char* buffer, int length)
    {
        if (deviceID < 0 || (size_t)deviceID >= g_devices.size())
            return -1;

        auto& ctx = g_devices[deviceID];
        std::lock_guard<std::mutex> deviceLock(ctx->mutex);

        if (!ctx->handle)
            return -1;

        DWORD bytesWritten = 0;
        FT_STATUS status = FT_Write(ctx->handle, (void*)buffer, length, &bytesWritten);
        return (int)bytesWritten;
    }

    // Read data from device
    __declspec(dllexport) int ReadFTDI(int deviceID, unsigned char* buffer, int maxLength)
    {
        if (deviceID < 0 || (size_t)deviceID >= g_devices.size())
            return -1;

        auto& ctx = g_devices[deviceID];
        std::lock_guard<std::mutex> deviceLock(ctx->mutex);

        if (!ctx->handle)
            return -1;

        DWORD bytesRead = 0;
        FT_Read(ctx->handle, buffer, maxLength, &bytesRead);
        return (int)bytesRead;
    }

    // Get number of bytes available to read
    __declspec(dllexport) int GetBytesAvailable(int deviceID)
    {
        if (deviceID < 0 || (size_t)deviceID >= g_devices.size())
            return 0;

        auto& ctx = g_devices[deviceID];
        std::lock_guard<std::mutex> deviceLock(ctx->mutex);

        if (!ctx->handle)
            return 0;

        DWORD bytesAvailable = 0;
        FT_STATUS status = FT_GetQueueStatus(ctx->handle, &bytesAvailable);
        return (status == FT_OK) ? (int)bytesAvailable : 0;
    }

} // extern "C"
