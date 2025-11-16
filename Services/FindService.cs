using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Engines;
using vFalcon.Utils;

namespace vFalcon.Services;

public class FindService
{
    public ScheduledFunction Scheduler { get; set; }
    public List<string> ThingsToFind { get; set; } = new();
    public bool Started { get; set; } = false;

    private int brightness = 50;

    public FindService()
    {
        Scheduler = new(Refresh, 1, true);
    }

    public void Start()
    {
        Started = true;
        Scheduler.Start();
    }

    public void Stop()
    {
        Started = false;
        Scheduler.Stop();
    }

    public void Dispose() => Scheduler.Dispose();

    public async Task Refresh()
    {
        if (!Started) return;
        if (brightness == 50)
        {
            brightness = 100;
            Colors.FindBySquare = SkiaEngine.ScaleColor(Colors.LimeGreen, brightness);
        }
        else
        {
            brightness = 50;
            Colors.FindBySquare = SkiaEngine.ScaleColor(Colors.LimeGreen, brightness);
        }
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
    }
}
