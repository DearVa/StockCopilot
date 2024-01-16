using System;

namespace StockCopilot.Models;

public struct Range2D
{
    private double start, end;

    public double Start
    {
        get => start;
        set
        {
            if (value > end)
            {
                throw new ArgumentException("Start value cannot be greater than End value.");
            }
            start = value;
        }
    }

    public double End
    {
        get => end;
        set
        {
            if (value < start)
            {
                throw new ArgumentException("End value cannot be less than Start value.");
            }
            end = value;
        }
    }

    public double Span => end - start;

    public Range2D(double start, double end)
    {
        if (start > end)
        {
            throw new ArgumentException("Start value cannot be greater than End value.");
        }
        
        this.start = start;
        this.end = end;
    }
}