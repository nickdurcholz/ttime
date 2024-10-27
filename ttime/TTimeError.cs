using System;

namespace ttime;

public class TTimeError(string message) : Exception(message);