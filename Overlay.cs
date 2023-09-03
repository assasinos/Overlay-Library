using System.Diagnostics;
using SkiaSharp;

namespace OverlayLibrary;

public class Overlay : IDisposable
{
    private Process _overlayedProcess;
    private GRContext _grContext;
    private SKSurface _skSurface;
    public SKCanvas _skCanvas;


    public Overlay(Process overlayedProcess)
    {
        _overlayedProcess = overlayedProcess;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}