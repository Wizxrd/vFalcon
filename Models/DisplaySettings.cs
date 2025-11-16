using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace vFalcon.Models;

public class DisplaySettings
{
    public int ZoomIndex { get; set; } = 50;
    public Coordinate? Center { get; set; }
}
