﻿using System;
using System.Collections.Generic;
using System.Linq;
using GocdTray.App.Abstractions;

namespace GocdTray.App
{
    public interface IDeviceManager
    {
        DeviceStatus Status { get; }
        Estate Estate { get; }
    }
}
